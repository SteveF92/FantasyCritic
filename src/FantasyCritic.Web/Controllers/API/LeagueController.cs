using System.Text;
using FantasyCritic.Lib.Domain.LeagueActions;
using FantasyCritic.Lib.Domain.Requests;
using FantasyCritic.Lib.Domain.Results;
using FantasyCritic.Lib.Domain.ScoringSystems;
using FantasyCritic.Lib.Domain.Trades;
using FantasyCritic.Lib.Extensions;
using FantasyCritic.Lib.Identity;
using FantasyCritic.Lib.Services;
using FantasyCritic.Web.Helpers;
using FantasyCritic.Web.Hubs;
using FantasyCritic.Web.Models.Requests.League;
using FantasyCritic.Web.Models.Requests.League.Trades;
using FantasyCritic.Web.Models.Requests.Shared;
using FantasyCritic.Web.Models.Responses;
using FantasyCritic.Web.Models.RoundTrip;
using FantasyCritic.Web.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace FantasyCritic.Web.Controllers.API;

[Route("api/[controller]/[action]")]
[Authorize]
public class LeagueController : BaseLeagueController
{
    private readonly DraftService _draftService;
    private readonly GameSearchingService _gameSearchingService;
    private readonly PublisherService _publisherService;
    private readonly IClock _clock;
    private readonly IHubContext<UpdateHub> _hubContext;
    private readonly ILogger<LeagueController> _logger;
    private readonly GameAcquisitionService _gameAcquisitionService;

    public LeagueController(FantasyCriticUserManager userManager, FantasyCriticService fantasyCriticService, InterLeagueService interLeagueService,
        LeagueMemberService leagueMemberService, DraftService draftService, GameSearchingService gameSearchingService, PublisherService publisherService, IClock clock,
        IHubContext<UpdateHub> hubContext, ILogger<LeagueController> logger, GameAcquisitionService gameAcquisitionService) : base(userManager, fantasyCriticService, interLeagueService, leagueMemberService)
    {
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
        var leagueRecord = await GetExistingLeague(id, RequiredRelationship.AllowAnonymous);
        if (leagueRecord.FailedResult is not null)
        {
            return leagueRecord.FailedResult;
        }
        var currentUser = leagueRecord.ValidResult.CurrentUser;
        var league = leagueRecord.ValidResult.League;
        var relationship = leagueRecord.ValidResult.Relationship;

        bool inviteCodeIsValid = false;
        if (inviteCode.HasValue)
        {
            var activeLinks = await _leagueMemberService.GetActiveInviteLinks(league);
            inviteCodeIsValid = activeLinks.Any(x => x.Active && x.InviteCode == inviteCode.Value);
        }

        if (!league.PublicLeague && !relationship.HasPermissionToViewLeague && !inviteCodeIsValid)
        {
            return Forbid();
        }

        bool userIsFollowingLeague = false;
        if (currentUser is not null)
        {
            var leagueFollowers = await _fantasyCriticService.GetLeagueFollowers(league);
            userIsFollowingLeague = leagueFollowers.Any(x => x.Id == currentUser.Id);
        }

        var leagueViewModel = new LeagueViewModel(league, relationship.LeagueManager, leagueRecord.ValidResult.PlayersInLeague,
            relationship.LeagueInvite, currentUser, relationship.InLeague, userIsFollowingLeague);
        return Ok(leagueViewModel);
    }

    [AllowAnonymous]
    public async Task<IActionResult> GetLeagueYear(Guid leagueID, int year, Guid? inviteCode)
    {
        var leagueYearRecord = await GetExistingLeagueYear(leagueID, year, ActionProcessingModeBehavior.Allow, RequiredRelationship.AllowAnonymous, RequiredYearStatus.Any);
        if (leagueYearRecord.FailedResult is not null)
        {
            return leagueYearRecord.FailedResult;
        }
        var leagueYear = leagueYearRecord.ValidResult.LeagueYear;
        var league = leagueYear.League;
        var currentUser = leagueYearRecord.ValidResult.CurrentUser;
        var relationship = leagueYearRecord.ValidResult.Relationship;

        bool inviteCodeIsValid = false;
        if (inviteCode.HasValue)
        {
            var activeLinks = await _leagueMemberService.GetActiveInviteLinks(league);
            inviteCodeIsValid = activeLinks.Any(x => x.Active && x.InviteCode == inviteCode.Value);
        }

        if (!league.PublicLeague && !relationship.HasPermissionToViewLeague && !inviteCodeIsValid)
        {
            return Forbid();
        }

        StartDraftResult startDraftResult = await _draftService.GetStartDraftResult(leagueYear, leagueYearRecord.ValidResult.ActiveUsers);
        Publisher? nextDraftPublisher = _draftService.GetNextDraftPublisher(leagueYear);
        var draftPhase = _draftService.GetDraftPhase(leagueYear);

        SystemWideValues systemWideValues = await _interLeagueService.GetSystemWideValues();
        IReadOnlyList<ManagerMessage> managerMessages = await _fantasyCriticService.GetManagerMessages(leagueYear);

        FantasyCriticUser? previousYearWinner = await _fantasyCriticService.GetPreviousYearWinner(leagueYear);
        var publicBiddingGames = await _gameAcquisitionService.GetPublicBiddingGames(leagueYear);
        IReadOnlySet<Guid> counterPickedPublisherGameIDs = GameUtilities.GetCounterPickedPublisherGameIDs(leagueYear);

        IReadOnlyList<Trade> activeTrades = await _fantasyCriticService.GetActiveTradesForLeague(leagueYear);

        bool userIsFollowingLeague = false;
        Publisher? userPublisher = null;
        if (currentUser is not null)
        {
            var leagueFollowers = await _fantasyCriticService.GetLeagueFollowers(league);
            userIsFollowingLeague = leagueFollowers.Any(x => x.Id == currentUser.Id);
            userPublisher = leagueYear.GetUserPublisher(currentUser);
        }

        var currentDate = _clock.GetToday();
        PrivatePublisherDataViewModel? privatePublisherData = null;
        if (userPublisher is not null)
        {
            var bids = await _gameAcquisitionService.GetActiveAcquisitionBids(leagueYear, userPublisher);
            var dropRequests = await _gameAcquisitionService.GetActiveDropRequests(leagueYear, userPublisher);
            var queuedGames = await _publisherService.GetQueuedGames(userPublisher);
            privatePublisherData = new PrivatePublisherDataViewModel(leagueYear, userPublisher, bids, dropRequests, queuedGames, currentDate);
        }

        var upcomingGames = GetGameNewsViewModel(leagueYear, false, false).ToList();
        var recentGames = GetGameNewsViewModel(leagueYear, false, true).ToList();
        var gameNewsViewModel = new GameNewsViewModel(upcomingGames, recentGames);

        var leagueViewModel = new LeagueViewModel(league, relationship.LeagueManager, leagueYearRecord.ValidResult.PlayersInLeague,
            relationship.LeagueInvite, currentUser, relationship.InLeague, userIsFollowingLeague);

        var leagueYearViewModel = new LeagueYearViewModel(leagueViewModel, leagueYear, currentDate,
            startDraftResult, leagueYearRecord.ValidResult.ActiveUsers, nextDraftPublisher, draftPhase, systemWideValues,
            leagueYearRecord.ValidResult.InvitedPlayers, relationship.InLeague, relationship.InvitedToLeague, relationship.LeagueManager,
            currentUser, managerMessages, previousYearWinner, publicBiddingGames, counterPickedPublisherGameIDs, activeTrades, privatePublisherData, gameNewsViewModel);
        return Ok(leagueYearViewModel);
    }

