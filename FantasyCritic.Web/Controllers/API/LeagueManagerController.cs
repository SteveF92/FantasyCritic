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
using FantasyCritic.Lib.Services;
using FantasyCritic.Web.Models;
using FantasyCritic.Web.Models.Requests;
using FantasyCritic.Web.Models.Responses;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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

        public LeagueManagerController(FantasyCriticUserManager userManager, FantasyCriticService fantasyCriticService,
            IClock clock)
        {
            _userManager = userManager;
            _fantasyCriticService = fantasyCriticService;
            _clock = clock;
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
            await _fantasyCriticService.CreateLeague(domainRequest);

            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> ChangeLeagueName([FromBody] ChangeLeagueNameRequest request)
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
                return Unauthorized();
            }

            await _fantasyCriticService.ChangeLeagueName(league.Value, request.LeagueName);
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
                return Unauthorized();
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
                return Unauthorized();
            }

            var inviteUser = await _userManager.FindByEmailAsync(request.InviteEmail);
            if (inviteUser == null)
            {
                return BadRequest();
            }

            foreach (var year in league.Value.Years)
            {
                var leagueYear = await _fantasyCriticService.GetLeagueYear(league.Value.LeagueID, year);
                if (leagueYear.Value.PlayStatus.PlayStarted)
                {
                    return BadRequest("You can't add a player to a league that has already started playing");
                }
            }

            Result result = await _fantasyCriticService.InviteUser(league.Value, inviteUser);
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
                return Unauthorized();
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

            ClaimGameDomainRequest domainRequest = new ClaimGameDomainRequest(publisher.Value, request.GameName, request.CounterPick, request.ManagerOverride, masterGame);

            ClaimResult result = await _fantasyCriticService.ClaimGame(domainRequest);
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
        public async Task<IActionResult> StartPlay([FromBody] StartPlayRequest request)
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
                return Unauthorized();
            }

            var leagueYear = await _fantasyCriticService.GetLeagueYear(league.Value.LeagueID, request.Year);
            if (leagueYear.HasNoValue)
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

            await _fantasyCriticService.StartPlay(leagueYear.Value);

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
                return Unauthorized();
            }

            var leagueYear = await _fantasyCriticService.GetLeagueYear(league.Value.LeagueID, request.Year);
            if (leagueYear.HasNoValue)
            {
                return BadRequest();
            }

            if (!leagueYear.Value.PlayStatus.DraftFinished)
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
    }
}
