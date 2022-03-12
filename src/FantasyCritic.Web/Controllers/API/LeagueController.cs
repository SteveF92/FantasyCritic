using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amazon.RDS.Model.Internal.MarshallTransformations;
using CSharpFunctionalExtensions;
using FantasyCritic.Lib.Domain;
using FantasyCritic.Lib.Domain.LeagueActions;
using FantasyCritic.Lib.Domain.Requests;
using FantasyCritic.Lib.Domain.Results;
using FantasyCritic.Lib.Domain.ScoringSystems;
using FantasyCritic.Lib.Domain.Trades;
using FantasyCritic.Lib.Enums;
using FantasyCritic.Lib.Extensions;
using FantasyCritic.Lib.Identity;
using FantasyCritic.Lib.Services;
using FantasyCritic.Lib.Utilities;
using FantasyCritic.Web.Hubs;
using FantasyCritic.Web.Models;
using FantasyCritic.Web.Models.Requests;
using FantasyCritic.Web.Models.Requests.League;
using FantasyCritic.Web.Models.Requests.League.Trades;
using FantasyCritic.Web.Models.Requests.Shared;
using FantasyCritic.Web.Models.Responses;
using FantasyCritic.Web.Models.RoundTrip;
using FantasyCritic.Web.Utilities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NodaTime;
using static MoreLinq.Extensions.MaxByExtension;

namespace FantasyCritic.Web.Controllers.API
{
    [Route("api/[controller]/[action]")]
    [Authorize]
    public class LeagueController : FantasyCriticController
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
        private readonly GameAcquisitionService _gameAcquisitionService;