    [AllowAnonymous]
    public async Task<IActionResult> GetLeagueActions(Guid leagueID, int year)
    {
        var leagueYearRecord = await GetExistingLeagueYear(leagueID, year, ActionProcessingModeBehavior.Allow, RequiredRelationship.AllowAnonymous, RequiredYearStatus.Any);
        if (leagueYearRecord.FailedResult is not null)
        {
            return leagueYearRecord.FailedResult;
        }
        var leagueYear = leagueYearRecord.ValidResult.LeagueYear;
        var league = leagueYear.League;
        var relationship = leagueYearRecord.ValidResult.Relationship;

        if (!league.PublicLeague && !relationship.HasPermissionToViewLeague)
        {
            return Forbid();
        }

        var leagueActions = await _fantasyCriticService.GetLeagueActions(leagueYear);

        var viewModels = leagueActions.Select(x => new LeagueActionViewModel(leagueYear, x));
        viewModels = viewModels.OrderByDescending(x => x.Timestamp);
        return Ok(viewModels);
    }

    [AllowAnonymous]
    public async Task<IActionResult> GetLeagueActionSets(Guid leagueID, int year)
    {
        var leagueYearRecord = await GetExistingLeagueYear(leagueID, year, ActionProcessingModeBehavior.Allow, RequiredRelationship.AllowAnonymous, RequiredYearStatus.Any);
        if (leagueYearRecord.FailedResult is not null)
        {
            return leagueYearRecord.FailedResult;
        }
        var leagueYear = leagueYearRecord.ValidResult.LeagueYear;
        var league = leagueYear.League;
        var relationship = leagueYearRecord.ValidResult.Relationship;

        if (!league.PublicLeague && !relationship.HasPermissionToViewLeague)
        {
            return Forbid();
        }

        var leagueActionSets = await _fantasyCriticService.GetLeagueActionProcessingSets(leagueYear);

        var currentDate = _clock.GetToday();
        var viewModels = leagueActionSets.Where(x => x.HasActions).Select(x => new LeagueActionProcessingSetViewModel(x, currentDate));
        viewModels = viewModels.OrderByDescending(x => x.ProcessTime);
        return Ok(viewModels);
    }

    [AllowAnonymous]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetPublisher(Guid id)
    {
        var publisherRecord = await GetExistingLeagueYearAndPublisher(id, ActionProcessingModeBehavior.Allow, RequiredRelationship.AllowAnonymous, RequiredYearStatus.Any);
        if (publisherRecord.FailedResult is not null)
        {
            return publisherRecord.FailedResult;
        }
        var leagueYear = publisherRecord.ValidResult.LeagueYear;
        var league = leagueYear.League;
        var relationship = publisherRecord.ValidResult.Relationship;
        var publisher = publisherRecord.ValidResult.Publisher;

        if (!league.PublicLeague && !relationship.HasPermissionToViewLeague)
        {
            return Forbid();
        }

        SystemWideValues systemWideValues = await _interLeagueService.GetSystemWideValues();
        IReadOnlySet<Guid> counterPickedPublisherGameIDs = GameUtilities.GetCounterPickedPublisherGameIDs(leagueYear);

        var currentDate = _clock.GetToday();
        var publisherViewModel = new PublisherViewModel(leagueYear, publisher, currentDate, relationship.InLeague,
            relationship.InvitedToLeague, systemWideValues, counterPickedPublisherGameIDs);
        return Ok(publisherViewModel);
    }

    [AllowAnonymous]
    public async Task<IActionResult> GetLeagueYearOptions(Guid leagueID, int year)
    {
        var leagueYearRecord = await GetExistingLeagueYear(leagueID, year, ActionProcessingModeBehavior.Allow, RequiredRelationship.AllowAnonymous, RequiredYearStatus.Any);
        if (leagueYearRecord.FailedResult is not null)
        {
            return leagueYearRecord.FailedResult;
        }
        var leagueYear = leagueYearRecord.ValidResult.LeagueYear;
        var league = leagueYear.League;
        var relationship = leagueYearRecord.ValidResult.Relationship;

        if (!league.PublicLeague && !relationship.HasPermissionToViewLeague)
        {
            return Forbid();
        }

        var leagueViewModel = new LeagueYearSettingsViewModel(leagueYear);
        return Ok(leagueViewModel);
    }

