using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using FantasyCritic.Lib.Domain;
using FantasyCritic.Lib.Domain.Requests;
using FantasyCritic.Lib.Domain.Results;
using FantasyCritic.Lib.Domain.ScoringSystems;
using FantasyCritic.Lib.Enums;
using FantasyCritic.Lib.Interfaces;
using FantasyCritic.Lib.Services;
using FantasyCritic.Web.Extensions;
using FantasyCritic.Web.Hubs;
using FantasyCritic.Web.Models;
using FantasyCritic.Web.Models.Requests;
using FantasyCritic.Web.Models.Responses;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using NLog.Web.LayoutRenderers;
using NodaTime;

namespace FantasyCritic.Web.Controllers.API
{
    [Route("api/[controller]/[action]")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class LeagueManagerController : Controller
    {
        private readonly FantasyCriticUserManager _userManager;
        private readonly FantasyCriticService _fantasyCriticService;
        private readonly IClock _clock;
        private readonly IHubContext<UpdateHub> _hubcontext;
        private readonly IEmailSender _emailSender;

        public LeagueManagerController(FantasyCriticUserManager userManager, FantasyCriticService fantasyCriticService, IClock clock, IHubContext<UpdateHub> hubcontext, IEmailSender emailSender)
        {
            _userManager = userManager;
            _fantasyCriticService = fantasyCriticService;
            _clock = clock;
            _hubcontext = hubcontext;
            _emailSender = emailSender;
        }

        [HttpPost]
        public async Task<IActionResult> CreateLeague([FromBody] CreateLeagueRequest request)
        {
            var currentUser = await _userManager.FindByNameAsync(User.Identity.Name);
            if (currentUser == null)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            EligibilityLevel eligibilityLevel = await _fantasyCriticService.GetEligibilityLevel(request.MaximumEligibilityLevel);
            LeagueCreationParameters domainRequest = request.ToDomain(currentUser, eligibilityLevel);
            var league = await _fantasyCriticService.CreateLeague(domainRequest);
            if (league.IsFailure)
            {
                return BadRequest(league.Error);
            }

            return Ok();
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> AvailableYears(Guid id)
        {
            var currentUser = await _userManager.FindByNameAsync(User.Identity.Name);
            if (currentUser == null)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var league = await _fantasyCriticService.GetLeagueByID(id);
            if (league.HasNoValue)
            {
                return BadRequest();
            }

            if (league.Value.LeagueManager.UserID != currentUser.UserID)
            {
                return Forbid();
            }

            var supportedYears = await _fantasyCriticService.GetSupportedYears();
            var openYears = supportedYears.Where(x => x.OpenForCreation).Select(x => x.Year);
            var availableYears = openYears.Except(league.Value.Years);
            return Ok(availableYears);
        }

        [HttpPost]
        public async Task<IActionResult> AddNewLeagueYear([FromBody] NewLeagueYearRequest request)
        {
            var currentUser = await _userManager.FindByNameAsync(User.Identity.Name);
            var league = await _fantasyCriticService.GetLeagueByID(request.LeagueID);
            if (league.HasNoValue)
            {
                return BadRequest();
            }

            if (league.Value.LeagueManager.UserID != currentUser.UserID)
            {
                return Forbid();
            }

            if (league.Value.Years.Contains(request.Year))
            {
                return BadRequest();
            }

            var supportedYears = await _fantasyCriticService.GetSupportedYears();
            if (!supportedYears.Select(x => x.Year).Contains(request.Year))
            {
                return BadRequest();
            }

            if (!league.Value.Years.Any())
            {
                throw new Exception("League has no initial year.");
            }

            var mostRecentYear = league.Value.Years.Max();
            var mostRecentLeagueYear = await _fantasyCriticService.GetLeagueYear(league.Value.LeagueID, mostRecentYear);
            if (mostRecentLeagueYear.HasNoValue)
            {
                throw new Exception("Most recent league year could not be found");
            }

            await _fantasyCriticService.AddNewLeagueYear(league.Value, request.Year, mostRecentLeagueYear.Value.Options);

            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> ChangeLeagueOptions([FromBody] ChangeLeagueOptionsRequest request)
        {
            var currentUser = await _userManager.FindByNameAsync(User.Identity.Name);
            if (currentUser == null)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var league = await _fantasyCriticService.GetLeagueByID(request.LeagueID);
            if (league.HasNoValue)
            {
                return BadRequest();
            }

            if (league.Value.LeagueManager.UserID != currentUser.UserID)
            {
                return Forbid();
            }

            if (league.Value.TestLeague)
            {
                //Users can't change a test league to a non test.
                request.TestLeague = true;
            }

            await _fantasyCriticService.ChangeLeagueOptions(league.Value, request.LeagueName, request.PublicLeague, request.TestLeague);
            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> EditLeagueYearSettings([FromBody] LeagueYearSettingsViewModel request)
        {
            var currentUser = await _userManager.FindByNameAsync(User.Identity.Name);
            if (currentUser == null)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var league = await _fantasyCriticService.GetLeagueByID(request.LeagueID);
            if (league.HasNoValue)
            {
                return BadRequest();
            }

            var leagueYear = await _fantasyCriticService.GetLeagueYear(league.Value.LeagueID, request.Year);
            if (leagueYear.HasNoValue)
            {
                return BadRequest();
            }

            if (league.Value.LeagueManager.UserID != currentUser.UserID)
            {
                return Forbid();
            }

            EligibilityLevel eligibilityLevel = await _fantasyCriticService.GetEligibilityLevel(request.MaximumEligibilityLevel);
            EditLeagueYearParameters domainRequest = request.ToDomain(currentUser, eligibilityLevel);
            Result result = await _fantasyCriticService.EditLeague(league.Value, domainRequest);
            if (result.IsFailure)
            {
                return BadRequest(result.Error);
            }

            await _fantasyCriticService.UpdateFantasyPoints(leagueYear.Value);

            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> InvitePlayer([FromBody] InviteRequest request)
        {
            var currentUser = await _userManager.FindByNameAsync(User.Identity.Name);

            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var league = await _fantasyCriticService.GetLeagueByID(request.LeagueID);
            if (league.HasNoValue)
            {
                return BadRequest();
            }

            if (league.Value.LeagueManager.UserID != currentUser.UserID)
            {
                return Forbid();
            }

            foreach (var year in league.Value.Years)
            {
                var leagueYear = await _fantasyCriticService.GetLeagueYear(league.Value.LeagueID, year);
                if (leagueYear.Value.PlayStatus.PlayStarted)
                {
                    return BadRequest("You can't add a player to a league that has already started playing");
                }
            }

            string inviteEmail = request.InviteEmail.ToLower();

            Result result = await _fantasyCriticService.InviteUser(league.Value, inviteEmail);
            if (result.IsFailure)
            {
                return BadRequest(result.Error);
            }

            FantasyCriticUser inviteUser = await _userManager.FindByEmailAsync(inviteEmail);
            string baseURL = $"{Request.Scheme}://{Request.Host.Value}";
            if (inviteUser is null)
            {
                await _emailSender.SendSiteInviteEmail(inviteEmail, league.Value, baseURL);
            }
            else
            {
                await _emailSender.SendLeagueInviteEmail(inviteEmail, league.Value, baseURL);
            }

            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> RescindInvite([FromBody] InviteRequest request)
        {
            var currentUser = await _userManager.FindByNameAsync(User.Identity.Name);

            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var league = await _fantasyCriticService.GetLeagueByID(request.LeagueID);
            if (league.HasNoValue)
            {
                return BadRequest();
            }

            if (league.Value.LeagueManager.UserID != currentUser.UserID)
            {
                return Forbid();
            }

            Result result = await _fantasyCriticService.RescindInvite(league.Value, request.InviteEmail);
            if (result.IsFailure)
            {
                return BadRequest(result.Error);
            }

            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> RemovePlayer([FromBody] PlayerRemoveRequest request)
        {
            var currentUser = await _userManager.FindByNameAsync(User.Identity.Name);

            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var league = await _fantasyCriticService.GetLeagueByID(request.LeagueID);
            if (league.HasNoValue)
            {
                return BadRequest();
            }

            if (league.Value.LeagueManager.UserID != currentUser.UserID)
            {
                return Forbid();
            }

            if (league.Value.LeagueManager.UserID == request.UserID)
            {
                return BadRequest("Can't remove the league manager.");
            }

            var removeUser = await _userManager.FindByIdAsync(request.UserID.ToString());
            if (removeUser == null)
            {
                return BadRequest();
            }

            var playersInLeague = await _fantasyCriticService.GetUsersInLeague(league.Value);
            bool userIsInLeague = playersInLeague.Any(x => x.UserID == removeUser.UserID);
            if (!userIsInLeague)
            {
                return BadRequest("That user is not in that league.");
            }

            foreach (var year in league.Value.Years)
            {
                var leagueYear = await _fantasyCriticService.GetLeagueYear(league.Value.LeagueID, year);
                if (leagueYear.Value.PlayStatus.PlayStarted)
                {
                    return BadRequest("You can't remove a player from a league that has already started playing");
                }
            }

            await _fantasyCriticService.RemovePlayerFromLeague(league.Value, removeUser);

            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> ManagerClaimGame([FromBody] ClaimGameRequest request)
        {
            var currentUser = await _userManager.FindByNameAsync(User.Identity.Name);

            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var publisher = await _fantasyCriticService.GetPublisher(request.PublisherID);
            if (publisher.HasNoValue)
            {
                return BadRequest();
            }

            var league = await _fantasyCriticService.GetLeagueByID(publisher.Value.League.LeagueID);
            if (league.HasNoValue)
            {
                return BadRequest();
            }

            var leagueYear = await _fantasyCriticService.GetLeagueYear(league.Value.LeagueID, publisher.Value.Year);
            if (leagueYear.HasNoValue)
            {
                return BadRequest();
            }

            if (!leagueYear.Value.PlayStatus.DraftFinished)
            {
                return BadRequest("You can't manually manage games until you draft.");
            }

            if (league.Value.LeagueManager.UserID != currentUser.UserID)
            {
                return Forbid();
            }

            var claimUser = await _userManager.FindByIdAsync(publisher.Value.User.UserID.ToString());
            if (claimUser == null)
            {
                return BadRequest();
            }

            Maybe<MasterGame> masterGame = Maybe<MasterGame>.None;
            if (request.MasterGameID.HasValue)
            {
                masterGame = await _fantasyCriticService.GetMasterGame(request.MasterGameID.Value);
            }

            ClaimGameDomainRequest domainRequest = new ClaimGameDomainRequest(publisher.Value, request.GameName, request.CounterPick, request.ManagerOverride, masterGame, null, null);

            ClaimResult result = await _fantasyCriticService.ClaimGame(domainRequest, true, false);
            var viewModel = new ManagerClaimResultViewModel(result);

            await _fantasyCriticService.UpdateFantasyPoints(leagueYear.Value);
            return Ok(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> ManagerAssociateGame([FromBody] AssociateGameRequest request)
        {
            var currentUser = await _userManager.FindByNameAsync(User.Identity.Name);

            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var publisher = await _fantasyCriticService.GetPublisher(request.PublisherID);
            if (publisher.HasNoValue)
            {
                return BadRequest();
            }

            var league = await _fantasyCriticService.GetLeagueByID(publisher.Value.League.LeagueID);
            if (league.HasNoValue)
            {
                return BadRequest();
            }

            var leagueYear = await _fantasyCriticService.GetLeagueYear(league.Value.LeagueID, publisher.Value.Year);
            if (leagueYear.HasNoValue)
            {
                return BadRequest();
            }

            if (!leagueYear.Value.PlayStatus.DraftFinished)
            {
                return BadRequest("You cannot manually associate games until you draft.");
            }

            if (league.Value.LeagueManager.UserID != currentUser.UserID)
            {
                return Forbid();
            }

            var claimUser = await _userManager.FindByIdAsync(publisher.Value.User.UserID.ToString());
            if (claimUser == null)
            {
                return BadRequest();
            }

            var publisherGame = publisher.Value.PublisherGames.SingleOrDefault(x => x.PublisherGameID == request.PublisherGameID);
            if (publisherGame == null)
            {
                return BadRequest();
            }

            Maybe<MasterGame> masterGame = await _fantasyCriticService.GetMasterGame(request.MasterGameID);
            if (masterGame.HasNoValue)
            {
                return BadRequest();
            }

            AssociateGameDomainRequest domainRequest = new AssociateGameDomainRequest(publisher.Value, publisherGame, masterGame.Value, request.ManagerOverride);

            ClaimResult result = await _fantasyCriticService.AssociateGame(domainRequest);
            var viewModel = new ManagerClaimResultViewModel(result);

            await _fantasyCriticService.UpdateFantasyPoints(leagueYear.Value);

            return Ok(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> RemovePublisherGame([FromBody] GameRemoveRequest request)
        {
            var currentUser = await _userManager.FindByNameAsync(User.Identity.Name);

            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var publisher = await _fantasyCriticService.GetPublisher(request.PublisherID);
            if (publisher.HasNoValue)
            {
                return BadRequest();
            }

            var league = await _fantasyCriticService.GetLeagueByID(publisher.Value.League.LeagueID);
            if (league.HasNoValue)
            {
                return BadRequest();
            }

            if (league.Value.LeagueManager.UserID != currentUser.UserID)
            {
                return Forbid();
            }

            Maybe<PublisherGame> publisherGame = await _fantasyCriticService.GetPublisherGame(request.PublisherGameID);
            if (publisherGame.HasNoValue)
            {
                return BadRequest();
            }

            var leagueYear = await _fantasyCriticService.GetLeagueYear(league.Value.LeagueID, publisher.Value.Year);
            if (leagueYear.HasNoValue)
            {
                return BadRequest();
            }

            Result result = await _fantasyCriticService.RemovePublisherGame(leagueYear.Value, publisher.Value, publisherGame.Value);
            if (result.IsFailure)
            {
                return BadRequest(result.Error);
            }

            await _fantasyCriticService.UpdateFantasyPoints(leagueYear.Value);

            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> ManuallyScorePublisherGame([FromBody] ManualPublisherGameScoreRequest request)
        {
            var publisher = await _fantasyCriticService.GetPublisher(request.PublisherID);
            if (publisher.HasNoValue)
            {
                return BadRequest();
            }

            Maybe<LeagueYear> leagueYear = await _fantasyCriticService.GetLeagueYear(publisher.Value.League.LeagueID, publisher.Value.Year);
            if (leagueYear.HasNoValue)
            {
                return BadRequest();
            }
            if (!leagueYear.Value.PlayStatus.DraftFinished)
            {
                return BadRequest("You cannot manually score a game until after you draft.");
            }

            return await UpdateManualCriticScore(request.PublisherID, request.PublisherGameID, request.ManualCriticScore);
        }

        [HttpPost]
        public Task<IActionResult> RemoveManualPublisherGameScore([FromBody] ManualPublisherGameScoreRequest request)
        {
            return UpdateManualCriticScore(request.PublisherID, request.PublisherGameID, null);
        }

        private async Task<IActionResult> UpdateManualCriticScore(Guid publisherID, Guid publisherGameID, decimal? manualCriticScore)
        {
            var currentUser = await _userManager.FindByNameAsync(User.Identity.Name);

            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var publisher = await _fantasyCriticService.GetPublisher(publisherID);
            if (publisher.HasNoValue)
            {
                return BadRequest();
            }

            var league = await _fantasyCriticService.GetLeagueByID(publisher.Value.League.LeagueID);
            if (league.HasNoValue)
            {
                return BadRequest();
            }

            if (league.Value.LeagueManager.UserID != currentUser.UserID)
            {
                return Forbid();
            }

            Maybe<PublisherGame> publisherGame = await _fantasyCriticService.GetPublisherGame(publisherGameID);
            if (publisherGame.HasNoValue)
            {
                return BadRequest();
            }

            var leagueYear = await _fantasyCriticService.GetLeagueYear(league.Value.LeagueID, publisher.Value.Year);
            if (leagueYear.HasNoValue)
            {
                return BadRequest();
            }

            await _fantasyCriticService.ManuallyScoreGame(publisherGame.Value, manualCriticScore);
            await _fantasyCriticService.UpdateFantasyPoints(leagueYear.Value);

            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> StartDraft([FromBody] StartDraftRequest request)
        {
            var currentUser = await _userManager.FindByNameAsync(User.Identity.Name);

            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var league = await _fantasyCriticService.GetLeagueByID(request.LeagueID);
            if (league.HasNoValue)
            {
                return BadRequest();
            }

            if (league.Value.LeagueManager.UserID != currentUser.UserID)
            {
                return Forbid();
            }

            var leagueYear = await _fantasyCriticService.GetLeagueYear(league.Value.LeagueID, request.Year);
            if (leagueYear.HasNoValue)
            {
                return BadRequest();
            }

            if (!leagueYear.Value.PlayStatus.Equals(PlayStatus.NotStartedDraft))
            {
                return BadRequest();
            }

            var publishersInLeague = await _fantasyCriticService.GetPublishersInLeagueForYear(leagueYear.Value.League, leagueYear.Value.Year);
            var supportedYear = (await _fantasyCriticService.GetSupportedYears()).SingleOrDefault(x => x.Year == request.Year);
            if (supportedYear is null)
            {
                return BadRequest();
            }

            var usersInLeague = await _fantasyCriticService.GetUsersInLeague(league.Value);

            bool readyToPlay = _fantasyCriticService.LeagueIsReadyToPlay(supportedYear, publishersInLeague, usersInLeague);
            if (!readyToPlay)
            {
                return BadRequest();
            }

            await _fantasyCriticService.StartDraft(leagueYear.Value);
            await _hubcontext.Clients.Group(leagueYear.Value.GetGroupName).SendAsync("RefreshLeagueYear", leagueYear.Value);

            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> SetDraftOrder([FromBody] DraftOrderRequest request)
        {
            var currentUser = await _userManager.FindByNameAsync(User.Identity.Name);

            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var league = await _fantasyCriticService.GetLeagueByID(request.LeagueID);
            if (league.HasNoValue)
            {
                return BadRequest();
            }

            if (league.Value.LeagueManager.UserID != currentUser.UserID)
            {
                return Forbid();
            }

            var leagueYear = await _fantasyCriticService.GetLeagueYear(league.Value.LeagueID, request.Year);
            if (leagueYear.HasNoValue)
            {
                return BadRequest();
            }

            if (leagueYear.Value.PlayStatus.PlayStarted)
            {
                return BadRequest();
            }

            var usersInLeague = await _fantasyCriticService.GetUsersInLeague(league.Value);
            var publishersInLeague = await _fantasyCriticService.GetPublishersInLeagueForYear(leagueYear.Value.League, leagueYear.Value.Year);
            var readyToSetDraftOrder = _fantasyCriticService.LeagueIsReadyToSetDraftOrder(publishersInLeague, usersInLeague);
            if (!readyToSetDraftOrder)
            {
                return BadRequest();
            }

            List<KeyValuePair<Publisher, int>> draftPositions = new List<KeyValuePair<Publisher, int>>();
            for (var index = 0; index < request.PublisherDraftPositions.Count; index++)
            {
                var requestPublisher = request.PublisherDraftPositions[index];
                var publisher = publishersInLeague.SingleOrDefault(x => x.PublisherID == requestPublisher);
                if (publisher is null)
                {
                    return BadRequest();
                }

                draftPositions.Add(new KeyValuePair<Publisher, int>(publisher, index + 1));
            }

            var result = await _fantasyCriticService.SetDraftOrder(leagueYear.Value, draftPositions);
            if (result.IsFailure)
            {
                return BadRequest(result.Error);
            }

            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> ManagerDraftGame([FromBody] ManagerDraftGameRequest request)
        {
            var currentUser = await _userManager.FindByNameAsync(User.Identity.Name);

            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var publisher = await _fantasyCriticService.GetPublisher(request.PublisherID);
            if (publisher.HasNoValue)
            {
                return BadRequest();
            }

            var league = await _fantasyCriticService.GetLeagueByID(publisher.Value.League.LeagueID);
            if (league.HasNoValue)
            {
                return BadRequest();
            }

            if (league.Value.LeagueManager.UserID != currentUser.UserID)
            {
                return Forbid();
            }

            var leagueYear = await _fantasyCriticService.GetLeagueYear(league.Value.LeagueID, publisher.Value.Year);
            if (leagueYear.HasNoValue)
            {
                return BadRequest();
            }

            if (!leagueYear.Value.PlayStatus.DraftIsActive)
            {
                return BadRequest("You can't draft a game if the draft isn't active.");
            }

            var publishersInLeague = await _fantasyCriticService.GetPublishersInLeagueForYear(leagueYear.Value.League, leagueYear.Value.Year);
            var nextPublisher = _fantasyCriticService.GetNextDraftPublisher(leagueYear.Value, publishersInLeague);
            if (nextPublisher.HasNoValue)
            {
                return BadRequest("There are no spots open to draft.");
            }

            if (!nextPublisher.Value.Equals(publisher.Value))
            {
                return BadRequest("That publisher is not next up for drafting.");
            }

            Maybe<MasterGame> masterGame = Maybe<MasterGame>.None;
            if (request.MasterGameID.HasValue)
            {
                masterGame = await _fantasyCriticService.GetMasterGame(request.MasterGameID.Value);
            }

            int? publisherPosition = null;
            int? overallPosition = null;
            var draftPhase = await _fantasyCriticService.GetDraftPhase(leagueYear.Value);
            if (draftPhase.Equals(DraftPhase.StandardGames))
            {
                publisherPosition = publisher.Value.PublisherGames.Count(x => !x.CounterPick) + 1;
                overallPosition = publishersInLeague.SelectMany(x => x.PublisherGames).Count(x => !x.CounterPick) + 1;

                if (request.CounterPick)
                {
                    return BadRequest("Not drafting counterPicks now.");
                }
            }

            if (draftPhase.Equals(DraftPhase.CounterPicks))
            {
                if (!request.CounterPick)
                {
                    return BadRequest("Not drafting standard games now.");
                }
            }

            ClaimGameDomainRequest domainRequest = new ClaimGameDomainRequest(publisher.Value, request.GameName, request.CounterPick, request.ManagerOverride, masterGame, publisherPosition, overallPosition);

            ClaimResult result = await _fantasyCriticService.ClaimGame(domainRequest, true, true);
            bool draftCompleted = await _fantasyCriticService.CompleteDraft(leagueYear.Value);
            var viewModel = new ManagerClaimResultViewModel(result);
            await _hubcontext.Clients.Group(leagueYear.Value.GetGroupName).SendAsync("RefreshLeagueYear", leagueYear.Value);

            if (draftCompleted)
            {
                await _hubcontext.Clients.Group(leagueYear.Value.GetGroupName).SendAsync("DraftFinished", leagueYear.Value);
            }

            return Ok(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> SetDraftPause([FromBody] DraftPauseRequest request)
        {
            var currentUser = await _userManager.FindByNameAsync(User.Identity.Name);

            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var league = await _fantasyCriticService.GetLeagueByID(request.LeagueID);
            if (league.HasNoValue)
            {
                return BadRequest();
            }

            if (league.Value.LeagueManager.UserID != currentUser.UserID)
            {
                return Forbid();
            }

            var leagueYear = await _fantasyCriticService.GetLeagueYear(league.Value.LeagueID, request.Year);
            if (leagueYear.HasNoValue)
            {
                return BadRequest();
            }

            if (request.Pause)
            {
                if (!leagueYear.Value.PlayStatus.Equals(PlayStatus.Drafting))
                {
                    return BadRequest();
                }
            }
            if (!request.Pause)
            {
                if (!leagueYear.Value.PlayStatus.Equals(PlayStatus.DraftPaused))
                {
                    return BadRequest();
                }
            }

            await _fantasyCriticService.SetDraftPause(leagueYear.Value, request.Pause);
            await _hubcontext.Clients.Group(leagueYear.Value.GetGroupName).SendAsync("RefreshLeagueYear", leagueYear.Value);

            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> UndoLastDraftAction([FromBody] UndoLastDraftActionRequest request)
        {
            var currentUser = await _userManager.FindByNameAsync(User.Identity.Name);

            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var league = await _fantasyCriticService.GetLeagueByID(request.LeagueID);
            if (league.HasNoValue)
            {
                return BadRequest();
            }

            if (league.Value.LeagueManager.UserID != currentUser.UserID)
            {
                return Forbid();
            }

            var leagueYear = await _fantasyCriticService.GetLeagueYear(league.Value.LeagueID, request.Year);
            if (leagueYear.HasNoValue)
            {
                return BadRequest();
            }

            if (!leagueYear.Value.PlayStatus.Equals(PlayStatus.DraftPaused))
            {
                return BadRequest("Can only undo when the draft is paused.");
            }

            var publishers = await _fantasyCriticService.GetPublishersInLeagueForYear(league.Value, leagueYear.Value.Year);
            bool hasGames = publishers.SelectMany(x => x.PublisherGames).Any();
            if (!hasGames)
            {
                return BadRequest("Can't undo a drafted game if no games have been drafted.");
            }

            await _fantasyCriticService.UndoLastDraftAction(leagueYear.Value, publishers);
            await _hubcontext.Clients.Group(leagueYear.Value.GetGroupName).SendAsync("RefreshLeagueYear", leagueYear.Value);

            return Ok();
        }
    }
}