        public LeagueController(FantasyCriticUserManager userManager, FantasyCriticService fantasyCriticService, InterLeagueService interLeagueService,
            LeagueMemberService leagueMemberService, DraftService draftService, GameSearchingService gameSearchingService, PublisherService publisherService, IClock clock,
            IHubContext<UpdateHub> hubContext, ILogger<LeagueController> logger, GameAcquisitionService gameAcquisitionService) : base(userManager)
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
            _gameAcquisitionService = gameAcquisitionService;
        }

        public async Task<IActionResult> LeagueOptions()
        {
            var supportedYears = await _interLeagueService.GetSupportedYears();
            var openYears = supportedYears.Where(x => x.OpenForCreation && !x.Finished);

            var currentUserResult = await GetCurrentUser();
            if (currentUserResult.IsFailure)
            {
                return BadRequest(currentUserResult.Error);
            }
            var currentUser = currentUserResult.Value;

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
            LeagueOptionsViewModel viewModel = new LeagueOptionsViewModel(openYearInts, DraftSystem.GetAllPossibleValues(),
                PickupSystem.GetAllPossibleValues(), TiebreakSystem.GetAllPossibleValues(),
                ScoringSystem.GetAllPossibleValues(), TradingSystem.GetAllPossibleValues());

            return Ok(viewModel);
        }

        public async Task<IActionResult> MyLeagues(int? year)
        {
            var currentUserResult = await GetCurrentUser();
            if (currentUserResult.IsFailure)
            {
                return BadRequest(currentUserResult.Error);
            }
            var currentUser = currentUserResult.Value;

            IReadOnlyList<League> myLeagues = await _leagueMemberService.GetLeaguesForUser(currentUser);

            List<LeagueViewModel> viewModels = new List<LeagueViewModel>();
            foreach (var league in myLeagues)
            {
                if (year.HasValue && !league.Years.Contains(year.Value))
                {
                    continue;
                }
                bool isManager = (league.LeagueManager.Id == currentUser.Id);
                viewModels.Add(new LeagueViewModel(league, isManager, true, false));
            }

            return Ok(viewModels);
        }

        public async Task<IActionResult> FollowedLeagues()
        {
            var currentUserResult = await GetCurrentUser();
            if (currentUserResult.IsFailure)
            {
                return BadRequest(currentUserResult.Error);
            }
            var currentUser = currentUserResult.Value;

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
            var currentUserResult = await GetCurrentUser();
            if (currentUserResult.IsFailure)
            {
                return BadRequest(currentUserResult.Error);
            }
            var currentUser = currentUserResult.Value;

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
                var currentUserResult = await GetCurrentUser();
                if (currentUserResult.IsFailure)
                {
                    return BadRequest(currentUserResult.Error);
                }
                currentUser = currentUserResult.Value;
            }

            var playersInLeague = await _leagueMemberService.GetUsersWithRemoveStatus(league.Value);
            var inviteesToLeague = await _leagueMemberService.GetOutstandingInvitees(league.Value);
            var leagueFollowers = await _fantasyCriticService.GetLeagueFollowers(league.Value);

            bool userIsInLeague = false;
            bool userIsInvitedToLeague = false;
            bool isManager = false;
            bool userIsFollowingLeague = false;
            bool userIsAdmin = false;
            Maybe<LeagueInvite> leagueInvite = Maybe<LeagueInvite>.None;
            if (currentUser != null)
            {
                userIsInLeague = playersInLeague.Any(x => x.User.Id == currentUser.Id);
                userIsInvitedToLeague = inviteesToLeague.UserIsInvited(currentUser.Email);
                isManager = (league.Value.LeagueManager.Id == currentUser.Id);
                userIsFollowingLeague = leagueFollowers.Any(x => x.Id == currentUser.Id);
                leagueInvite = inviteesToLeague.GetMatchingInvite(currentUser.Email);
                userIsAdmin = await _userManager.IsInRoleAsync(currentUser, "Admin");
            }

            bool inviteCodeIsValid = false;
            if (inviteCode.HasValue)
            {
                var activeLinks = await _leagueMemberService.GetActiveInviteLinks(league.Value);
                inviteCodeIsValid = activeLinks.Any(x => x.Active && x.InviteCode == inviteCode.Value);
            }

            if (!userIsInLeague && !userIsInvitedToLeague && !league.Value.PublicLeague && !inviteCodeIsValid && !userIsAdmin)
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
                var currentUserResult = await GetCurrentUser();
                if (currentUserResult.IsFailure)
                {
                    return BadRequest(currentUserResult.Error);
                }
                currentUser = currentUserResult.Value;
            }

            var activeUsers = await _leagueMemberService.GetActivePlayersForLeagueYear(leagueYear.Value.League, year);
            var inviteesToLeague = await _leagueMemberService.GetOutstandingInvitees(leagueYear.Value.League);

            bool userIsInLeague = false;
            bool userIsInvitedToLeague = false;
            bool isManager = false;
            bool userIsAdmin = false;
            if (currentUser != null)
            {
                userIsInLeague = activeUsers.Any(x => x.Id == currentUser.Id);
                userIsInvitedToLeague = inviteesToLeague.UserIsInvited(currentUser.Email);
                isManager = (leagueYear.Value.League.LeagueManager.Id == currentUser.Id);
                userIsAdmin = await _userManager.IsInRoleAsync(currentUser, "Admin");
            }

            bool inviteCodeIsValid = false;
            if (inviteCode.HasValue)
            {
                var activeLinks = await _leagueMemberService.GetActiveInviteLinks(leagueYear.Value.League);
                inviteCodeIsValid = activeLinks.Any(x => x.Active && x.InviteCode == inviteCode.Value);
            }

            if (!userIsInLeague && !userIsInvitedToLeague && !leagueYear.Value.League.PublicLeague && !inviteCodeIsValid && !userIsAdmin)
            {
                return Forbid();
            }

            var publishersInLeague = await _publisherService.GetPublishersInLeagueForYear(leagueYear.Value, activeUsers);
            var supportedYears = await _interLeagueService.GetSupportedYears();
            var supportedYear = supportedYears.SingleOrDefault(x => x.Year == year);
            if (supportedYear is null)
            {
                return BadRequest();
            }

            StartDraftResult startDraftResult = await _draftService.GetStartDraftResult(leagueYear.Value, publishersInLeague, activeUsers);
            Maybe<Publisher> nextDraftPublisher = _draftService.GetNextDraftPublisher(leagueYear.Value, publishersInLeague);
            var draftPhase = _draftService.GetDraftPhase(leagueYear.Value, publishersInLeague);

            Maybe<Publisher> userPublisher = Maybe<Publisher>.None;
            if (userIsInLeague)
            {
                userPublisher = publishersInLeague.SingleOrDefault(x => x.User.Id == currentUser.Id);
            }

            SystemWideValues systemWideValues = await _interLeagueService.GetSystemWideValues();
            IReadOnlyList<ManagerMessage> managerMessages = await _fantasyCriticService.GetManagerMessages(leagueYear.Value);

            Maybe<FantasyCriticUser> previousYearWinner = await _fantasyCriticService.GetPreviousYearWinner(leagueYear.Value);
            var publicBiddingGames = await _gameAcquisitionService.GetPublicBiddingGames(leagueYear.Value);
            IReadOnlySet<Guid> counterPickedPublisherGameIDs = GameUtilities.GetCounterPickedPublisherGameIDs(leagueYear.Value, publishersInLeague);

            IReadOnlyList<Trade> activeTrades = await _fantasyCriticService.GetActiveTradesForLeague(leagueYear.Value, publishersInLeague);

            var currentDate = _clock.GetToday();
            var leagueViewModel = new LeagueYearViewModel(leagueYear.Value, supportedYear, publishersInLeague, userPublisher, currentDate,
                startDraftResult, activeUsers, nextDraftPublisher, draftPhase, systemWideValues, inviteesToLeague, userIsInLeague, userIsInvitedToLeague, isManager,
                currentUser, managerMessages, previousYearWinner, publicBiddingGames, counterPickedPublisherGameIDs, activeTrades);
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
                var currentUserResult = await GetCurrentUser();
                if (currentUserResult.IsFailure)
                {
                    return BadRequest(currentUserResult.Error);
                }
                currentUser = currentUserResult.Value;
            }

            var inviteesToLeague = await _leagueMemberService.GetOutstandingInvitees(leagueYear.Value.League);

            bool userIsInLeague = false;
            bool userIsInvitedToLeague = false;
            bool userIsAdmin = false;
            if (currentUser != null)
            {
                var usersInLeague = await _leagueMemberService.GetUsersInLeague(leagueYear.Value.League);
                userIsInLeague = usersInLeague.Any(x => x.Id == currentUser.Id);
                userIsInvitedToLeague = inviteesToLeague.UserIsInvited(currentUser.Email);
                userIsAdmin = await _userManager.IsInRoleAsync(currentUser, "Admin");
            }

            if (!userIsInLeague && !userIsInvitedToLeague && !leagueYear.Value.League.PublicLeague && !userIsAdmin)
            {
                return Forbid();
            }

            var leagueActions = await _fantasyCriticService.GetLeagueActions(leagueYear.Value);

            var viewModels = leagueActions.Select(x => new LeagueActionViewModel(x, _clock));
            viewModels = viewModels.OrderByDescending(x => x.Timestamp);
            return Ok(viewModels);
        }

        [AllowAnonymous]
        public async Task<IActionResult> GetLeagueActionSets(Guid leagueID, int year)
        {
            Maybe<LeagueYear> leagueYear = await _fantasyCriticService.GetLeagueYear(leagueID, year);
            if (leagueYear.HasNoValue)
            {
                throw new Exception("Something went really wrong, no options are set up for this league.");
            }

            FantasyCriticUser currentUser = null;
            if (!string.IsNullOrWhiteSpace(User.Identity.Name))
            {
                var currentUserResult = await GetCurrentUser();
                if (currentUserResult.IsFailure)
                {
                    return BadRequest(currentUserResult.Error);
                }
                currentUser = currentUserResult.Value;
            }

            var inviteesToLeague = await _leagueMemberService.GetOutstandingInvitees(leagueYear.Value.League);

            bool userIsInLeague = false;
            bool userIsInvitedToLeague = false;
            bool userIsAdmin = false;
            if (currentUser != null)
            {
                var usersInLeague = await _leagueMemberService.GetUsersInLeague(leagueYear.Value.League);
                userIsInLeague = usersInLeague.Any(x => x.Id == currentUser.Id);
                userIsInvitedToLeague = inviteesToLeague.UserIsInvited(currentUser.Email);
                userIsAdmin = await _userManager.IsInRoleAsync(currentUser, "Admin");
            }

            if (!userIsInLeague && !userIsInvitedToLeague && !leagueYear.Value.League.PublicLeague && !userIsAdmin)
            {
                return Forbid();
            }

            var leagueActionSets = await _fantasyCriticService.GetLeagueActionProcessingSets(leagueYear.Value);

            var currentDate = _clock.GetToday();
            var viewModels = leagueActionSets.Where(x => x.HasActions).Select(x => new LeagueActionProcessingSetViewModel(x, currentDate));
            viewModels = viewModels.OrderByDescending(x => x.ProcessTime);
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
                var currentUserResult = await GetCurrentUser();
                if (currentUserResult.IsFailure)
                {
                    return BadRequest(currentUserResult.Error);
                }
                currentUser = currentUserResult.Value;
            }

            var playersInLeague = await _leagueMemberService.GetUsersInLeague(publisher.Value.LeagueYear.League);
            var inviteesToLeague = await _leagueMemberService.GetOutstandingInvitees(publisher.Value.LeagueYear.League);

            bool userIsInLeague = false;
            bool userIsInvitedToLeague = false;
            bool userIsAdmin = false;
            if (currentUser != null)
            {
                userIsInLeague = playersInLeague.Any(x => x.Id == currentUser.Id);
                userIsInvitedToLeague = inviteesToLeague.UserIsInvited(currentUser.Email);
                userIsAdmin = await _userManager.IsInRoleAsync(currentUser, "Admin");
            }

            if (!userIsInLeague && !publisher.Value.LeagueYear.League.PublicLeague && !userIsAdmin)
            {
                return Forbid();
            }

            var requestedPlayerIsInLeague = playersInLeague.Any(x => x.Id == publisher.Value.User.Id);
            if (!requestedPlayerIsInLeague)
            {
                return BadRequest("Requested player is not in requested league.");
            }

            SystemWideValues systemWideValues = await _interLeagueService.GetSystemWideValues();
            var supportedYear = (await _interLeagueService.GetSupportedYears()).SingleOrDefault(x => x.Year == publisher.Value.LeagueYear.Year);
            var activeUsers = await _leagueMemberService.GetActivePlayersForLeagueYear(publisher.Value.LeagueYear.League, publisher.Value.LeagueYear.Year);
            var publishersInLeague = await _publisherService.GetPublishersInLeagueForYear(publisher.Value.LeagueYear, activeUsers);
            IReadOnlySet<Guid> counterPickedPublisherGameIDs = GameUtilities.GetCounterPickedPublisherGameIDs(publisher.Value.LeagueYear, publishersInLeague);

            var currentDate = _clock.GetToday();
            var publisherViewModel = new PublisherViewModel(publisher.Value, currentDate, userIsInLeague, userIsInvitedToLeague, systemWideValues,
                supportedYear.Finished, counterPickedPublisherGameIDs);
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
            bool userIsAdmin = false;
            FantasyCriticUser currentUser = null;
            if (!string.IsNullOrWhiteSpace(User.Identity.Name))
            {
                var currentUserResult = await GetCurrentUser();
                if (currentUserResult.IsFailure)
                {
                    return BadRequest(currentUserResult.Error);
                }
                currentUser = currentUserResult.Value;
            }
            if (currentUser != null)
            {
                var usersInLeague = await _leagueMemberService.GetUsersInLeague(leagueYear.Value.League);
                userIsInLeague = usersInLeague.Any(x => x.Id == currentUser.Id);
                userIsInvitedToLeague = inviteesToLeague.UserIsInvited(currentUser.Email);
                userIsAdmin = await _userManager.IsInRoleAsync(currentUser, "Admin");
            }

            if (!userIsInLeague && !userIsInvitedToLeague && !leagueYear.Value.League.PublicLeague && !userIsAdmin)
            {
                return Forbid();
            }

            var leagueViewModel = new LeagueYearSettingsViewModel(leagueYear.Value);
            return Ok(leagueViewModel);
        }

        [HttpPost]
        public async Task<IActionResult> AcceptInvite([FromBody] AcceptInviteRequest request)
        {
            var currentUserResult = await GetCurrentUser();
            if (currentUserResult.IsFailure)
            {
                return BadRequest(currentUserResult.Error);
            }
            var currentUser = currentUserResult.Value;

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
            var currentUserResult = await GetCurrentUser();
            if (currentUserResult.IsFailure)
            {
                return BadRequest(currentUserResult.Error);
            }
            var currentUser = currentUserResult.Value;

            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var inviteLink = await _leagueMemberService.GetInviteLinkByInviteCode(request.InviteCode);
            if (inviteLink.HasNoValue)
            {
                return BadRequest();
            }

            var league = inviteLink.Value.League;

            if (league.LeagueID != request.LeagueID)
            {
                return BadRequest();
            }
            
            var mostRecentYear = await _fantasyCriticService.GetLeagueYear(league.LeagueID, league.Years.Max());
            if (mostRecentYear.HasNoValue)
            {
                return BadRequest();
            }

            bool mostRecentYearIsStarted = mostRecentYear.Value.PlayStatus.PlayStarted;
            if (mostRecentYearIsStarted)
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

            var currentUserResult = await GetCurrentUser();
            if (currentUserResult.IsFailure)
            {
                return BadRequest(currentUserResult.Error);
            }
            var currentUser = currentUserResult.Value;

            if (string.IsNullOrWhiteSpace(request.PublisherName))
            {
                return BadRequest("You cannot have a blank name.");
            }

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
            var publisherForUser = currentPublishers.SingleOrDefault(x => x.User.Id == currentUser.Id);
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

            var currentUserResult = await GetCurrentUser();
            if (currentUserResult.IsFailure)
            {
                return BadRequest(currentUserResult.Error);
            }
            var currentUser = currentUserResult.Value;

            if (string.IsNullOrWhiteSpace(request.PublisherName))
            {
                return BadRequest("You cannot have a blank name.");
            }

            bool userIsInLeague = await _leagueMemberService.UserIsInLeague(publisher.Value.LeagueYear.League, currentUser);
            if (!userIsInLeague)
            {
                return Forbid();
            }

            if (publisher.Value.User.Id != currentUser.Id)
            {
                return Forbid();
            }

            await _publisherService.ChangePublisherName(publisher.Value, request.PublisherName);
            return Ok();
        }

        [HttpPost]
        [Authorize(Roles = "PlusUser")]
        public async Task<IActionResult> ChangePublisherIcon([FromBody] ChangePublisherIconRequest request)
        {
            var publisher = await _publisherService.GetPublisher(request.PublisherID);
            if (publisher.HasNoValue)
            {
                return BadRequest();
            }

            var currentUserResult = await GetCurrentUser();
            if (currentUserResult.IsFailure)
            {
                return BadRequest(currentUserResult.Error);
            }
            var currentUser = currentUserResult.Value;

            bool userIsInLeague = await _leagueMemberService.UserIsInLeague(publisher.Value.LeagueYear.League, currentUser);
            if (!userIsInLeague)
            {
                return Forbid();
            }

            if (publisher.Value.User.Id != currentUser.Id)
            {
                return Forbid();
            }

            await _publisherService.ChangePublisherIcon(publisher.Value, request.PublisherIcon.ToMaybe());
            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> SetAutoDraft([FromBody] SetAutoDraftRequest request)
        {
            var publisher = await _publisherService.GetPublisher(request.PublisherID);
            if (publisher.HasNoValue)
            {
                return BadRequest();
            }

            var currentUserResult = await GetCurrentUser();
            if (currentUserResult.IsFailure)
            {
                return BadRequest(currentUserResult.Error);
            }
            var currentUser = currentUserResult.Value;

            bool userIsInLeague = await _leagueMemberService.UserIsInLeague(publisher.Value.LeagueYear.League, currentUser);
            if (!userIsInLeague)
            {
                return Forbid();
            }

            if (publisher.Value.User.Id != currentUser.Id)
            {
                return Forbid();
            }

            await _publisherService.SetAutoDraft(publisher.Value, request.AutoDraft);

            var draftComplete = await _draftService.RunAutoDraftAndCheckIfComplete(publisher.Value.LeagueYear);
            if (draftComplete)
            {
                await _hubContext.Clients.Group(publisher.Value.LeagueYear.GetGroupName).SendAsync("DraftFinished");
            }

            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> DeclineInvite([FromBody] DeleteInviteRequest request)
        {
            var currentUserResult = await GetCurrentUser();
            if (currentUserResult.IsFailure)
            {
                return BadRequest(currentUserResult.Error);
            }
            var currentUser = currentUserResult.Value;

            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            Maybe<LeagueInvite> invite = await _leagueMemberService.GetInvite(request.InviteID);
            if (invite.HasNoValue)
            {
                return BadRequest();
            }

            if (!string.Equals(invite.Value.EmailAddress, currentUser.Email, StringComparison.OrdinalIgnoreCase))
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
            if (systemWideSettings.ActionProcessingMode)
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

            var currentUserResult = await GetCurrentUser();
            if (currentUserResult.IsFailure)
            {
                return BadRequest(currentUserResult.Error);
            }
            var currentUser = currentUserResult.Value;
            bool userIsPublisher = (currentUser.Id == publisher.Value.User.Id);
            if (!userIsPublisher)
            {
                return Forbid();
            }

            var masterGame = await _interLeagueService.GetMasterGame(request.MasterGameID);
            if (masterGame.HasNoValue)
            {
                return BadRequest("That master game does not exist.");
            }

            var publicBiddingGames = await _gameAcquisitionService.GetPublicBiddingGames(leagueYear.Value);
            bool publicBidIsValid = _gameAcquisitionService.PublicBidIsValid(leagueYear.Value, masterGame.Value, request.CounterPick, publicBiddingGames);
            if (!publicBidIsValid)
            {
                return BadRequest("During the public bidding window, you can only bid on a game that is already being bid on by at least one player.");
            }

            Maybe<PublisherGame> conditionalDropPublisherGame = Maybe<PublisherGame>.None;
            if (request.ConditionalDropPublisherGameID.HasValue)
            {
                conditionalDropPublisherGame = publisher.Value.GetPublisherGameByPublisherGameID(request.ConditionalDropPublisherGameID.Value);
            }
            
            ClaimResult bidResult = await _gameAcquisitionService.MakePickupBid(publisher.Value, masterGame.Value, conditionalDropPublisherGame, request.CounterPick, request.BidAmount, leagueYear.Value.Options);
            var viewModel = new PickupBidResultViewModel(bidResult);

            return Ok(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> EditPickupBid([FromBody] PickupBidEditRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var systemWideSettings = await _interLeagueService.GetSystemWideSettings();
            if (systemWideSettings.ActionProcessingMode)
            {
                return BadRequest();
            }

            var maybeBid = await _gameAcquisitionService.GetPickupBid(request.BidID);
            if (maybeBid.HasNoValue)
            {
                return BadRequest("That bid does not exist.");
            }

            var publisher = maybeBid.Value.Publisher;

            var currentUserResult = await GetCurrentUser();
            if (currentUserResult.IsFailure)
            {
                return BadRequest(currentUserResult.Error);
            }
            var currentUser = currentUserResult.Value;

            bool userIsPublisher = (currentUser.Id == publisher.User.Id);
            if (!userIsPublisher)
            {
                return Forbid();
            }

            Maybe<PublisherGame> conditionalDropPublisherGame = Maybe<PublisherGame>.None;
            if (request.ConditionalDropPublisherGameID.HasValue)
            {
                conditionalDropPublisherGame = publisher.GetPublisherGameByPublisherGameID(request.ConditionalDropPublisherGameID.Value);
            }

            ClaimResult bidResult = await _gameAcquisitionService.EditPickupBid(maybeBid.Value, conditionalDropPublisherGame, request.BidAmount);
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
            if (systemWideSettings.ActionProcessingMode)
            {
                return BadRequest();
            }

            var maybeBid = await _gameAcquisitionService.GetPickupBid(request.BidID);
            if (maybeBid.HasNoValue)
            {
                return BadRequest("That bid does not exist.");
            }

            var publisher = maybeBid.Value.Publisher;

            var currentUserResult = await GetCurrentUser();
            if (currentUserResult.IsFailure)
            {
                return BadRequest(currentUserResult.Error);
            }
            var currentUser = currentUserResult.Value;

            bool userIsPublisher = (currentUser.Id == publisher.User.Id);
            if (!userIsPublisher)
            {
                return Forbid();
            }

            bool canCancel = _gameAcquisitionService.CanCancelBid(publisher.LeagueYear, maybeBid.Value.CounterPick);
            if (!canCancel)
            {
                return BadRequest("Can't cancel a bid when in the public bidding window.");
            }

            PickupBid bid = maybeBid.Value;
            Result result = await _gameAcquisitionService.RemovePickupBid(bid);
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

            var currentUserResult = await GetCurrentUser();
            if (currentUserResult.IsFailure)
            {
                return BadRequest(currentUserResult.Error);
            }
            var currentUser = currentUserResult.Value;
            if (currentUser.Id != publisher.Value.User.Id)
            {
                return Forbid();
            }

            var bids = await _gameAcquisitionService.GetActiveAcquisitionBids(publisher.Value);
            var currentDate = _clock.GetToday();
            var viewModels = bids.Select(x => new PickupBidViewModel(x, currentDate)).OrderBy(x => x.Priority);
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

            var currentUserResult = await GetCurrentUser();
            if (currentUserResult.IsFailure)
            {
                return BadRequest(currentUserResult.Error);
            }
            var currentUser = currentUserResult.Value;
            if (currentUser.Id != publisher.Value.User.Id)
            {
                return Forbid();
            }

            var activeBids = await _gameAcquisitionService.GetActiveAcquisitionBids(publisher.Value);

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

            var currentUserResult = await GetCurrentUser();
            if (currentUserResult.IsFailure)
            {
                return BadRequest(currentUserResult.Error);
            }
            var currentUser = currentUserResult.Value;
            if (currentUser.Id != publisher.Value.User.Id)
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

            var draftPhase = _draftService.GetDraftPhase(leagueYear.Value, publishersInLeague);
            if (draftPhase.Equals(DraftPhase.StandardGames))
            {
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

            var draftStatus = _draftService.GetDraftStatus(draftPhase, leagueYear.Value, publishersInLeague);
            ClaimGameDomainRequest domainRequest = new ClaimGameDomainRequest(publisher.Value, request.GameName, request.CounterPick, false, false, false, masterGame,
                draftStatus.DraftPosition, draftStatus.OverallDraftPosition);

            var draftResult = await _draftService.DraftGame(domainRequest, false, leagueYear.Value, publishersInLeague);
            var viewModel = new PlayerClaimResultViewModel(draftResult.Result);
            await _hubContext.Clients.Group(leagueYear.Value.GetGroupName).SendAsync("RefreshLeagueYear");

            if (draftResult.DraftComplete)
            {
                await _hubContext.Clients.Group(leagueYear.Value.GetGroupName).SendAsync("DraftFinished");
            }

            return Ok(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> FollowLeague([FromBody] FollowLeagueRequest request)
        {
            var currentUserResult = await GetCurrentUser();
            if (currentUserResult.IsFailure)
            {
                return BadRequest(currentUserResult.Error);
            }
            var currentUser = currentUserResult.Value;

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
            var currentUserResult = await GetCurrentUser();
            if (currentUserResult.IsFailure)
            {
                return BadRequest(currentUserResult.Error);
            }
            var currentUser = currentUserResult.Value;

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

        public async Task<ActionResult<List<SingleGameNewsViewModel>>> MyUpcomingGames()
        {
            var currentUserResult = await GetCurrentUser();
            if (currentUserResult.IsFailure)
            {
                return BadRequest(currentUserResult.Error);
            }
            var currentUser = currentUserResult.Value;

            var supportedYears = await _interLeagueService.GetSupportedYears();
            var activeYears = supportedYears.Where(x => x.OpenForPlay && !x.Finished);

            List<Publisher> myPublishers = new List<Publisher>();
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

                    myPublishers.Add(userPublisher.Value);
                }
            }

            var viewModels = GetGameNewsViewModel(myPublishers, true, false).ToList();
            return viewModels;
        }

        public async Task<ActionResult<GameNewsViewModel>> MyGameNews()
        {
            var currentUserResult = await GetCurrentUser();
            if (currentUserResult.IsFailure)
            {
                return BadRequest(currentUserResult.Error);
            }
            var currentUser = currentUserResult.Value;

            var supportedYears = await _interLeagueService.GetSupportedYears();
            var activeYears = supportedYears.Where(x => x.OpenForPlay && !x.Finished);

            List<Publisher> myPublishers = new List<Publisher>();
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

                    myPublishers.Add(userPublisher.Value);
                }
            }

            var upcomingGames = GetGameNewsViewModel(myPublishers, true, false).ToList();
            var recentGames = GetGameNewsViewModel(myPublishers, true, true).ToList();
            return new GameNewsViewModel(upcomingGames, recentGames);
        }

        [AllowAnonymous]
        public async Task<ActionResult<List<SingleGameNewsViewModel>>> LeagueUpcomingGames(Guid leagueID, int year)
        {
            Maybe<LeagueYear> leagueYear = await _fantasyCriticService.GetLeagueYear(leagueID, year);
            if (leagueYear.HasNoValue)
            {
                throw new Exception("Something went really wrong, no options are set up for this league.");
            }

            FantasyCriticUser currentUser = null;
            if (!string.IsNullOrWhiteSpace(User.Identity.Name))
            {
                var currentUserResult = await GetCurrentUser();
                if (currentUserResult.IsFailure)
                {
                    return BadRequest(currentUserResult.Error);
                }
                currentUser = currentUserResult.Value;
            }

            var activeUsers = await _leagueMemberService.GetActivePlayersForLeagueYear(leagueYear.Value.League, year);
            var inviteesToLeague = await _leagueMemberService.GetOutstandingInvitees(leagueYear.Value.League);
            bool userIsAdmin = false;

            bool userIsInLeague = false;
            bool userIsInvitedToLeague = false;
            if (currentUser != null)
            {
                userIsInLeague = activeUsers.Any(x => x.Id == currentUser.Id);
                userIsInvitedToLeague = inviteesToLeague.UserIsInvited(currentUser.Email);
                userIsAdmin = await _userManager.IsInRoleAsync(currentUser, "Admin");
            }

            if (!userIsInLeague && !userIsInvitedToLeague && !leagueYear.Value.League.PublicLeague && !userIsAdmin)
            {
                return Forbid();
            }

            var publishersInLeague = await _publisherService.GetPublishersInLeagueForYear(leagueYear.Value, activeUsers);
            var supportedYear = (await _interLeagueService.GetSupportedYears()).SingleOrDefault(x => x.Year == year);
            if (supportedYear is null)
            {
                return BadRequest();
            }

            var viewModels = GetGameNewsViewModel(publishersInLeague, false, false).ToList();
            return viewModels;
        }

        [AllowAnonymous]
        public async Task<ActionResult<GameNewsViewModel>> LeagueGameNews(Guid leagueID, int year)
        {
            Maybe<LeagueYear> leagueYear = await _fantasyCriticService.GetLeagueYear(leagueID, year);
            if (leagueYear.HasNoValue)
            {
                throw new Exception("Something went really wrong, no options are set up for this league.");
            }

            FantasyCriticUser currentUser = null;
            if (!string.IsNullOrWhiteSpace(User.Identity.Name))
            {
                var currentUserResult = await GetCurrentUser();
                if (currentUserResult.IsFailure)
                {
                    return BadRequest(currentUserResult.Error);
                }
                currentUser = currentUserResult.Value;
            }

            var activeUsers = await _leagueMemberService.GetActivePlayersForLeagueYear(leagueYear.Value.League, year);
            var inviteesToLeague = await _leagueMemberService.GetOutstandingInvitees(leagueYear.Value.League);
            bool userIsAdmin = false;

            bool userIsInLeague = false;
            bool userIsInvitedToLeague = false;
            if (currentUser != null)
            {
                userIsInLeague = activeUsers.Any(x => x.Id == currentUser.Id);
                userIsInvitedToLeague = inviteesToLeague.UserIsInvited(currentUser.Email);
                userIsAdmin = await _userManager.IsInRoleAsync(currentUser, "Admin");
            }

            if (!userIsInLeague && !userIsInvitedToLeague && !leagueYear.Value.League.PublicLeague && !userIsAdmin)
            {
                return Forbid();
            }

            var publishersInLeague = await _publisherService.GetPublishersInLeagueForYear(leagueYear.Value, activeUsers);
            var supportedYear = (await _interLeagueService.GetSupportedYears()).SingleOrDefault(x => x.Year == year);
            if (supportedYear is null)
            {
                return BadRequest();
            }

            var upcomingGames = GetGameNewsViewModel(publishersInLeague, false, false).ToList();
            var recentGames = GetGameNewsViewModel(publishersInLeague, false, true).ToList();
            return new GameNewsViewModel(upcomingGames, recentGames);
        }

        public async Task<ActionResult<List<PossibleMasterGameYearViewModel>>> PossibleMasterGames(string gameName, int year, Guid leagueID)
        {
            var currentUserResult = await GetCurrentUser();
            if (currentUserResult.IsFailure)
            {
                return BadRequest(currentUserResult.Error);
            }
            var currentUser = currentUserResult.Value;

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

            var currentDate = _clock.GetToday();
            var matchingGames = await _gameSearchingService.SearchGames(gameName, userPublisher, publishersInLeague, year);
            var viewModels = matchingGames.Select(x => new PossibleMasterGameYearViewModel(x, currentDate)).Take(50).ToList();

            return viewModels;
        }

        public async Task<ActionResult<List<PossibleMasterGameYearViewModel>>> TopAvailableGames(int year, Guid leagueID, string slotInfo)
        {
            var currentUserResult = await GetCurrentUser();
            if (currentUserResult.IsFailure)
            {
                return BadRequest(currentUserResult.Error);
            }
            var currentUser = currentUserResult.Value;

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

            IReadOnlyList<PossibleMasterGameYear> topAvailableGames;
            if (slotInfo is not null)
            {
                string slotInfoJSON = Encoding.UTF8.GetString(Convert.FromBase64String(slotInfo));
                PublisherSingleSlotRequirementsViewModel slotInfoObject = JsonConvert.DeserializeObject<PublisherSingleSlotRequirementsViewModel>(slotInfoJSON);
                var tagDictionary = await _interLeagueService.GetMasterGameTagDictionary();
                var leagueTagRequirements = slotInfoObject.GetLeagueTagStatus(tagDictionary);
                topAvailableGames = await _gameSearchingService.GetTopAvailableGamesForSlot(userPublisher, publishersInLeague, year, leagueTagRequirements);
            }
            else
            {
                topAvailableGames = await _gameSearchingService.GetTopAvailableGames(userPublisher, publishersInLeague, year);
            }

            var currentDate = _clock.GetToday();
            var viewModels = topAvailableGames.Select(x => new PossibleMasterGameYearViewModel(x, currentDate)).Take(100).ToList();

            return viewModels;
        }

        public async Task<ActionResult<List<PublisherGameViewModel>>> PossibleCounterPicks(Guid publisherID)
        {
            var publisher = await _publisherService.GetPublisher(publisherID);
            if (publisher.HasNoValue)
            {
                return BadRequest();
            }

            var publishersInLeague = await _publisherService.GetPublishersInLeagueForYear(publisher.Value.LeagueYear);
            var availableCounterPicks = _draftService.GetAvailableCounterPicks(publisher.Value.LeagueYear, publisher.Value, publishersInLeague);
            var currentDate = _clock.GetToday();
            var viewModels = availableCounterPicks
                .Select(x => new PublisherGameViewModel(x, currentDate, false, false))
                .OrderBy(x => x.GameName).ToList();

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
            if (systemWideSettings.ActionProcessingMode)
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

            var currentUserResult = await GetCurrentUser();
            if (currentUserResult.IsFailure)
            {
                return BadRequest(currentUserResult.Error);
            }
            var currentUser = currentUserResult.Value;

            bool userIsPublisher = (currentUser.Id == publisher.Value.User.Id);
            if (!userIsPublisher)
            {
                return Forbid();
            }

            var publisherGame = publisher.Value.PublisherGames.SingleOrDefault(x => x.PublisherGameID == request.PublisherGameID);
            if (publisherGame is null)
            {
                return BadRequest("That game does not exist for that publisher.");
            }

            DropResult dropResult = await _gameAcquisitionService.MakeDropRequest(publisher.Value, publisherGame, false);
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
            if (systemWideSettings.ActionProcessingMode)
            {
                return BadRequest();
            }

            var dropRequest = await _gameAcquisitionService.GetDropRequest(request.DropRequestID);
            if (dropRequest.HasNoValue)
            {
                return BadRequest("That drop request does not exist.");
            }

            var publisher = dropRequest.Value.Publisher;

            var currentUserResult = await GetCurrentUser();
            if (currentUserResult.IsFailure)
            {
                return BadRequest(currentUserResult.Error);
            }
            var currentUser = currentUserResult.Value;

            bool userIsPublisher = (currentUser.Id == publisher.User.Id);
            if (!userIsPublisher)
            {
                return Forbid();
            }

            Result result = await _gameAcquisitionService.RemoveDropRequest(dropRequest.Value);
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

            var currentUserResult = await GetCurrentUser();
            if (currentUserResult.IsFailure)
            {
                return BadRequest(currentUserResult.Error);
            }
            var currentUser = currentUserResult.Value;
            if (currentUser.Id != publisher.Value.User.Id)
            {
                return Forbid();
            }

            var dropRequests = await _gameAcquisitionService.GetActiveDropRequests(publisher.Value);

            var currentDate = _clock.GetToday();
            var viewModels = dropRequests.Select(x => new DropGameRequestViewModel(x, currentDate));
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

            var currentUserResult = await GetCurrentUser();
            if (currentUserResult.IsFailure)
            {
                return BadRequest(currentUserResult.Error);
            }
            var currentUser = currentUserResult.Value;
            if (currentUser.Id != publisher.Value.User.Id)
            {
                return Forbid();
            }

            var queuedGames = await _publisherService.GetQueuedGames(publisher.Value);

            var publishersInLeague = await _publisherService.GetPublishersInLeagueForYear(publisher.Value.LeagueYear);
            HashSet<MasterGame> publisherMasterGames = publishersInLeague
                .SelectMany(x => x.PublisherGames)
                .Where(x => !x.CounterPick && x.MasterGame.HasValue)
                .Select(x => x.MasterGame.Value.MasterGame)
                .ToHashSet();

            HashSet<MasterGame> myPublisherMasterGames = publisher.Value.MyMasterGames;

            var currentDate = _clock.GetToday();
            var viewModels = queuedGames.Select(x =>
                new QueuedGameViewModel(x, currentDate, publisherMasterGames.Contains(x.MasterGame),
                    myPublisherMasterGames.Contains(x.MasterGame)
                )).OrderBy(x => x.Rank);
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

            var currentUserResult = await GetCurrentUser();
            if (currentUserResult.IsFailure)
            {
                return BadRequest(currentUserResult.Error);
            }
            var currentUser = currentUserResult.Value;
            if (currentUser.Id != publisher.Value.User.Id)
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

            var currentDate = _clock.GetToday();
            var queuedPossibleGames = await _gameSearchingService.GetQueuedPossibleGames(publisher.Value, publishersInLeague, queuedGames);
            var viewModels = queuedPossibleGames.Select(x => new PossibleMasterGameYearViewModel(x, currentDate)).Take(100).ToList();

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

            var currentUserResult = await GetCurrentUser();
            if (currentUserResult.IsFailure)
            {
                return BadRequest(currentUserResult.Error);
            }
            var currentUser = currentUserResult.Value;
            if (currentUser.Id != publisher.Value.User.Id)
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

            Maybe<MasterGame> masterGame = await _interLeagueService.GetMasterGame(request.MasterGameID);
            if (masterGame.HasNoValue)
            {
                return BadRequest("That master game does not exist.");
            }

            ClaimResult bidResult = await _gameAcquisitionService.QueueGame(publisher.Value, masterGame.Value);
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

            var currentUserResult = await GetCurrentUser();
            if (currentUserResult.IsFailure)
            {
                return BadRequest(currentUserResult.Error);
            }
            var currentUser = currentUserResult.Value;

            bool userIsPublisher = (currentUser.Id == publisher.Value.User.Id);
            if (!userIsPublisher)
            {
                return Forbid();
            }

            var queuedGames = await _publisherService.GetQueuedGames(publisher.Value);
            var thisQueuedGame = queuedGames.SingleOrDefault(x => x.MasterGame.MasterGameID == request.MasterGameID);
            if (thisQueuedGame is null)
            {
                return BadRequest();
            }

            await _gameAcquisitionService.RemoveQueuedGame(thisQueuedGame);

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

            var currentUserResult = await GetCurrentUser();
            if (currentUserResult.IsFailure)
            {
                return BadRequest(currentUserResult.Error);
            }
            var currentUser = currentUserResult.Value;
            if (currentUser.Id != publisher.Value.User.Id)
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

        [HttpPost]
        public async Task<IActionResult> ReorderPublisherGames([FromBody] ReorderPublisherGamesRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var systemWideSettings = await _interLeagueService.GetSystemWideSettings();
            if (systemWideSettings.ActionProcessingMode)
            {
                return BadRequest();
            }

            var publisher = await _publisherService.GetPublisher(request.PublisherID);
            if (publisher.HasNoValue)
            {
                return BadRequest();
            }

            var publisherGameIDsBeingEdited = request.SlotStates.Where(x => x.Value.HasValue).Select(x => x.Value.Value);
            var publisherGameIDs = publisher.Value.PublisherGames.Select(x => x.PublisherGameID);
            var gamesNotForPublisher = publisherGameIDsBeingEdited.Except(publisherGameIDs);
            if (gamesNotForPublisher.Any())
            {
                return BadRequest();
            }

            Maybe<LeagueYear> leagueYear = await _fantasyCriticService.GetLeagueYear(publisher.Value.LeagueYear.League.LeagueID, publisher.Value.LeagueYear.Year);
            if (leagueYear.HasNoValue)
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

            var currentUserResult = await GetCurrentUser();
            if (currentUserResult.IsFailure)
            {
                return BadRequest(currentUserResult.Error);
            }
            var currentUser = currentUserResult.Value;

            bool userIsPublisher = (currentUser.Id == publisher.Value.User.Id);
            if (!userIsPublisher)
            {
                return Forbid();
            }

            var reorderResult = await _publisherService.ReorderPublisherGames(publisher.Value, request.SlotStates);
            if (reorderResult.IsFailure)
            {
                return BadRequest(reorderResult.Error);
            }

            await _fantasyCriticService.UpdatePublisherGameCalculatedStats(leagueYear.Value);

            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> SetArchiveStatus([FromBody] SetArchiveStatusRequest request)
        {
            var currentUserResult = await GetCurrentUser();
            if (currentUserResult.IsFailure)
            {
                return BadRequest(currentUserResult.Error);
            }
            var currentUser = currentUserResult.Value;

            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var league = await _fantasyCriticService.GetLeagueByID(request.LeagueID);
            if (league.HasNoValue)
            {
                return BadRequest();
            }

            await _leagueMemberService.SetArchiveStatusForUser(league.Value, request.Archive, currentUser);

            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> DismissManagerMessage([FromBody] DismissManagerMessageRequest request)
        {
            var currentUserResult = await GetCurrentUser();
            if (currentUserResult.IsFailure)
            {
                return BadRequest(currentUserResult.Error);
            }
            var currentUser = currentUserResult.Value;

            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            Result result = await _fantasyCriticService.DismissManagerMessage(request.MessageID, currentUser.Id);
            if (result.IsFailure)
            {
                return BadRequest();
            }

            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> ProposeTrade([FromBody] ProposeTradeRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var systemWideSettings = await _interLeagueService.GetSystemWideSettings();
            if (systemWideSettings.ActionProcessingMode)
            {
                return BadRequest();
            }

            var publisher = await _publisherService.GetPublisher(request.ProposerPublisherID);
            if (publisher.HasNoValue)
            {
                return BadRequest();
            }

            Maybe<LeagueYear> leagueYear = await _fantasyCriticService.GetLeagueYear(publisher.Value.LeagueYear.League.LeagueID, publisher.Value.LeagueYear.Year);
            if (leagueYear.HasNoValue)
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

            var currentUserResult = await GetCurrentUser();
            if (currentUserResult.IsFailure)
            {
                return BadRequest(currentUserResult.Error);
            }
            var currentUser = currentUserResult.Value;

            bool userIsPublisher = (currentUser.Id == publisher.Value.User.Id);
            if (!userIsPublisher)
            {
                return Forbid();
            }

            Result result = await _fantasyCriticService.ProposeTrade(publisher.Value, request.CounterPartyPublisherID, request.ProposerPublisherGameIDs,
                request.CounterPartyPublisherGameIDs, request.ProposerBudgetSendAmount, request.CounterPartyBudgetSendAmount, request.Message);
            if (result.IsFailure)
            {
                return BadRequest(result.Error);
            }

            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> RescindTrade([FromBody] BasicTradeRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var systemWideSettings = await _interLeagueService.GetSystemWideSettings();
            if (systemWideSettings.ActionProcessingMode)
            {
                return BadRequest();
            }

            var trade = await _fantasyCriticService.GetTrade(request.TradeID);
            if (trade.HasNoValue)
            {
                return BadRequest();
            }

            var currentUserResult = await GetCurrentUser();
            if (currentUserResult.IsFailure)
            {
                return BadRequest(currentUserResult.Error);
            }
            var currentUser = currentUserResult.Value;

            bool userIsProposer = (currentUser.Id == trade.Value.Proposer.User.Id);
            if (!userIsProposer)
            {
                return Forbid();
            }

            var supportedYear = (await _interLeagueService.GetSupportedYears()).SingleOrDefault(x => x.Year == trade.Value.Proposer.LeagueYear.Year);
            if (supportedYear is null)
            {
                return BadRequest();
            }

            if (supportedYear.Finished)
            {
                return BadRequest("That year is already finished");
            }

            Result result = await _fantasyCriticService.RescindTrade(trade.Value);
            if (result.IsFailure)
            {
                return BadRequest(result.Error);
            }

            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> AcceptTrade([FromBody] BasicTradeRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var systemWideSettings = await _interLeagueService.GetSystemWideSettings();
            if (systemWideSettings.ActionProcessingMode)
            {
                return BadRequest();
            }

            var trade = await _fantasyCriticService.GetTrade(request.TradeID);
            if (trade.HasNoValue)
            {
                return BadRequest();
            }

            var currentUserResult = await GetCurrentUser();
            if (currentUserResult.IsFailure)
            {
                return BadRequest(currentUserResult.Error);
            }
            var currentUser = currentUserResult.Value;

            bool userIsCounterParty = (currentUser.Id == trade.Value.CounterParty.User.Id);
            if (!userIsCounterParty)
            {
                return Forbid();
            }

            var supportedYear = (await _interLeagueService.GetSupportedYears()).SingleOrDefault(x => x.Year == trade.Value.Proposer.LeagueYear.Year);
            if (supportedYear is null)
            {
                return BadRequest();
            }

            if (supportedYear.Finished)
            {
                return BadRequest("That year is already finished");
            }

            Result result = await _fantasyCriticService.AcceptTrade(trade.Value);
            if (result.IsFailure)
            {
                return BadRequest(result.Error);
            }

            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> RejectTrade([FromBody] BasicTradeRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var systemWideSettings = await _interLeagueService.GetSystemWideSettings();
            if (systemWideSettings.ActionProcessingMode)
            {
                return BadRequest();
            }

            var trade = await _fantasyCriticService.GetTrade(request.TradeID);
            if (trade.HasNoValue)
            {
                return BadRequest();
            }

            var currentUserResult = await GetCurrentUser();
            if (currentUserResult.IsFailure)
            {
                return BadRequest(currentUserResult.Error);
            }
            var currentUser = currentUserResult.Value;

            bool userIsCounterParty = (currentUser.Id == trade.Value.CounterParty.User.Id);
            if (!userIsCounterParty)
            {
                return Forbid();
            }

            var supportedYear = (await _interLeagueService.GetSupportedYears()).SingleOrDefault(x => x.Year == trade.Value.Proposer.LeagueYear.Year);
            if (supportedYear is null)
            {
                return BadRequest();
            }

            if (supportedYear.Finished)
            {
                return BadRequest("That year is already finished");
            }

            Result result = await _fantasyCriticService.RejectTradeByCounterParty(trade.Value);
            if (result.IsFailure)
            {
                return BadRequest(result.Error);
            }

            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> VoteOnTrade([FromBody] TradeVoteRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var systemWideSettings = await _interLeagueService.GetSystemWideSettings();
            if (systemWideSettings.ActionProcessingMode)
            {
                return BadRequest();
            }

            var trade = await _fantasyCriticService.GetTrade(request.TradeID);
            if (trade.HasNoValue)
            {
                return BadRequest();
            }

            var currentUserResult = await GetCurrentUser();
            if (currentUserResult.IsFailure)
            {
                return BadRequest(currentUserResult.Error);
            }
            var currentUser = currentUserResult.Value;

            var publishersInLeague = await _publisherService.GetPublishersInLeagueForYear(trade.Value.Proposer.LeagueYear);
            var validUserIDs = publishersInLeague.Select(x => x.User.Id).Except(new List<Guid>()
                {trade.Value.Proposer.User.Id, trade.Value.CounterParty.User.Id}).ToHashSet();
            bool userIsInLeagueButNotInTrade = validUserIDs.Contains(currentUser.Id);
            if (!userIsInLeagueButNotInTrade)
            {
                return Forbid();
            }

            var supportedYear = (await _interLeagueService.GetSupportedYears()).SingleOrDefault(x => x.Year == trade.Value.Proposer.LeagueYear.Year);
            if (supportedYear is null)
            {
                return BadRequest();
            }

            if (supportedYear.Finished)
            {
                return BadRequest("That year is already finished");
            }

            Result result = await _fantasyCriticService.VoteOnTrade(trade.Value, currentUser, request.Approved, request.Comment);
            if (result.IsFailure)
            {
                return BadRequest(result.Error);
            }

            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> DeleteTradeVote([FromBody] BasicTradeRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var systemWideSettings = await _interLeagueService.GetSystemWideSettings();
            if (systemWideSettings.ActionProcessingMode)
            {
                return BadRequest();
            }

            var trade = await _fantasyCriticService.GetTrade(request.TradeID);
            if (trade.HasNoValue)
            {
                return BadRequest();
            }

            var currentUserResult = await GetCurrentUser();
            if (currentUserResult.IsFailure)
            {
                return BadRequest(currentUserResult.Error);
            }
            var currentUser = currentUserResult.Value;

            var publishersInLeague = await _publisherService.GetPublishersInLeagueForYear(trade.Value.Proposer.LeagueYear);
            var validUserIDs = publishersInLeague.Select(x => x.User.Id).Except(new List<Guid>()
                {trade.Value.Proposer.User.Id, trade.Value.CounterParty.User.Id}).ToHashSet();
            bool userIsInLeagueButNotInTrade = validUserIDs.Contains(currentUser.Id);
            if (!userIsInLeagueButNotInTrade)
            {
                return Forbid();
            }

            var supportedYear = (await _interLeagueService.GetSupportedYears()).SingleOrDefault(x => x.Year == trade.Value.Proposer.LeagueYear.Year);
            if (supportedYear is null)
            {
                return BadRequest();
            }

            if (supportedYear.Finished)
            {
                return BadRequest("That year is already finished");
            }

            Result result = await _fantasyCriticService.DeleteTradeVote(trade.Value, currentUser);
            if (result.IsFailure)
            {
                return BadRequest(result.Error);
            }

            return Ok();
        }

        [AllowAnonymous]
        public async Task<ActionResult<TradeViewModel>> TradeHistory(Guid leagueID, int year)
        {
            Maybe<LeagueYear> leagueYear = await _fantasyCriticService.GetLeagueYear(leagueID, year);
            if (leagueYear.HasNoValue)
            {
                throw new Exception("Something went really wrong, no options are set up for this league.");
            }

            FantasyCriticUser currentUser = null;
            if (!string.IsNullOrWhiteSpace(User.Identity.Name))
            {
                var currentUserResult = await GetCurrentUser();
                if (currentUserResult.IsFailure)
                {
                    return BadRequest(currentUserResult.Error);
                }
                currentUser = currentUserResult.Value;
            }

            var inviteesToLeague = await _leagueMemberService.GetOutstandingInvitees(leagueYear.Value.League);

            bool userIsInLeague = false;
            bool userIsInvitedToLeague = false;
            bool userIsAdmin = false;
            if (currentUser != null)
            {
                var usersInLeague = await _leagueMemberService.GetUsersInLeague(leagueYear.Value.League);
                userIsInLeague = usersInLeague.Any(x => x.Id == currentUser.Id);
                userIsInvitedToLeague = inviteesToLeague.UserIsInvited(currentUser.Email);
                userIsAdmin = await _userManager.IsInRoleAsync(currentUser, "Admin");
            }

            if (!userIsInLeague && !userIsInvitedToLeague && !leagueYear.Value.League.PublicLeague && !userIsAdmin)
            {
                return Forbid();
            }

            var publishersInLeague = await _publisherService.GetPublishersInLeagueForYear(leagueYear.Value);
            var currentDate = _clock.GetToday();
            var trades = await _fantasyCriticService.GetTradesForLeague(leagueYear.Value, publishersInLeague);
            var inactiveTrades = trades.Where(x => !x.Status.IsActive);
            var viewModels = inactiveTrades.Select(x => new TradeViewModel(x, currentDate));
            return Ok(viewModels);
        }

        private IReadOnlyList<SingleGameNewsViewModel> GetGameNewsViewModel(IEnumerable<Publisher> publishers, bool userMode, bool recentReleases)
        {
            var publisherGames = publishers.SelectMany(x => x.PublisherGames).Where(x => x.MasterGame.HasValue);
            var currentDate = _clock.GetToday();
            var yesterday = currentDate.PlusDays(-1);
            var tomorrow = currentDate.PlusDays(1);

            IEnumerable<IGrouping<MasterGameYear, PublisherGame>> orderedByReleaseDate;

            if (recentReleases)
            {
                orderedByReleaseDate = publisherGames
                    .Distinct()
                    .Where(x => x.MasterGame.Value.MasterGame.GetDefiniteMaximumReleaseDate() < tomorrow)
                    .OrderByDescending(x => x.MasterGame.Value.MasterGame.GetDefiniteMaximumReleaseDate())
                    .GroupBy(x => x.MasterGame.Value)
                    .Take(10);
            }
            else
            {
                orderedByReleaseDate = publisherGames
                    .Distinct()
                    .Where(x => x.MasterGame.Value.MasterGame.GetDefiniteMaximumReleaseDate() > yesterday)
                    .OrderBy(x => x.MasterGame.Value.MasterGame.GetDefiniteMaximumReleaseDate())
                    .GroupBy(x => x.MasterGame.Value)
                    .Take(10);
            }


            List<SingleGameNewsViewModel> viewModels = new List<SingleGameNewsViewModel>();
            foreach (var publisherGameGroup in orderedByReleaseDate)
            {
                IEnumerable<Publisher> publishersThatHaveGame = publishers.Where(x => publisherGameGroup.Select(y => y.PublisherID).Contains(x.PublisherID));
                IEnumerable<Publisher> publishersThatHaveStandardGame = publishersThatHaveGame.Where(x => x.PublisherGames.Where(y => !y.CounterPick).Where(x => x.MasterGame.HasValue).Select(y => y.MasterGame.Value).Contains(publisherGameGroup.Key));
                viewModels.Add(new SingleGameNewsViewModel(publisherGameGroup.Key, publishersThatHaveGame, publishersThatHaveStandardGame, userMode, currentDate));
            }

            return viewModels;
        }
    }
}