    [HttpPost]
    public async Task<IActionResult> AcceptInvite([FromBody] AcceptInviteRequest request)
    {
        var leagueRecord = await GetExistingLeague(request.LeagueID, RequiredRelationship.LoggedIn);
        if (leagueRecord.FailedResult is not null)
        {
            return leagueRecord.FailedResult;
        }
        var currentUser = leagueRecord.ValidResult.CurrentUser;
        var league = leagueRecord.ValidResult.League;

        if (!currentUser.EmailConfirmed)
        {
            return BadRequest();
        }

        Result result = await _leagueMemberService.AcceptInvite(league, currentUser);
        if (result.IsFailure)
        {
            return BadRequest(result.Error);
        }

        return Ok();
    }

    [HttpPost]
    public async Task<IActionResult> JoinWithInviteLink([FromBody] JoinWithInviteLinkRequest request)
    {
        var leagueRecord = await GetExistingLeague(request.LeagueID, RequiredRelationship.LoggedIn);
        if (leagueRecord.FailedResult is not null)
        {
            return leagueRecord.FailedResult;
        }
        var currentUser = leagueRecord.ValidResult.CurrentUser;
        var league = leagueRecord.ValidResult.League;

        var inviteLink = await _leagueMemberService.GetInviteLinkByInviteCode(request.InviteCode);
        if (inviteLink is null)
        {
            return BadRequest();
        }

        if (league.LeagueID != request.LeagueID)
        {
            return BadRequest();
        }

        var mostRecentYear = await _fantasyCriticService.GetLeagueYear(league.LeagueID, league.Years.Max());
        if (mostRecentYear is null)
        {
            return BadRequest();
        }

        bool mostRecentYearIsStarted = mostRecentYear.PlayStatus.PlayStarted;
        if (mostRecentYearIsStarted)
        {
            return BadRequest();
        }

        Result result = await _leagueMemberService.AcceptInviteLink(inviteLink, currentUser);
        if (result.IsFailure)
        {
            return BadRequest(result.Error);
        }

        return Ok();
    }

    [HttpPost]
    public async Task<IActionResult> CreatePublisher([FromBody] CreatePublisherRequest request)
    {
        var leagueYearRecord = await GetExistingLeagueYear(request.LeagueID, request.Year, ActionProcessingModeBehavior.Allow, RequiredRelationship.ActiveInYear, RequiredYearStatus.YearNotFinishedDraftNotStarted);
        if (leagueYearRecord.FailedResult is not null)
        {
            return leagueYearRecord.FailedResult;
        }
        var leagueYear = leagueYearRecord.ValidResult.LeagueYear;
        var currentUser = leagueYearRecord.ValidResult.CurrentUser;

        if (string.IsNullOrWhiteSpace(request.PublisherName))
        {
            return BadRequest("You cannot have a blank name.");
        }

        if (leagueYear.PlayStatus.PlayStarted)
        {
            return BadRequest();
        }

        var publisherForUser = leagueYear.GetUserPublisher(currentUser);
        if (publisherForUser != null)
        {
            return BadRequest("You have already created a publisher for this this league/year.");
        }

        await _publisherService.CreatePublisher(leagueYear, currentUser, request.PublisherName);
        return Ok();
    }

    [HttpPost]
    public async Task<IActionResult> ChangePublisherName([FromBody] ChangePublisherNameRequest request)
    {
        var publisherRecord = await GetExistingLeagueYearAndPublisher(request.PublisherID, ActionProcessingModeBehavior.Allow, RequiredRelationship.BePublisher, RequiredYearStatus.Any);
        if (publisherRecord.FailedResult is not null)
        {
            return publisherRecord.FailedResult;
        }
        var publisher = publisherRecord.ValidResult.Publisher;

        if (string.IsNullOrWhiteSpace(request.PublisherName))
        {
            return BadRequest("You cannot have a blank name.");
        }

        await _publisherService.ChangePublisherName(publisher, request.PublisherName);
        return Ok();
    }

    [HttpPost]
    [Authorize(Roles = "PlusUser")]
    public async Task<IActionResult> ChangePublisherIcon([FromBody] ChangePublisherIconRequest request)
    {
        var publisherRecord = await GetExistingLeagueYearAndPublisher(request.PublisherID, ActionProcessingModeBehavior.Allow, RequiredRelationship.BePublisher, RequiredYearStatus.Any);
        if (publisherRecord.FailedResult is not null)
        {
            return publisherRecord.FailedResult;
        }
        var publisher = publisherRecord.ValidResult!.Publisher;

        await _publisherService.ChangePublisherIcon(publisher, request.PublisherIcon);
        return Ok();
    }

    [HttpPost]
    public async Task<IActionResult> SetAutoDraft([FromBody] SetAutoDraftRequest request)
    {
        var publisherRecord = await GetExistingLeagueYearAndPublisher(request.PublisherID, ActionProcessingModeBehavior.Allow, RequiredRelationship.BePublisher, RequiredYearStatus.YearNotFinishedDraftNotFinished);
        if (publisherRecord.FailedResult is not null)
        {
            return publisherRecord.FailedResult;
        }
        var leagueYear = publisherRecord.ValidResult.LeagueYear;
        var publisher = publisherRecord.ValidResult.Publisher;

        await _publisherService.SetAutoDraft(publisher, request.AutoDraft);

        var draftComplete = await _draftService.RunAutoDraftAndCheckIfComplete(leagueYear);
        if (draftComplete)
        {
            await _hubContext.Clients.Group(leagueYear.GetGroupName).SendAsync("DraftFinished");
        }

        return Ok();
    }

    [HttpPost]
    public async Task<IActionResult> DeclineInvite([FromBody] DeleteInviteRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest();
        }

        var currentUserResult = await GetCurrentUser();
        if (currentUserResult.IsFailure)
        {
            return BadRequest(currentUserResult.Error);
        }
        var currentUser = currentUserResult.Value;

        LeagueInvite? invite = await _leagueMemberService.GetInvite(request.InviteID);
        if (invite is null)
        {
            return BadRequest();
        }

        if (!string.Equals(invite.EmailAddress, currentUser.Email, StringComparison.OrdinalIgnoreCase))
        {
            return Forbid();
        }

