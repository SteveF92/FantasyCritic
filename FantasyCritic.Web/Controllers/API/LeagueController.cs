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
using FantasyCritic.Lib.Extensions;
using FantasyCritic.Lib.Services;
using FantasyCritic.Lib.Utilities;
using FantasyCritic.Web.Hubs;
using FantasyCritic.Web.Models;
using FantasyCritic.Web.Models.Requests;
using FantasyCritic.Web.Models.Requests.League;
using FantasyCritic.Web.Models.Requests.Shared;
using FantasyCritic.Web.Models.Responses;
using FantasyCritic.Web.Models.RoundTrip;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.ResponseCaching.Internal;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.Extensions.Logging;
using NodaTime;

namespace FantasyCritic.Web.Controllers.API
{
    [Route("api/[controller]/[action]")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class LeagueController : Controller
    {
        private readonly FantasyCriticUserManager _userManager;
        private readonly FantasyCriticService _fantasyCriticService;
        private readonly InterLeagueService _interLeagueService;
        private readonly LeagueMemberService _leagueMemberService;
        private readonly DraftService _draftService;
        private readonly GameSearchingService _gameSearchingService;
        private readonly PublisherService _publisherService;
        private readonly IClock _clock;
        private readonly IHubContext<UpdateHub> _hubContext;
        private readonly ILogger<LeagueController> _logger;

        public LeagueController(FantasyCriticUserManager userManager, FantasyCriticService fantasyCriticService, InterLeagueService interLeagueService,
            LeagueMemberService leagueMemberService, DraftService draftService, GameSearchingService gameSearchingService, PublisherService publisherService, IClock clock,
            IHubContext<UpdateHub> hubContext, ILogger<LeagueController> logger)
        {
            _userManager = userManager;
            _fantasyCriticService = fantasyCriticService;
            _interLeagueService = interLeagueService;
            _leagueMemberService = leagueMemberService;
            _draftService = draftService;
            _gameSearchingService = gameSearchingService;
            _publisherService = publisherService;
            _clock = clock;
            _hubContext = hubContext;
            _logger = logger;
        }

        public async Task<IActionResult> LeagueOptions()
        {
            var supportedYears = await _interLeagueService.GetSupportedYears();
            var openYears = supportedYears.Where(x => x.OpenForCreation && !x.Finished);
            var currentUser = await _userManager.FindByNameAsync(User.Identity.Name);
            if (currentUser != null)
            {
                var userIsBetaUser = await _userManager.IsInRoleAsync(currentUser, "BetaTester");
                if (userIsBetaUser)
                {
                    var betaYears = supportedYears.Where(x => x.OpenForBetaUsers);
                    openYears = openYears.Concat(betaYears).Distinct();
                }
            }
            
            var openYearInts = openYears.Select(x => x.Year);
            IReadOnlyList<EligibilityLevel> eligibilityLevels = await _interLeagueService.GetEligibilityLevels();
            LeagueOptionsViewModel viewModel = new LeagueOptionsViewModel(openYearInts, DraftSystem.GetAllPossibleValues(),
                PickupSystem.GetAllPossibleValues(), ScoringSystem.GetAllPossibleValues(), eligibilityLevels);

            return Ok(viewModel);
        }

        public async Task<IActionResult> MyLeagues()
        {
            var currentUser = await _userManager.FindByNameAsync(User.Identity.Name);
            IReadOnlyList<League> myLeagues = await _leagueMemberService.GetLeaguesForUser(currentUser);

            List<LeagueViewModel> viewModels = new List<LeagueViewModel>();
            foreach (var league in myLeagues)
            {
                bool isManager = (league.LeagueManager.UserID == currentUser.UserID);
                viewModels.Add(new LeagueViewModel(league, isManager, true, false));
            }

            return Ok(viewModels);
        }

        public async Task<IActionResult> FollowedLeagues()
        {
            var currentUser = await _userManager.FindByNameAsync(User.Identity.Name);
            IReadOnlyList<League> leaguesFollowing = await _fantasyCriticService.GetFollowedLeagues(currentUser);

            List<LeagueViewModel> viewModels = new List<LeagueViewModel>();
            foreach (var league in leaguesFollowing)
            {
                viewModels.Add(new LeagueViewModel(league, false, false, true));
            }

            return Ok(viewModels);
        }

        public async Task<IActionResult> MyInvites()
        {
            var currentUser = await _userManager.FindByNameAsync(User.Identity.Name);
            var invitedLeagues = await _leagueMemberService.GetLeagueInvites(currentUser);
            var viewModels = invitedLeagues.Select(x => LeagueInviteViewModel.CreateWithDisplayName(x, currentUser));
            return Ok(viewModels);
        }

        [AllowAnonymous]
        [HttpGet("{year}")]
        public async Task<IActionResult> PublicLeagues(int year, int? count)
        {
            IReadOnlyList<LeagueYear> publicLeagueYears = await _fantasyCriticService.GetPublicLeagueYears(year);

            List<PublicLeagueYearViewModel> viewModels = new List<PublicLeagueYearViewModel>();
            foreach (var leagueYear in publicLeagueYears)
            {
                viewModels.Add(new PublicLeagueYearViewModel(leagueYear));
            }

            if (count.HasValue)
            {
                viewModels = viewModels.Take(count.Value).ToList();
            }

            return Ok(viewModels);
        }

        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetLeague(Guid id, Guid? inviteCode)
        {
            Maybe<League> league = await _fantasyCriticService.GetLeagueByID(id);
            if (league.HasNoValue)
            {
                return NotFound();
            }

            FantasyCriticUser currentUser = null;
            if (!string.IsNullOrWhiteSpace(User.Identity.Name))
            {
                currentUser = await _userManager.FindByNameAsync(User.Identity.Name);
            }

            var playersInLeague = await _leagueMemberService.GetUsersWithRemoveStatus(league.Value);
            var inviteesToLeague = await _leagueMemberService.GetOutstandingInvitees(league.Value);
            var leagueFollowers = await _fantasyCriticService.GetLeagueFollowers(league.Value);

            bool userIsInLeague = false;
            bool userIsInvitedToLeague = false;
            bool isManager = false;
            bool userIsFollowingLeague = false;
            Maybe<LeagueInvite> leagueInvite = Maybe<LeagueInvite>.None;
            if (currentUser != null)
            {
                userIsInLeague = playersInLeague.Any(x => x.User.UserID == currentUser.UserID);
                userIsInvitedToLeague = inviteesToLeague.UserIsInvited(currentUser.EmailAddress);
                isManager = (league.Value.LeagueManager.UserID == currentUser.UserID);
                userIsFollowingLeague = leagueFollowers.Any(x => x.UserID == currentUser.UserID);
                leagueInvite = inviteesToLeague.GetMatchingInvite(currentUser.EmailAddress);
            }

            bool inviteCodeIsValid = false;
            if (inviteCode.HasValue)
            {
                var activeLinks = await _leagueMemberService.GetActiveInviteLinks(league.Value);
                inviteCodeIsValid = activeLinks.Any(x => x.Active && x.InviteCode == inviteCode.Value);
            }

            if (!userIsInLeague && !userIsInvitedToLeague && !league.Value.PublicLeague && !inviteCodeIsValid)
            {
                return Forbid();
            }

            bool hasBeenStarted = await _fantasyCriticService.LeagueHasBeenStarted(league.Value.LeagueID);
            bool neverStarted = !hasBeenStarted;

            var leagueViewModel = new LeagueViewModel(league.Value, isManager, playersInLeague, leagueInvite, currentUser, neverStarted, userIsInLeague, userIsFollowingLeague);
            return Ok(leagueViewModel);
        }

        [AllowAnonymous]
        public async Task<IActionResult> GetLeagueYear(Guid leagueID, int year, Guid? inviteCode)
        {
            Maybe<LeagueYear> leagueYear = await _fantasyCriticService.GetLeagueYear(leagueID, year);
            if (leagueYear.HasNoValue)
            {
                throw new Exception("Something went really wrong, no options are set up for this league.");
            }

            FantasyCriticUser currentUser = null;
            if (!string.IsNullOrWhiteSpace(User.Identity.Name))
            {
                currentUser = await _userManager.FindByNameAsync(User.Identity.Name);
            }

            var activeUsers = await _leagueMemberService.GetActivePlayersForLeagueYear(leagueYear.Value.League, year);
            var inviteesToLeague = await _leagueMemberService.GetOutstandingInvitees(leagueYear.Value.League);

            bool userIsInLeague = false;
            bool userIsInvitedToLeague = false;
            bool isManager = false;
            if (currentUser != null)
            {
                userIsInLeague = activeUsers.Any(x => x.UserID == currentUser.UserID);
                userIsInvitedToLeague = inviteesToLeague.UserIsInvited(currentUser.EmailAddress);
                isManager = (leagueYear.Value.League.LeagueManager.UserID == currentUser.UserID);
            }

            bool inviteCodeIsValid = false;
            if (inviteCode.HasValue)
            {
                var activeLinks = await _leagueMemberService.GetActiveInviteLinks(leagueYear.Value.League);
                inviteCodeIsValid = activeLinks.Any(x => x.Active && x.InviteCode == inviteCode.Value);
            }

            if (!userIsInLeague && !userIsInvitedToLeague && !leagueYear.Value.League.PublicLeague && !inviteCodeIsValid)
            {
                return Forbid();
            }

            var publishersInLeague = await _publisherService.GetPublishersInLeagueForYear(leagueYear.Value, activeUsers);
            var supportedYear = (await _interLeagueService.GetSupportedYears()).SingleOrDefault(x => x.Year == year);
            if (supportedYear is null)
            {
                return BadRequest();
            }

            StartDraftResult startDraftResult = await _draftService.GetStartDraftResult(leagueYear.Value, publishersInLeague, activeUsers);
            Maybe<Publisher> nextDraftPublisher = _draftService.GetNextDraftPublisher(leagueYear.Value, publishersInLeague);
            DraftPhase draftPhase = _draftService.GetDraftPhase(leagueYear.Value, publishersInLeague);

            Maybe<Publisher> userPublisher = Maybe<Publisher>.None;
            if (userIsInLeague)
            {
                userPublisher = publishersInLeague.SingleOrDefault(x => x.User.UserID == currentUser.UserID);
            }

            IReadOnlyList<PublisherGame> availableCounterPicks = new List<PublisherGame>();
            if (nextDraftPublisher.HasValue)
            {
                availableCounterPicks = _draftService.GetAvailableCounterPicks(leagueYear.Value, nextDraftPublisher.Value, publishersInLeague);
            }

            SystemWideValues systemWideValues = await _interLeagueService.GetSystemWideValues();

            var leagueViewModel = new LeagueYearViewModel(leagueYear.Value, supportedYear, publishersInLeague, userPublisher, _clock,
                leagueYear.Value.PlayStatus, startDraftResult, activeUsers, nextDraftPublisher, draftPhase, availableCounterPicks,
                leagueYear.Value.Options, systemWideValues, inviteesToLeague, userIsInLeague, userIsInvitedToLeague, isManager, currentUser);
            return Ok(leagueViewModel);
        }

        [AllowAnonymous]
        public async Task<IActionResult> GetLeagueActions(Guid leagueID, int year)
        {
            Maybe<LeagueYear> leagueYear = await _fantasyCriticService.GetLeagueYear(leagueID, year);
            if (leagueYear.HasNoValue)
            {
                throw new Exception("Something went really wrong, no options are set up for this league.");
            }

            FantasyCriticUser currentUser = null;
            if (!string.IsNullOrWhiteSpace(User.Identity.Name))
            {
                currentUser = await _userManager.FindByNameAsync(User.Identity.Name);
            }

            var inviteesToLeague = await _leagueMemberService.GetOutstandingInvitees(leagueYear.Value.League);

            bool userIsInLeague = false;
            bool userIsInvitedToLeague = false;
            if (currentUser != null)
            {
                var usersInLeague = await _leagueMemberService.GetUsersInLeague(leagueYear.Value.League);
                userIsInLeague = usersInLeague.Any(x => x.UserID == currentUser.UserID);
                userIsInvitedToLeague = inviteesToLeague.UserIsInvited(currentUser.EmailAddress);
            }

            if (!userIsInLeague && !userIsInvitedToLeague && !leagueYear.Value.League.PublicLeague)
            {
                return Forbid();
            }

            var leagueActions = await _fantasyCriticService.GetLeagueActions(leagueYear.Value);

            var viewModels = leagueActions.Select(x => new LeagueActionViewModel(x, _clock));
            viewModels = viewModels.OrderByDescending(x => x.Timestamp);
            return Ok(viewModels);
        }

        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetPublisher(Guid id)
        {
            Maybe<Publisher> publisher = await _publisherService.GetPublisher(id);
            if (publisher.HasNoValue)
            {
                return NotFound();
            }

            FantasyCriticUser currentUser = null;
            if (!string.IsNullOrWhiteSpace(User.Identity.Name))
            {
                currentUser = await _userManager.FindByNameAsync(User.Identity.Name);
            }

            var playersInLeague = await _leagueMemberService.GetUsersInLeague(publisher.Value.LeagueYear.League);
            var inviteesToLeague = await _leagueMemberService.GetOutstandingInvitees(publisher.Value.LeagueYear.League);

            bool userIsInLeague = false;
            bool userIsInvitedToLeague = false;
            if (currentUser != null)
            {
                userIsInLeague = playersInLeague.Any(x => x.UserID == currentUser.UserID);
                userIsInvitedToLeague = inviteesToLeague.UserIsInvited(currentUser.EmailAddress);
            }

            if (!userIsInLeague && !publisher.Value.LeagueYear.League.PublicLeague)
            {
                return Forbid();
            }

            var requstedPlayerIsInLeague = playersInLeague.Any(x => x.UserID == publisher.Value.User.UserID);
            if (!requstedPlayerIsInLeague)
            {
                return BadRequest("Requested player is not in requested league.");
            }


            SystemWideValues systemWideValues = await _interLeagueService.GetSystemWideValues();
            var supportedYear = (await _interLeagueService.GetSupportedYears()).SingleOrDefault(x => x.Year == publisher.Value.LeagueYear.Year);

            var publisherViewModel = new PublisherViewModel(publisher.Value, _clock, userIsInLeague, userIsInvitedToLeague, systemWideValues, supportedYear.Finished);
            return Ok(publisherViewModel);
        }

        [AllowAnonymous]
        public async Task<IActionResult> GetLeagueYearOptions(Guid leagueID, int year)
        {
            Maybe<League> league = await _fantasyCriticService.GetLeagueByID(leagueID);
            if (league.HasNoValue)
            {
                return NotFound();
            }

            Maybe<LeagueYear> leagueYear = await _fantasyCriticService.GetLeagueYear(leagueID, year);
            if (leagueYear.HasNoValue)
            {
                throw new Exception("Something went really wrong, no options are set up for this league.");
            }

            var inviteesToLeague = await _leagueMemberService.GetOutstandingInvitees(leagueYear.Value.League);

            bool userIsInLeague = false;
            bool userIsInvitedToLeague = false;
            FantasyCriticUser currentUser = null;
            if (!string.IsNullOrWhiteSpace(User.Identity.Name))
            {
                currentUser = await _userManager.FindByNameAsync(User.Identity.Name);
            }
            if (currentUser != null)
            {
                var usersInLeague = await _leagueMemberService.GetUsersInLeague(leagueYear.Value.League);
                userIsInLeague = usersInLeague.Any(x => x.UserID == currentUser.UserID);
                userIsInvitedToLeague = inviteesToLeague.UserIsInvited(currentUser.EmailAddress);
            }

            if (!userIsInLeague && !userIsInvitedToLeague && !leagueYear.Value.League.PublicLeague)
            {
                return Forbid();
            }

            var leagueViewModel = new LeagueYearSettingsViewModel(leagueYear.Value);
            return Ok(leagueViewModel);
        }

        [HttpPost]
        public async Task<IActionResult> AcceptInvite([FromBody] AcceptInviteRequest request)
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

            if (!currentUser.EmailConfirmed)
            {
                return BadRequest();
            }

            Result result = await _leagueMemberService.AcceptInvite(league.Value, currentUser);
            if (result.IsFailure)
            {
                return BadRequest(result.Error);
            }

            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> JoinWithInviteLink([FromBody] JoinWithInviteLinkRequest request)
        {
            var currentUser = await _userManager.FindByNameAsync(User.Identity.Name);

            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var inviteLink = await _leagueMemberService.GetInviteLinkByInviteCode(request.InviteCode);
            if (inviteLink.HasNoValue)
            {
                return BadRequest();
            }

            if (inviteLink.Value.League.LeagueID != request.LeagueID)
            {
                return BadRequest();
            }

            Result result = await _leagueMemberService.AcceptInviteLink(inviteLink.Value, currentUser);
            if (result.IsFailure)
            {
                return BadRequest(result.Error);
            }

            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> CreatePublisher([FromBody] CreatePublisherRequest request)
        {
            var league = await _fantasyCriticService.GetLeagueByID(request.LeagueID);
            if (league.HasNoValue)
            {
                return BadRequest();
            }

            var currentUser = await _userManager.FindByNameAsync(User.Identity.Name);
            
            bool userIsActive = await _leagueMemberService.UserIsActiveInLeagueYear(league.Value, request.Year, currentUser);
            if (!userIsActive)
            {
                return Forbid();
            }

            Maybe<LeagueYear> leagueYear = await _fantasyCriticService.GetLeagueYear(league.Value.LeagueID, request.Year);
            if (leagueYear.HasNoValue)
            {
                return BadRequest();
            }

            if (leagueYear.Value.PlayStatus.PlayStarted)
            {
                return BadRequest();
            }

            var currentPublishers = await _publisherService.GetPublishersInLeagueForYear(leagueYear.Value);
            var publisherForUser = currentPublishers.SingleOrDefault(x => x.User.UserID == currentUser.UserID);
            if (publisherForUser != null)
            {
                return BadRequest("You have already created a publisher for this this league/year.");
            }

            await _publisherService.CreatePublisher(leagueYear.Value, currentUser, request.PublisherName, currentPublishers);
            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> ChangePublisherName([FromBody] ChangePublisherNameRequest request)
        {
            var publisher = await _publisherService.GetPublisher(request.PublisherID);
            if (publisher.HasNoValue)
            {
                return BadRequest();
            }

            var currentUser = await _userManager.FindByNameAsync(User.Identity.Name);
            bool userIsInLeague = await _leagueMemberService.UserIsInLeague(publisher.Value.LeagueYear.League, currentUser);
            if (!userIsInLeague)
            {
                return Forbid();
            }

            if (publisher.Value.User.UserID != currentUser.UserID)
            {
                return Forbid();
            }

            await _fantasyCriticService.ChangePublisherName(publisher.Value, request.PublisherName);
            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> DeclineInvite([FromBody] DeleteInviteRequest request)
        {
            var currentUser = await _userManager.FindByNameAsync(User.Identity.Name);

            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            Maybe<LeagueInvite> invite = await _leagueMemberService.GetInvite(request.InviteID);
            if (invite.HasNoValue)
            {
                return BadRequest();
            }

            if (!string.Equals(invite.Value.EmailAddress, currentUser.EmailAddress, StringComparison.OrdinalIgnoreCase))
            {
                return Forbid();
            }

            await _leagueMemberService.DeleteInvite(invite.Value);
            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> MakePickupBid([FromBody] PickupBidRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var systemWideSettings = await _interLeagueService.GetSystemWideSettings();
            if (systemWideSettings.BidProcessingMode)
            {
                return BadRequest();
            }

            var publisher = await _publisherService.GetPublisher(request.PublisherID);
            if (publisher.HasNoValue)
            {
                return BadRequest();
            }

            var supportedYear = (await _interLeagueService.GetSupportedYears()).SingleOrDefault(x => x.Year == publisher.Value.LeagueYear.Year);
            if (supportedYear is null)
            {
                return BadRequest();
            }

            if (supportedYear.Finished)
            {
                return BadRequest("That year is already finished");
            }

            Maybe<LeagueYear> leagueYear = await _fantasyCriticService.GetLeagueYear(publisher.Value.LeagueYear.League.LeagueID, publisher.Value.LeagueYear.Year);
            if (leagueYear.HasNoValue)
            {
                return BadRequest();
            }
            if (!leagueYear.Value.PlayStatus.DraftFinished)
            {
                return BadRequest("Draft has not finished for that year.");
            }

            var currentUser = await _userManager.FindByNameAsync(User.Identity.Name);
            bool userIsInLeague = await _leagueMemberService.UserIsInLeague(publisher.Value.LeagueYear.League, currentUser);
            bool userIsPublisher = (currentUser.UserID == publisher.Value.User.UserID);
            if (!userIsInLeague || !userIsPublisher)
            {
                return Forbid();
            }

            var masterGame = await _interLeagueService.GetMasterGame(request.MasterGameID);
            if (masterGame.HasNoValue)
            {
                return BadRequest("That master game does not exist.");
            }
            
            ClaimResult bidResult = await _fantasyCriticService.MakePickupBid(publisher.Value, masterGame.Value, request.BidAmount);
            var viewModel = new PickupBidResultViewModel(bidResult);

            return Ok(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> DeletePickupBid([FromBody] PickupBidDeleteRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var systemWideSettings = await _interLeagueService.GetSystemWideSettings();
            if (systemWideSettings.BidProcessingMode)
            {
                return BadRequest();
            }

            var maybeBid = await _fantasyCriticService.GetPickupBid(request.BidID);
            if (maybeBid.HasNoValue)
            {
                return BadRequest("That bid does not exist.");
            }

            var publisher = maybeBid.Value.Publisher;
            var currentUser = await _userManager.FindByNameAsync(User.Identity.Name);
            bool userIsInLeague = await _leagueMemberService.UserIsInLeague(publisher.LeagueYear.League, currentUser);
            bool userIsPublisher = (currentUser.UserID == publisher.User.UserID);
            if (!userIsInLeague || !userIsPublisher)
            {
                return Forbid();
            }

            PickupBid bid = maybeBid.Value;
            Result result = await _fantasyCriticService.RemovePickupBid(bid);
            if (result.IsFailure)
            {
                return BadRequest(result.Error);
            }

            return Ok();
        }

        [HttpGet("{publisherID}")]
        public async Task<IActionResult> CurrentBids(Guid publisherID)
        {
            Maybe<Publisher> publisher = await _publisherService.GetPublisher(publisherID);
            if (publisher.HasNoValue)
            {
                return NotFound();
            }

            var currentUser = await _userManager.FindByNameAsync(User.Identity.Name);
            if (currentUser.UserID != publisher.Value.User.UserID)
            {
                return Forbid();
            }

            var bids = await _fantasyCriticService.GetActiveAcquistitionBids(publisher.Value);

            var viewModels = bids.Select(x => new PickupBidViewModel(x, _clock)).OrderBy(x => x.Priority);
            return Ok(viewModels);
        }

        [HttpPost]
        public async Task<IActionResult> SetBidPriorities([FromBody] BidPriorityOrderRequest request)
        {
            Maybe<Publisher> publisher = await _publisherService.GetPublisher(request.PublisherID);
            if (publisher.HasNoValue)
            {
                return NotFound();
            }

            var currentUser = await _userManager.FindByNameAsync(User.Identity.Name);
            if (currentUser.UserID != publisher.Value.User.UserID)
            {
                return Forbid();
            }

            var activeBids = await _fantasyCriticService.GetActiveAcquistitionBids(publisher.Value);

            if (activeBids.Count != request.BidPriorities.Count)
            {
                return BadRequest();
            }

            List<KeyValuePair<PickupBid, int>> bidPriorities = new List<KeyValuePair<PickupBid, int>>();
            for (var index = 0; index < request.BidPriorities.Count; index++)
            {
                var requestBid = request.BidPriorities[index];
                var activeBid = activeBids.SingleOrDefault(x => x.BidID == requestBid);
                if (activeBid is null)
                {
                    return BadRequest();
                }

                bidPriorities.Add(new KeyValuePair<PickupBid, int>(activeBid, index + 1));
            }

            Result result = await _publisherService.SetBidPriorityOrder(bidPriorities);
            if (result.IsFailure)
            {
                return BadRequest(result.Error);
            }

            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> DraftGame([FromBody] DraftGameRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var publisher = await _publisherService.GetPublisher(request.PublisherID);
            if (publisher.HasNoValue)
            {
                return BadRequest();
            }

            var currentUser = await _userManager.FindByNameAsync(User.Identity.Name);
            if (currentUser.UserID != publisher.Value.User.UserID)
            {
                return Forbid();
            }

            var league = await _fantasyCriticService.GetLeagueByID(publisher.Value.LeagueYear.League.LeagueID);
            if (league.HasNoValue)
            {
                return BadRequest();
            }

            var leagueYear = await _fantasyCriticService.GetLeagueYear(league.Value.LeagueID, publisher.Value.LeagueYear.Year);
            if (leagueYear.HasNoValue)
            {
                return BadRequest();
            }

            if (!leagueYear.Value.PlayStatus.DraftIsActive)
            {
                return BadRequest("You can't draft a game if the draft isn't active.");
            }

            var publishersInLeague = await _publisherService.GetPublishersInLeagueForYear(leagueYear.Value);
            var nextPublisher = _draftService.GetNextDraftPublisher(leagueYear.Value, publishersInLeague);
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
                masterGame = await _interLeagueService.GetMasterGame(request.MasterGameID.Value);
            }

            int? publisherPosition = null;
            int? overallPosition = null;
            var draftPhase = _draftService.GetDraftPhase(leagueYear.Value, publishersInLeague);
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

            ClaimGameDomainRequest domainRequest = new ClaimGameDomainRequest(publisher.Value, request.GameName, request.CounterPick, false, masterGame, publisherPosition, overallPosition);

            ClaimResult result = await _fantasyCriticService.ClaimGame(domainRequest, false, true, publishersInLeague);
            bool draftCompleted = await _draftService.CompleteDraft(leagueYear.Value, publishersInLeague, result.Success && !request.CounterPick, result.Success && request.CounterPick);
            var viewModel = new PlayerClaimResultViewModel(result);
            await _hubContext.Clients.Group(leagueYear.Value.GetGroupName).SendAsync("RefreshLeagueYear", leagueYear.Value);

            //await _draftService.AutoDraftForLeague(leagueYear.Value, publishersInLeague);
            
            if (draftCompleted)
            {
                await _hubContext.Clients.Group(leagueYear.Value.GetGroupName).SendAsync("DraftFinished", leagueYear.Value);
            }

            return Ok(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> FollowLeague([FromBody] FollowLeagueRequest request)
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

            Result result = await _fantasyCriticService.FollowLeague(league.Value, currentUser);
            if (result.IsFailure)
            {
                return BadRequest(result.Error);
            }

            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> UnfollowLeague([FromBody] FollowLeagueRequest request)
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

            Result result = await _fantasyCriticService.UnfollowLeague(league.Value, currentUser);
            if (result.IsFailure)
            {
                return BadRequest(result.Error);
            }

            return Ok();
        }

        public async Task<ActionResult<List<MasterGameYearViewModel>>> MyUpcomingGames()
        {
            var currentUser = await _userManager.FindByNameAsync(User.Identity.Name);
            var supportedYears = await _interLeagueService.GetSupportedYears();
            var activeYears = supportedYears.Where(x => x.OpenForPlay && !x.Finished);

            List<MasterGameYear> myMasterGames = new List<MasterGameYear>();

            foreach (var year in activeYears)
            {
                IReadOnlyList<LeagueYear> activeLeagueYears = await _leagueMemberService.GetLeaguesYearsForUser(currentUser, year.Year);

                foreach (var leagueYear in activeLeagueYears)
                {
                    if (leagueYear.League.TestLeague)
                    {
                        continue;
                    }

                    var userPublisher = await _publisherService.GetPublisher(leagueYear, currentUser);
                    if (userPublisher.HasNoValue)
                    {
                        continue;
                    }

                    var myMasterGamesInLeague = userPublisher.Value.PublisherGames
                        .Where(x => x.MasterGame.HasValue)
                        .Where(x => x.WillRelease())
                        .Select(x => x.MasterGame.Value);
                    myMasterGames.AddRange(myMasterGamesInLeague);
                }
            }

            var easternZone = DateTimeZoneProviders.Tzdb.GetZoneOrNull("America/New_York");
            if (easternZone is null)
            {
                throw new Exception("Time has broken.");
            }

            var currentDate = _clock.GetCurrentInstant().InZone(easternZone).Date;
            var yesterday = currentDate.PlusDays(-1);

            var orderedByReleaseDate = myMasterGames
                .Distinct()
                .Where(x => x.MasterGame.SortableEstimatedReleaseDate > yesterday)
                .OrderBy(x => x.MasterGame.SortableEstimatedReleaseDate)
                .Take(10);

            List<MasterGameYearViewModel> viewModels = orderedByReleaseDate.Select(x => new MasterGameYearViewModel(x, _clock)).ToList();
            return viewModels;
        }

        public async Task<ActionResult<List<PossibleMasterGameYearViewModel>>> PossibleMasterGames(string gameName, int year, Guid leagueID)
        {
            var currentUser = await _userManager.FindByNameAsync(User.Identity.Name);
            if (string.IsNullOrWhiteSpace(gameName))
            {
                return new List<PossibleMasterGameYearViewModel>();
            }

            var leagueYear = await _fantasyCriticService.GetLeagueYear(leagueID, year);
            if (leagueYear.HasNoValue)
            {
                return BadRequest();
            }

            var publishersInLeague = await _publisherService.GetPublishersInLeagueForYear(leagueYear.Value);
            var userPublisher = publishersInLeague.SingleOrDefault(x => x.User.Equals(currentUser));
            if (userPublisher is null)
            {
                return BadRequest();
            }

            var matchingGames = await _gameSearchingService.SearchGames(gameName, userPublisher, publishersInLeague, year);
            var viewModels = matchingGames.Select(x => new PossibleMasterGameYearViewModel(x, _clock)).Take(50).ToList();

            return viewModels;
        }

        public async Task<ActionResult<List<PossibleMasterGameYearViewModel>>> TopAvailableGames(int year, Guid leagueID)
        {
            var currentUser = await _userManager.FindByNameAsync(User.Identity.Name);

            var leagueYear = await _fantasyCriticService.GetLeagueYear(leagueID, year);
            if (leagueYear.HasNoValue)
            {
                return BadRequest();
            }

            var publishersInLeague = await _publisherService.GetPublishersInLeagueForYear(leagueYear.Value);
            var userPublisher = publishersInLeague.SingleOrDefault(x => x.User.Equals(currentUser));
            if (userPublisher is null)
            {
                return BadRequest();
            }

            var topAvailableGames = await _gameSearchingService.GetTopAvailableGames(userPublisher, publishersInLeague, year);
            var viewModels = topAvailableGames.Select(x => new PossibleMasterGameYearViewModel(x, _clock)).Take(100).ToList();

            return viewModels;
        }

        [HttpPost]
        public async Task<IActionResult> MakeDropRequest([FromBody] DropGameRequestRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var systemWideSettings = await _interLeagueService.GetSystemWideSettings();
            if (systemWideSettings.BidProcessingMode)
            {
                return BadRequest();
            }

            var publisher = await _publisherService.GetPublisher(request.PublisherID);
            if (publisher.HasNoValue)
            {
                return BadRequest();
            }

            Maybe<LeagueYear> leagueYear = await _fantasyCriticService.GetLeagueYear(publisher.Value.LeagueYear.League.LeagueID, publisher.Value.LeagueYear.Year);
            if (leagueYear.HasNoValue)
            {
                return BadRequest();
            }
            if (!leagueYear.Value.PlayStatus.DraftFinished)
            {
                return BadRequest("Draft has not finished for that year.");
            }

            var supportedYear = (await _interLeagueService.GetSupportedYears()).SingleOrDefault(x => x.Year == publisher.Value.LeagueYear.Year);
            if (supportedYear is null)
            {
                return BadRequest();
            }

            if (supportedYear.Finished)
            {
                return BadRequest("That year is already finished");
            }

            if (supportedYear.Year == 2019)
            {
                return BadRequest("This feature is not supported for 2019");
            }

            var currentUser = await _userManager.FindByNameAsync(User.Identity.Name);
            bool userIsInLeague = await _leagueMemberService.UserIsInLeague(publisher.Value.LeagueYear.League, currentUser);
            bool userIsPublisher = (currentUser.UserID == publisher.Value.User.UserID);
            if (!userIsInLeague || !userIsPublisher)
            {
                return Forbid();
            }

            var masterGame = await _interLeagueService.GetMasterGame(request.MasterGameID);
            if (masterGame.HasNoValue)
            {
                return BadRequest("That master game does not exist.");
            }

            var publisherGame = publisher.Value.PublisherGames.Where(x => x.MasterGame.HasValue)
                .SingleOrDefault(x => masterGame.Value.Equals(x.MasterGame.Value.MasterGame));

            if (publisherGame is null)
            {
                return BadRequest("That publisher does not have that game.");
            }

            DropResult dropResult = await _fantasyCriticService.MakeDropRequest(publisher.Value, publisherGame);
            var viewModel = new DropGameResultViewModel(dropResult);

            return Ok(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteDropRequest([FromBody] DropGameRequestDeleteRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var systemWideSettings = await _interLeagueService.GetSystemWideSettings();
            if (systemWideSettings.BidProcessingMode)
            {
                return BadRequest();
            }

            var dropRequest = await _fantasyCriticService.GetDropRequest(request.DropRequestID);
            if (dropRequest.HasNoValue)
            {
                return BadRequest("That drop request does not exist.");
            }

            var publisher = dropRequest.Value.Publisher;
            var currentUser = await _userManager.FindByNameAsync(User.Identity.Name);
            bool userIsInLeague = await _leagueMemberService.UserIsInLeague(publisher.LeagueYear.League, currentUser);
            bool userIsPublisher = (currentUser.UserID == publisher.User.UserID);
            if (!userIsInLeague || !userIsPublisher)
            {
                return Forbid();
            }

            Result result = await _fantasyCriticService.RemoveDropRequest(dropRequest.Value);
            if (result.IsFailure)
            {
                return BadRequest(result.Error);
            }

            return Ok();
        }

        [HttpGet("{publisherID}")]
        public async Task<IActionResult> CurrentDropRequests(Guid publisherID)
        {
            Maybe<Publisher> publisher = await _publisherService.GetPublisher(publisherID);
            if (publisher.HasNoValue)
            {
                return NotFound();
            }

            var currentUser = await _userManager.FindByNameAsync(User.Identity.Name);
            if (currentUser.UserID != publisher.Value.User.UserID)
            {
                return Forbid();
            }

            var dropRequests = await _fantasyCriticService.GetActiveDropRequests(publisher.Value);

            var viewModels = dropRequests.Select(x => new DropGameRequestViewModel(x, _clock));
            return Ok(viewModels);
        }

        [HttpGet("{publisherID}")]
        public async Task<IActionResult> CurrentQueuedGames(Guid publisherID)
        {
            Maybe<Publisher> publisher = await _publisherService.GetPublisher(publisherID);
            if (publisher.HasNoValue)
            {
                return NotFound();
            }

            var currentUser = await _userManager.FindByNameAsync(User.Identity.Name);
            if (currentUser.UserID != publisher.Value.User.UserID)
            {
                return Forbid();
            }

            var queuedGames = await _publisherService.GetQueuedGames(publisher.Value);

            var viewModels = queuedGames.Select(x => new QueuedGameViewModel(x, _clock)).OrderBy(x => x.Rank);
            return Ok(viewModels);
        }

        [HttpGet("{publisherID}")]
        public async Task<IActionResult> CurrentQueuedGameYears(Guid publisherID)
        {
            Maybe<Publisher> publisher = await _publisherService.GetPublisher(publisherID);
            if (publisher.HasNoValue)
            {
                return NotFound();
            }

            var currentUser = await _userManager.FindByNameAsync(User.Identity.Name);
            if (currentUser.UserID != publisher.Value.User.UserID)
            {
                return Forbid();
            }

            Maybe<LeagueYear> leagueYear = await _fantasyCriticService.GetLeagueYear(publisher.Value.LeagueYear.League.LeagueID, publisher.Value.LeagueYear.Year);
            if (leagueYear.HasNoValue)
            {
                return BadRequest();
            }

            var publishersInLeague = await _publisherService.GetPublishersInLeagueForYear(leagueYear.Value);
            var queuedGames = await _publisherService.GetQueuedGames(publisher.Value);

            var queuedPossibleGames = await _gameSearchingService.GetQueuedPossibleGames(publisher.Value, publishersInLeague, queuedGames);
            var viewModels = queuedPossibleGames.Select(x => new PossibleMasterGameYearViewModel(x, _clock)).Take(100).ToList();

            return Ok(viewModels);
        }

        [HttpPost]
        public async Task<IActionResult> AddGameToQueue([FromBody] AddGameToQueueRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var publisher = await _publisherService.GetPublisher(request.PublisherID);
            if (publisher.HasNoValue)
            {
                return BadRequest();
            }

            var currentUser = await _userManager.FindByNameAsync(User.Identity.Name);
            if (currentUser.UserID != publisher.Value.User.UserID)
            {
                return Forbid();
            }

            var league = await _fantasyCriticService.GetLeagueByID(publisher.Value.LeagueYear.League.LeagueID);
            if (league.HasNoValue)
            {
                return BadRequest();
            }

            var leagueYear = await _fantasyCriticService.GetLeagueYear(league.Value.LeagueID, publisher.Value.LeagueYear.Year);
            if (leagueYear.HasNoValue)
            {
                return BadRequest();
            }

            if (leagueYear.Value.PlayStatus.DraftFinished)
            {
                return BadRequest("You can't queue game if the draft is over.");
            }

            Maybe<MasterGame> masterGame = await _interLeagueService.GetMasterGame(request.MasterGameID);
            if (masterGame.HasNoValue)
            {
                return BadRequest("That master game does not exist.");
            }

            ClaimResult bidResult = await _fantasyCriticService.QueueGame(publisher.Value, masterGame.Value);
            var viewModel = new QueueResultViewModel(bidResult);

            return Ok(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteQueuedGame([FromBody] QueuedGameDeleteRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var publisher = await _publisherService.GetPublisher(request.PublisherID);
            if (publisher.HasNoValue)
            {
                return BadRequest();
            }

            var league = await _fantasyCriticService.GetLeagueByID(publisher.Value.LeagueYear.League.LeagueID);
            if (league.HasNoValue)
            {
                return BadRequest();
            }

            var currentUser = await _userManager.FindByNameAsync(User.Identity.Name);
            bool userIsInLeague = await _leagueMemberService.UserIsInLeague(league.Value, currentUser);
            bool userIsPublisher = (currentUser.UserID == publisher.Value.User.UserID);
            if (!userIsInLeague || !userIsPublisher)
            {
                return Forbid();
            }

            var queuedGames = await _publisherService.GetQueuedGames(publisher.Value);
            var thisQueuedGame = queuedGames.SingleOrDefault(x => x.MasterGame.MasterGameID == request.MasterGameID);
            if (thisQueuedGame is null)
            {
                return BadRequest();
            }

            await _fantasyCriticService.RemoveQueuedGame(thisQueuedGame);

            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> SetQueueRankings([FromBody] QueueRankingRequest request)
        {
            Maybe<Publisher> publisher = await _publisherService.GetPublisher(request.PublisherID);
            if (publisher.HasNoValue)
            {
                return NotFound();
            }

            var currentUser = await _userManager.FindByNameAsync(User.Identity.Name);
            if (currentUser.UserID != publisher.Value.User.UserID)
            {
                return Forbid();
            }

            var queuedGames = await _publisherService.GetQueuedGames(publisher.Value);
            if (queuedGames.Count != request.QueueRanks.Count)
            {
                return BadRequest();
            }

            List<KeyValuePair<QueuedGame, int>> queueRanks = new List<KeyValuePair<QueuedGame, int>>();
            for (var index = 0; index < request.QueueRanks.Count; index++)
            {
                var requestQueuedGame = request.QueueRanks[index];
                var activeQueuedGame = queuedGames.SingleOrDefault(x => x.MasterGame.MasterGameID == requestQueuedGame);
                if (activeQueuedGame is null)
                {
                    return BadRequest();
                }

                queueRanks.Add(new KeyValuePair<QueuedGame, int>(activeQueuedGame, index + 1));
            }

            Result result = await _publisherService.SetQueueRankings(queueRanks);
            if (result.IsFailure)
            {
                return BadRequest(result.Error);
            }

            return Ok();
        }
    }
}