        await _leagueMemberService.DeleteInvite(invite);
        return Ok();
    }

    [HttpPost]
    public async Task<IActionResult> MakePickupBid([FromBody] PickupBidRequest request)
    {
        var publisherRecord = await GetExistingLeagueYearAndPublisher(request.PublisherID, ActionProcessingModeBehavior.Ban, RequiredRelationship.BePublisher, RequiredYearStatus.YearNotFinishedDraftFinished);
        if (publisherRecord.FailedResult is not null)
        {
            return publisherRecord.FailedResult;
        }
        var leagueYear = publisherRecord.ValidResult.LeagueYear;
        var publisher = publisherRecord.ValidResult.Publisher;

        var masterGame = await _interLeagueService.GetMasterGame(request.MasterGameID);
        if (masterGame is null)
        {
            return BadRequest("That master game does not exist.");
        }

        var publicBiddingGames = await _gameAcquisitionService.GetPublicBiddingGames(leagueYear);
        bool publicBidIsValid = _gameAcquisitionService.PublicBidIsValid(leagueYear, masterGame, request.CounterPick, publicBiddingGames);
        if (!publicBidIsValid)
        {
            return BadRequest("During the public bidding window, you can only bid on a game that is already being bid on by at least one player.");
        }

        PublisherGame? conditionalDropPublisherGame = null;
        if (request.ConditionalDropPublisherGameID.HasValue)
        {
            conditionalDropPublisherGame = publisher.GetPublisherGameByPublisherGameID(request.ConditionalDropPublisherGameID.Value);
        }

        ClaimResult bidResult = await _gameAcquisitionService.MakePickupBid(leagueYear, publisher, masterGame, conditionalDropPublisherGame, request.CounterPick, request.BidAmount);
        var viewModel = new PickupBidResultViewModel(bidResult);

        return Ok(viewModel);
    }

    [HttpPost]
    public async Task<IActionResult> EditPickupBid([FromBody] PickupBidEditRequest request)
    {
        var publisherRecord = await GetExistingLeagueYearAndPublisher(request.PublisherID, ActionProcessingModeBehavior.Ban, RequiredRelationship.BePublisher, RequiredYearStatus.YearNotFinishedDraftFinished);
        if (publisherRecord.FailedResult is not null)
        {
            return publisherRecord.FailedResult;
        }
        var publisher = publisherRecord.ValidResult.Publisher;

        var maybeBid = await _gameAcquisitionService.GetPickupBid(request.BidID);
        if (maybeBid is null)
        {
            return BadRequest("That bid does not exist.");
        }

        if (maybeBid.Publisher.PublisherID != publisher.PublisherID)
        {
            return Forbid();
        }

        PublisherGame? conditionalDropPublisherGame = null;
        if (request.ConditionalDropPublisherGameID.HasValue)
        {
            conditionalDropPublisherGame = publisher.GetPublisherGameByPublisherGameID(request.ConditionalDropPublisherGameID.Value);
        }

        ClaimResult bidResult = await _gameAcquisitionService.EditPickupBid(maybeBid, conditionalDropPublisherGame, request.BidAmount);
        var viewModel = new PickupBidResultViewModel(bidResult);

        return Ok(viewModel);
    }

    [HttpPost]
    public async Task<IActionResult> DeletePickupBid([FromBody] PickupBidDeleteRequest request)
    {
        var publisherRecord = await GetExistingLeagueYearAndPublisher(request.PublisherID, ActionProcessingModeBehavior.Ban, RequiredRelationship.BePublisher, RequiredYearStatus.YearNotFinishedDraftFinished);
        if (publisherRecord.FailedResult is not null)
        {
            return publisherRecord.FailedResult;
        }
        var leagueYear = publisherRecord.ValidResult.LeagueYear;
        var publisher = publisherRecord.ValidResult.Publisher;

        var maybeBid = await _gameAcquisitionService.GetPickupBid(request.BidID);
        if (maybeBid is null)
        {
            return BadRequest("That bid does not exist.");
        }

        if (maybeBid.Publisher.PublisherID != publisher.PublisherID)
        {
            return Forbid();
        }

        bool canCancel = _gameAcquisitionService.CanCancelBid(leagueYear, maybeBid.CounterPick);
        if (!canCancel)
        {
            return BadRequest("Can't cancel a bid when in the public bidding window.");
        }

        PickupBid bid = maybeBid;
        Result result = await _gameAcquisitionService.RemovePickupBid(bid);
        if (result.IsFailure)
        {
            return BadRequest(result.Error);
        }

        return Ok();
    }

    [HttpPost]
    public async Task<IActionResult> SetBidPriorities([FromBody] BidPriorityOrderRequest request)
    {
        var publisherRecord = await GetExistingLeagueYearAndPublisher(request.PublisherID, ActionProcessingModeBehavior.Ban, RequiredRelationship.BePublisher, RequiredYearStatus.YearNotFinishedDraftFinished);
        if (publisherRecord.FailedResult is not null)
        {
            return publisherRecord.FailedResult;
        }
        var leagueYear = publisherRecord.ValidResult.LeagueYear;
        var publisher = publisherRecord.ValidResult.Publisher;

        var activeBids = await _gameAcquisitionService.GetActiveAcquisitionBids(leagueYear, publisher);
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
        var publisherRecord = await GetExistingLeagueYearAndPublisher(request.PublisherID, ActionProcessingModeBehavior.Allow, RequiredRelationship.BePublisher, RequiredYearStatus.ActiveDraft);
        if (publisherRecord.FailedResult is not null)
        {
            return publisherRecord.FailedResult;
        }
        var leagueYear = publisherRecord.ValidResult.LeagueYear;
        var publisher = publisherRecord.ValidResult.Publisher;

        var nextPublisher = _draftService.GetNextDraftPublisher(leagueYear);
        if (nextPublisher is null)
        {
            return BadRequest("There are no spots open to draft.");
        }

        if (!nextPublisher.Equals(publisher))
        {
            return BadRequest("It is not your turn to draft.");
        }

        MasterGame? masterGame = null;
        if (request.MasterGameID.HasValue)
        {
            masterGame = await _interLeagueService.GetMasterGame(request.MasterGameID.Value);
        }

        var draftPhase = _draftService.GetDraftPhase(leagueYear);
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

        var draftStatus = _draftService.GetDraftStatus(draftPhase, leagueYear);
        ClaimGameDomainRequest domainRequest = new ClaimGameDomainRequest(leagueYear, publisher, request.GameName, request.CounterPick, false, false, false, masterGame,
            draftStatus.DraftPosition, draftStatus.OverallDraftPosition);

        var draftResult = await _draftService.DraftGame(domainRequest, false);
        var viewModel = new PlayerClaimResultViewModel(draftResult.Result);
        await _hubContext.Clients.Group(leagueYear.GetGroupName).SendAsync("RefreshLeagueYear");

        if (draftResult.DraftComplete)
        {
            await _hubContext.Clients.Group(leagueYear.GetGroupName).SendAsync("DraftFinished");
        }

        return Ok(viewModel);
    }

    [HttpPost]
    public async Task<IActionResult> FollowLeague([FromBody] FollowLeagueRequest request)
    {
        var leagueRecord = await GetExistingLeague(request.LeagueID, RequiredRelationship.LoggedIn);
        if (leagueRecord.FailedResult is not null)
        {
            return leagueRecord.FailedResult;
        }
        var currentUser = leagueRecord.ValidResult.CurrentUser;
        var league = leagueRecord.ValidResult.League;

        Result result = await _fantasyCriticService.FollowLeague(league, currentUser);
        if (result.IsFailure)
        {
            return BadRequest(result.Error);
        }

        return Ok();
    }

    [HttpPost]
    public async Task<IActionResult> UnfollowLeague([FromBody] FollowLeagueRequest request)
    {
        var leagueRecord = await GetExistingLeague(request.LeagueID, RequiredRelationship.LoggedIn);
        if (leagueRecord.FailedResult is not null)
        {
            return leagueRecord.FailedResult;
        }
        var currentUser = leagueRecord.ValidResult.CurrentUser;
        var league = leagueRecord.ValidResult.League;

        Result result = await _fantasyCriticService.UnfollowLeague(league, currentUser);
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

        List<LeagueYearPublisherPair> myPublishers = new List<LeagueYearPublisherPair>();
        foreach (var year in activeYears)
        {
            IReadOnlyList<LeagueYear> activeLeagueYears = await _leagueMemberService.GetLeaguesYearsForUser(currentUser, year.Year);

            foreach (var leagueYear in activeLeagueYears)
            {
                if (leagueYear.League.TestLeague)
                {
                    continue;
                }

                var userPublisher = leagueYear.GetUserPublisher(currentUser);
                if (userPublisher is null)
                {
                    continue;
                }

                myPublishers.Add(new LeagueYearPublisherPair(leagueYear, userPublisher));
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

        List<LeagueYearPublisherPair> myPublishers = new List<LeagueYearPublisherPair>();
        foreach (var year in activeYears)
        {
            IReadOnlyList<LeagueYear> activeLeagueYears = await _leagueMemberService.GetLeaguesYearsForUser(currentUser, year.Year);

            foreach (var leagueYear in activeLeagueYears)
            {
                if (leagueYear.League.TestLeague)
                {
                    continue;
                }

                var userPublisher = leagueYear.GetUserPublisher(currentUser);
                if (userPublisher is null)
                {
                    continue;
                }

                myPublishers.Add(new LeagueYearPublisherPair(leagueYear, userPublisher));
            }
        }

        var upcomingGames = GetGameNewsViewModel(myPublishers, true, false).ToList();
        var recentGames = GetGameNewsViewModel(myPublishers, true, true).ToList();
        return new GameNewsViewModel(upcomingGames, recentGames);
    }

    [AllowAnonymous]
    public async Task<IActionResult> LeagueUpcomingGames(Guid leagueID, int year)
    {
        var leagueYearRecord = await GetExistingLeagueYear(leagueID, year, ActionProcessingModeBehavior.Allow, RequiredRelationship.AllowAnonymous, RequiredYearStatus.Any);
        if (leagueYearRecord.FailedResult is not null)
        {
            return leagueYearRecord.FailedResult;
        }
        var leagueYear = leagueYearRecord.ValidResult.LeagueYear;
        var league = leagueYear.League;
        var relationship = leagueYearRecord.ValidResult.Relationship;

        if (!league.PublicLeague && !relationship.HasPermissionToViewLeague)
        {
            return Forbid();
        }

        var viewModels = GetGameNewsViewModel(leagueYear, false, false).ToList();
        return Ok(viewModels);
    }

    public async Task<IActionResult> PossibleMasterGames(string gameName, int year, Guid leagueID)
    {
        var leagueYearRecord = await GetExistingLeagueYear(leagueID, year, ActionProcessingModeBehavior.Allow, RequiredRelationship.ActiveInYear, RequiredYearStatus.Any);
        if (leagueYearRecord.FailedResult is not null)
        {
            return leagueYearRecord.FailedResult;
        }
        var leagueYear = leagueYearRecord.ValidResult.LeagueYear;
        var currentUser = leagueYearRecord.ValidResult.CurrentUser;

        var userPublisher = leagueYear.GetUserPublisher(currentUser);
        if (userPublisher is null)
        {
            return BadRequest();
        }

        var currentDate = _clock.GetToday();
        var matchingGames = await _gameSearchingService.SearchGames(gameName, leagueYear, userPublisher, year);
        var viewModels = matchingGames.Select(x => new PossibleMasterGameYearViewModel(x, currentDate)).Take(50).ToList();

        return Ok(viewModels);
    }

    public async Task<IActionResult> TopAvailableGames(int year, Guid leagueID, string slotInfo)
    {
        var leagueYearRecord = await GetExistingLeagueYear(leagueID, year, ActionProcessingModeBehavior.Allow, RequiredRelationship.ActiveInYear, RequiredYearStatus.Any);
        if (leagueYearRecord.FailedResult is not null)
        {
            return leagueYearRecord.FailedResult;
        }
        var leagueYear = leagueYearRecord.ValidResult.LeagueYear;
        var currentUser = leagueYearRecord.ValidResult.CurrentUser;

        var userPublisher = leagueYear.GetUserPublisher(currentUser);
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
            topAvailableGames = await _gameSearchingService.GetTopAvailableGamesForSlot(leagueYear, userPublisher, leagueTagRequirements);
        }
        else
        {
            topAvailableGames = await _gameSearchingService.GetTopAvailableGames(leagueYear, userPublisher);
        }

        var currentDate = _clock.GetToday();
        var viewModels = topAvailableGames.Select(x => new PossibleMasterGameYearViewModel(x, currentDate)).Take(100).ToList();

        return Ok(viewModels);
    }

    public async Task<IActionResult> PossibleCounterPicks(Guid publisherID)
    {
        var publisherRecord = await GetExistingLeagueYearAndPublisher(publisherID, ActionProcessingModeBehavior.Allow, RequiredRelationship.ActiveInYear, RequiredYearStatus.Any);
        if (publisherRecord.FailedResult is not null)
        {
            return publisherRecord.FailedResult;
        }
        var leagueYear = publisherRecord.ValidResult.LeagueYear;
        var publisher = publisherRecord.ValidResult.Publisher;

        var availableCounterPicks = _draftService.GetAvailableCounterPicks(leagueYear, publisher);
        var currentDate = _clock.GetToday();
        var viewModels = availableCounterPicks
            .Select(x => new PublisherGameViewModel(x, currentDate, false, false))
            .OrderBy(x => x.GameName).ToList();

        return Ok(viewModels);
    }

    [HttpPost]
    public async Task<IActionResult> MakeDropRequest([FromBody] DropGameRequestRequest request)
    {
        var publisherRecord = await GetExistingLeagueYearAndPublisherGame(request.PublisherID, request.PublisherGameID, ActionProcessingModeBehavior.Ban, RequiredRelationship.BePublisher, RequiredYearStatus.YearNotFinishedDraftFinished);
        if (publisherRecord.FailedResult is not null)
        {
            return publisherRecord.FailedResult;
        }
        var leagueYear = publisherRecord.ValidResult.LeagueYear;
        var publisher = publisherRecord.ValidResult.Publisher;
        var publisherGame = publisherRecord.ValidResult.PublisherGame;

        DropResult dropResult = await _gameAcquisitionService.MakeDropRequest(leagueYear, publisher, publisherGame, false);
        var viewModel = new DropGameResultViewModel(dropResult);

        return Ok(viewModel);
    }

    [HttpPost]
    public async Task<IActionResult> DeleteDropRequest([FromBody] DropGameRequestDeleteRequest request)
    {
        var publisherRecord = await GetExistingLeagueYearAndPublisher(request.PublisherID, ActionProcessingModeBehavior.Ban, RequiredRelationship.BePublisher, RequiredYearStatus.YearNotFinishedDraftFinished);
        if (publisherRecord.FailedResult is not null)
        {
            return publisherRecord.FailedResult;
        }
        var publisher = publisherRecord.ValidResult.Publisher;

        var dropRequest = await _gameAcquisitionService.GetDropRequest(request.DropRequestID);
        if (dropRequest is null)
        {
            return BadRequest("That drop request does not exist.");
        }

        if (dropRequest.Publisher.PublisherID != publisher.PublisherID)
        {
            return Forbid();
        }

        Result result = await _gameAcquisitionService.RemoveDropRequest(dropRequest);
        if (result.IsFailure)
        {
            return BadRequest(result.Error);
        }

        return Ok();
    }

    

    [HttpGet("{publisherID}")]
    public async Task<IActionResult> CurrentQueuedGameYears(Guid publisherID)
    {
        var publisherRecord = await GetExistingLeagueYearAndPublisher(publisherID, ActionProcessingModeBehavior.Allow, RequiredRelationship.BePublisher, RequiredYearStatus.Any);
        if (publisherRecord.FailedResult is not null)
        {
            return publisherRecord.FailedResult;
        }
        var leagueYear = publisherRecord.ValidResult.LeagueYear;
        var publisher = publisherRecord.ValidResult.Publisher;

        var queuedGames = await _publisherService.GetQueuedGames(publisher);

        var currentDate = _clock.GetToday();
        var queuedPossibleGames = await _gameSearchingService.GetQueuedPossibleGames(leagueYear, publisher, queuedGames);
        var viewModels = queuedPossibleGames.Select(x => new PossibleMasterGameYearViewModel(x, currentDate)).Take(100).ToList();

        return Ok(viewModels);
    }

    [HttpPost]
    public async Task<IActionResult> AddGameToQueue([FromBody] AddGameToQueueRequest request)
    {
        var publisherRecord = await GetExistingLeagueYearAndPublisher(request.PublisherID, ActionProcessingModeBehavior.Allow, RequiredRelationship.BePublisher, RequiredYearStatus.AnyYearNotFinished);
        if (publisherRecord.FailedResult is not null)
        {
            return publisherRecord.FailedResult;
        }
        var leagueYear = publisherRecord.ValidResult.LeagueYear;
        var publisher = publisherRecord.ValidResult.Publisher;

        MasterGame? masterGame = await _interLeagueService.GetMasterGame(request.MasterGameID);
        if (masterGame is null)
        {
            return BadRequest("That master game does not exist.");
        }

        ClaimResult bidResult = await _gameAcquisitionService.QueueGame(leagueYear, publisher, masterGame);
        var viewModel = new QueueResultViewModel(bidResult);

        return Ok(viewModel);
    }

    [HttpPost]
    public async Task<IActionResult> DeleteQueuedGame([FromBody] QueuedGameDeleteRequest request)
    {
        var publisherRecord = await GetExistingLeagueYearAndPublisher(request.PublisherID, ActionProcessingModeBehavior.Allow, RequiredRelationship.BePublisher, RequiredYearStatus.AnyYearNotFinished);
        if (publisherRecord.FailedResult is not null)
        {
            return publisherRecord.FailedResult;
        }
        var publisher = publisherRecord.ValidResult.Publisher;

        var queuedGames = await _publisherService.GetQueuedGames(publisher);
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
        var publisherRecord = await GetExistingLeagueYearAndPublisher(request.PublisherID, ActionProcessingModeBehavior.Allow, RequiredRelationship.BePublisher, RequiredYearStatus.AnyYearNotFinished);
        if (publisherRecord.FailedResult is not null)
        {
            return publisherRecord.FailedResult;
        }
        var publisher = publisherRecord.ValidResult.Publisher;

        var queuedGames = await _publisherService.GetQueuedGames(publisher);
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
        var publisherRecord = await GetExistingLeagueYearAndPublisher(request.PublisherID, ActionProcessingModeBehavior.Ban, RequiredRelationship.BePublisher, RequiredYearStatus.AnyYearNotFinished);
        if (publisherRecord.FailedResult is not null)
        {
            return publisherRecord.FailedResult;
        }
        var leagueYear = publisherRecord.ValidResult.LeagueYear;
        var publisher = publisherRecord.ValidResult.Publisher;

        var publisherGameIDsBeingEdited = request.SlotStates.Where(x => x.Value.HasValue).Select(x => x.Value.Value);
        var publisherGameIDs = publisher.PublisherGames.Select(x => x.PublisherGameID);
        var gamesNotForPublisher = publisherGameIDsBeingEdited.Except(publisherGameIDs);
        if (gamesNotForPublisher.Any())
        {
            return BadRequest();
        }

        var reorderResult = await _publisherService.ReorderPublisherGames(leagueYear, publisher, request.SlotStates);
        if (reorderResult.IsFailure)
        {
            return BadRequest(reorderResult.Error);
        }

        await _fantasyCriticService.UpdatePublisherGameCalculatedStats(leagueYear);

        return Ok();
    }

    [HttpPost]
    public async Task<IActionResult> SetArchiveStatus([FromBody] SetArchiveStatusRequest request)
    {
        var leagueRecord = await GetExistingLeague(request.LeagueID, RequiredRelationship.InLeague);
        if (leagueRecord.FailedResult is not null)
        {
            return leagueRecord.FailedResult;
        }
        var currentUser = leagueRecord.ValidResult.CurrentUser;
        var league = leagueRecord.ValidResult.League;

        await _leagueMemberService.SetArchiveStatusForUser(league, request.Archive, currentUser);

        return Ok();
    }

    [HttpPost]
    public async Task<IActionResult> DismissManagerMessage([FromBody] DismissManagerMessageRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest();
        }

        var currentUserResult = await GetCurrentUser();
        if (currentUserResult.IsFailure)
        {
            return BadRequest(currentUserResult.Error);
        }
        var currentUser = currentUserResult.Value;

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
        var publisherRecord = await GetExistingLeagueYearAndPublisher(request.ProposerPublisherID, ActionProcessingModeBehavior.Allow, RequiredRelationship.BePublisher, RequiredYearStatus.YearNotFinishedDraftFinished);
        if (publisherRecord.FailedResult is not null)
        {
            return publisherRecord.FailedResult;
        }
        var leagueYear = publisherRecord.ValidResult.LeagueYear;
        var publisher = publisherRecord.ValidResult.Publisher;

        Result result = await _fantasyCriticService.ProposeTrade(leagueYear, publisher, request.CounterPartyPublisherID, request.ProposerPublisherGameIDs,
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
        var leagueYearRecord = await GetExistingLeagueYear(request.LeagueID, request.Year, ActionProcessingModeBehavior.Allow, RequiredRelationship.ActiveInYear, RequiredYearStatus.YearNotFinishedDraftFinished);
        if (leagueYearRecord.FailedResult is not null)
        {
            return leagueYearRecord.FailedResult;
        }
        var currentUser = leagueYearRecord.ValidResult.CurrentUser;

        var trade = await _fantasyCriticService.GetTrade(request.TradeID);
        if (trade is null)
        {
            return BadRequest();
        }

        bool userIsProposer = (currentUser.Id == trade.Proposer.User.Id);
        if (!userIsProposer)
        {
            return Forbid();
        }

        Result result = await _fantasyCriticService.RescindTrade(trade);
        if (result.IsFailure)
        {
            return BadRequest(result.Error);
        }

        return Ok();
    }

    [HttpPost]
    public async Task<IActionResult> AcceptTrade([FromBody] BasicTradeRequest request)
    {
        var leagueYearRecord = await GetExistingLeagueYear(request.LeagueID, request.Year, ActionProcessingModeBehavior.Allow, RequiredRelationship.ActiveInYear, RequiredYearStatus.YearNotFinishedDraftFinished);
        if (leagueYearRecord.FailedResult is not null)
        {
            return leagueYearRecord.FailedResult;
        }
        var leagueYear = leagueYearRecord.ValidResult.LeagueYear;
        var currentUser = leagueYearRecord.ValidResult.CurrentUser;

        var trade = await _fantasyCriticService.GetTrade(request.TradeID);
        if (trade is null)
        {
            return BadRequest();
        }

        bool userIsCounterParty = (currentUser.Id == trade.CounterParty.User.Id);
        if (!userIsCounterParty)
        {
            return Forbid();
        }

        Result result = await _fantasyCriticService.AcceptTrade(trade);
        if (result.IsFailure)
        {
            return BadRequest(result.Error);
        }

        return Ok();
    }

    [HttpPost]
    public async Task<IActionResult> RejectTrade([FromBody] BasicTradeRequest request)
    {
        var leagueYearRecord = await GetExistingLeagueYear(request.LeagueID, request.Year, ActionProcessingModeBehavior.Allow, RequiredRelationship.ActiveInYear, RequiredYearStatus.YearNotFinishedDraftFinished);
        if (leagueYearRecord.FailedResult is not null)
        {
            return leagueYearRecord.FailedResult;
        }
        var leagueYear = leagueYearRecord.ValidResult.LeagueYear;
        var currentUser = leagueYearRecord.ValidResult.CurrentUser;

        var trade = await _fantasyCriticService.GetTrade(request.TradeID);
        if (trade is null)
        {
            return BadRequest();
        }

        bool userIsCounterParty = (currentUser.Id == trade.CounterParty.User.Id);
        if (!userIsCounterParty)
        {
            return Forbid();
        }

        Result result = await _fantasyCriticService.RejectTradeByCounterParty(trade);
        if (result.IsFailure)
        {
            return BadRequest(result.Error);
        }

        return Ok();
    }

    [HttpPost]
    public async Task<IActionResult> VoteOnTrade([FromBody] TradeVoteRequest request)
    {
        var leagueYearRecord = await GetExistingLeagueYear(request.LeagueID, request.Year, ActionProcessingModeBehavior.Allow, RequiredRelationship.ActiveInYear, RequiredYearStatus.YearNotFinishedDraftFinished);
        if (leagueYearRecord.FailedResult is not null)
        {
            return leagueYearRecord.FailedResult;
        }
        var leagueYear = leagueYearRecord.ValidResult.LeagueYear;
        var currentUser = leagueYearRecord.ValidResult.CurrentUser;

        var trade = await _fantasyCriticService.GetTrade(request.TradeID);
        if (trade is null)
        {
            return BadRequest();
        }

        var validUserIDs = leagueYear.Publishers.Select(x => x.User.Id).Except(new List<Guid>()
            {trade.Proposer.User.Id, trade.CounterParty.User.Id}).ToHashSet();
        bool userIsInLeagueButNotInTrade = validUserIDs.Contains(currentUser.Id);
        if (!userIsInLeagueButNotInTrade)
        {
            return Forbid();
        }

        Result result = await _fantasyCriticService.VoteOnTrade(trade, currentUser, request.Approved, request.Comment);
        if (result.IsFailure)
        {
            return BadRequest(result.Error);
        }

        return Ok();
    }

    [HttpPost]
    public async Task<IActionResult> DeleteTradeVote([FromBody] BasicTradeRequest request)
    {
        var leagueYearRecord = await GetExistingLeagueYear(request.LeagueID, request.Year, ActionProcessingModeBehavior.Allow, RequiredRelationship.ActiveInYear, RequiredYearStatus.YearNotFinishedDraftFinished);
        if (leagueYearRecord.FailedResult is not null)
        {
            return leagueYearRecord.FailedResult;
        }
        var leagueYear = leagueYearRecord.ValidResult.LeagueYear;
        var currentUser = leagueYearRecord.ValidResult.CurrentUser;

        var trade = await _fantasyCriticService.GetTrade(request.TradeID);
        if (trade is null)
        {
            return BadRequest();
        }

        var validUserIDs = leagueYear.Publishers.Select(x => x.User.Id).Except(new List<Guid>()
            {trade.Proposer.User.Id, trade.CounterParty.User.Id}).ToHashSet();
        bool userIsInLeagueButNotInTrade = validUserIDs.Contains(currentUser.Id);
        if (!userIsInLeagueButNotInTrade)
        {
            return Forbid();
        }

        Result result = await _fantasyCriticService.DeleteTradeVote(trade, currentUser);
        if (result.IsFailure)
        {
            return BadRequest(result.Error);
        }

        return Ok();
    }

    [AllowAnonymous]
    public async Task<IActionResult> TradeHistory(Guid leagueID, int year)
    {
        var leagueYearRecord = await GetExistingLeagueYear(leagueID, year, ActionProcessingModeBehavior.Allow, RequiredRelationship.AllowAnonymous, RequiredYearStatus.Any);
        if (leagueYearRecord.FailedResult is not null)
        {
            return leagueYearRecord.FailedResult;
        }
        var leagueYear = leagueYearRecord.ValidResult.LeagueYear;
        var league = leagueYear.League;
        var relationship = leagueYearRecord.ValidResult.Relationship;

        if (!league.PublicLeague && !relationship.HasPermissionToViewLeague)
        {
            return Forbid();
        }

        var currentDate = _clock.GetToday();
        var trades = await _fantasyCriticService.GetTradesForLeague(leagueYear);
        var inactiveTrades = trades.Where(x => !x.Status.IsActive);
        var viewModels = inactiveTrades.Select(x => new TradeViewModel(x, currentDate));
        return Ok(viewModels);
    }

    private IReadOnlyList<SingleGameNewsViewModel> GetGameNewsViewModel(LeagueYear leagueYear, bool userMode, bool recentReleases)
    {
        return GetGameNewsViewModel(leagueYear.Publishers.Select(x => new LeagueYearPublisherPair(leagueYear, x)), userMode, recentReleases);
    }

    private IReadOnlyList<SingleGameNewsViewModel> GetGameNewsViewModel(IEnumerable<LeagueYearPublisherPair> publishers, bool userMode, bool recentReleases)
    {
        var publisherGames = publishers.SelectMany(x => x.Publisher.PublisherGames).Where(x => x.MasterGame is not null);
        var currentDate = _clock.GetToday();
        var yesterday = currentDate.PlusDays(-1);
        var tomorrow = currentDate.PlusDays(1);

        IEnumerable<IGrouping<MasterGameYear, PublisherGame>> orderedByReleaseDate;

        if (recentReleases)
        {
            orderedByReleaseDate = publisherGames
                .Distinct()
                .Where(x => x.MasterGame.MasterGame.GetDefiniteMaximumReleaseDate() < tomorrow)
                .OrderByDescending(x => x.MasterGame.MasterGame.GetDefiniteMaximumReleaseDate())
                .GroupBy(x => x.MasterGame)
                .Take(10);
        }
        else
        {
            orderedByReleaseDate = publisherGames
                .Distinct()
                .Where(x => x.MasterGame.MasterGame.GetDefiniteMaximumReleaseDate() > yesterday)
                .OrderBy(x => x.MasterGame.MasterGame.GetDefiniteMaximumReleaseDate())
                .GroupBy(x => x.MasterGame)
                .Take(10);
        }

        List<SingleGameNewsViewModel> viewModels = new List<SingleGameNewsViewModel>();
        foreach (var publisherGameGroup in orderedByReleaseDate)
        {
            IReadOnlyList<LeagueYearPublisherPair> publishersThatHaveGame = publishers.Where(x => publisherGameGroup.Select(y => y.PublisherID).Contains(x.Publisher.PublisherID)).ToList();
            viewModels.Add(new SingleGameNewsViewModel(publisherGameGroup.Key, publishersThatHaveGame, userMode, currentDate));
        }

        return viewModels;
    }
}
