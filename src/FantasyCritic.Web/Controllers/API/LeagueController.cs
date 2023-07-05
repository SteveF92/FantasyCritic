using System.Text;
using FantasyCritic.Lib.BusinessLogicFunctions;
using FantasyCritic.Lib.Domain.Combinations;
using FantasyCritic.Lib.Domain.Draft;
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
    private readonly TradeService _tradeService;

    public LeagueController(FantasyCriticUserManager userManager, FantasyCriticService fantasyCriticService, InterLeagueService interLeagueService,
        LeagueMemberService leagueMemberService, DraftService draftService, GameSearchingService gameSearchingService, PublisherService publisherService, IClock clock,
        IHubContext<UpdateHub> hubContext, ILogger<LeagueController> logger, GameAcquisitionService gameAcquisitionService, TradeService tradeService)
        : base(userManager, fantasyCriticService, interLeagueService, leagueMemberService)
    {
        _draftService = draftService;
        _gameSearchingService = gameSearchingService;
        _publisherService = publisherService;
        _clock = clock;
        _hubContext = hubContext;
        _logger = logger;
        _gameAcquisitionService = gameAcquisitionService;
        _tradeService = tradeService;
    }

    [AllowAnonymous]
    public async Task<IActionResult> LeagueOptions()
    {
        var supportedYears = await _interLeagueService.GetSupportedYears();
        var openYears = supportedYears.Where(x => x.OpenForCreation && !x.Finished);

        var currentUserResult = await GetCurrentUser();
        if (currentUserResult.IsSuccess)
        {
            var userIsBetaUser = await _userManager.IsInRoleAsync(currentUserResult.Value, "BetaTester");
            if (userIsBetaUser)
            {
                var betaYears = supportedYears.Where(x => x.OpenForBetaUsers);
                openYears = openYears.Concat(betaYears).Distinct();
            }
        }

        var openYearInts = openYears.Select(x => x.Year);
        LeagueOptionsViewModel viewModel = new LeagueOptionsViewModel(openYearInts, DraftSystem.GetAllPossibleValues(),
            PickupSystem.GetAllPossibleValues(), TiebreakSystem.GetAllPossibleValues(),
            ScoringSystem.GetAllPossibleValues(), TradingSystem.GetAllPossibleValues(),
            ReleaseSystem.GetAllPossibleValues());

        return Ok(viewModel);
    }

    public async Task<IActionResult> MyLeagues(int? year)
    {
        var currentUser = await GetCurrentUserOrThrow();

        IReadOnlyList<League> myLeagues = await _leagueMemberService.GetLeaguesForUser(currentUser);
        var oneShotLeagues = await _leagueMemberService.GetLeaguesWithMostRecentYearOneShot();

        List<LeagueWithStatusViewModel> viewModels = new List<LeagueWithStatusViewModel>();
        foreach (var league in myLeagues)
        {
            if (year.HasValue && !league.Years.Contains(year.Value))
            {
                continue;
            }

            bool isManager = (league.LeagueManager.Id == currentUser.Id);
            viewModels.Add(new LeagueWithStatusViewModel(league, isManager, true, true, oneShotLeagues.Contains(league.LeagueID)));
        }

        var sortedViewModels = viewModels.OrderBy(l => l.LeagueName);
        return Ok(sortedViewModels);
    }

    public async Task<IActionResult> FollowedLeagues()
    {
        var currentUser = await GetCurrentUserOrThrow();

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
        var currentUser = await GetCurrentUserOrThrow();

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

        var validResult = leagueRecord.ValidResult!;
        var currentUser = validResult.CurrentUser;
        var league = validResult.League;
        var relationship = validResult.Relationship;

        bool inviteCodeIsValid = false;
        if (inviteCode.HasValue)
        {
            var activeLinks = await _leagueMemberService.GetActiveInviteLinks(league);
            inviteCodeIsValid = activeLinks.Any(x => x.Active && x.InviteCode == inviteCode.Value);
        }

        if (!league.PublicLeague && !relationship.HasPermissionToViewLeague && !inviteCodeIsValid)
        {
            return UnauthorizedOrForbid(validResult.CurrentUser is not null);
        }

        bool userIsFollowingLeague = false;
        if (currentUser is not null)
        {
            var leagueFollowers = await _fantasyCriticService.GetLeagueFollowers(league);
            userIsFollowingLeague = leagueFollowers.Any(x => x.Id == currentUser.Id);
        }

        var leagueViewModel = new LeagueViewModel(league, relationship.LeagueManager, validResult.PlayersInLeague,
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
        var validResult = leagueYearRecord.ValidResult!;
        var leagueYear = validResult.LeagueYear;
        var league = leagueYear.League;
        var currentUser = validResult.CurrentUser;
        var relationship = validResult.Relationship;

        bool inviteCodeIsValid = false;
        if (inviteCode.HasValue)
        {
            var activeLinks = await _leagueMemberService.GetActiveInviteLinks(league);
            inviteCodeIsValid = activeLinks.Any(x => x.Active && x.InviteCode == inviteCode.Value);
        }

        if (!league.PublicLeague && !relationship.HasPermissionToViewLeague && !inviteCodeIsValid)
        {
            return UnauthorizedOrForbid(validResult.CurrentUser is not null);
        }

        SystemWideValues systemWideValues = await _interLeagueService.GetSystemWideValues();
        IReadOnlyList<ManagerMessage> managerMessages = await _fantasyCriticService.GetManagerMessages(leagueYear);

        FantasyCriticUser? previousYearWinner = await _fantasyCriticService.GetPreviousYearWinner(leagueYear);
        var counterPickedByDictionary = GameUtilities.GetCounterPickedByDictionary(leagueYear);

        IReadOnlyList<Trade> activeTrades = await _tradeService.GetActiveTradesForLeague(leagueYear);
        IReadOnlyList<SpecialAuction> activeSpecialAuctions = await _gameAcquisitionService.GetActiveSpecialAuctionsForLeague(leagueYear);
        var publicBiddingGames = await _gameAcquisitionService.GetPublicBiddingGames(leagueYear, activeSpecialAuctions);

        bool userIsFollowingLeague = false;
        Publisher? userPublisher = null;
        if (currentUser is not null)
        {
            userIsFollowingLeague = await _fantasyCriticService.UserIsFollowingLeague(currentUser, league);
            userPublisher = leagueYear.GetUserPublisher(currentUser);
        }

        var currentInstant = _clock.GetCurrentInstant();
        var currentDate = currentInstant.ToEasternDate();
        PrivatePublisherDataViewModel? privatePublisherData = null;
        if (userPublisher is not null)
        {
            var bids = await _gameAcquisitionService.GetActiveAcquisitionBids(leagueYear, userPublisher);
            var dropRequests = await _gameAcquisitionService.GetActiveDropRequests(leagueYear, userPublisher);
            var queuedGames = await _publisherService.GetQueuedGames(userPublisher);
            var masterGameYears = await _interLeagueService.GetMasterGameYears(leagueYear.Year);
            var masterGameYearDictionary = masterGameYears.ToDictionary(x => x.MasterGame.MasterGameID);
            privatePublisherData = new PrivatePublisherDataViewModel(leagueYear, userPublisher, bids, dropRequests, queuedGames, currentDate, masterGameYearDictionary);
        }

        var publishers = leagueYear.Publishers.Select(x => new LeagueYearPublisherPair(leagueYear, x)).ToList();
        var upcomingGames = BuildLeagueGameNewsViewModel(leagueYear, currentDate, GameNewsFunctions.GetGameNews(publishers, false, currentDate)).ToList();
        var recentGames = BuildLeagueGameNewsViewModel(leagueYear, currentDate, GameNewsFunctions.GetGameNews(publishers, true, currentDate)).ToList();
        var gameNewsViewModel = new GameNewsViewModel(upcomingGames, recentGames);
        var completePlayStatus = new CompletePlayStatus(leagueYear, validResult.ActiveUsers, relationship.LeagueManager);

        var leagueViewModel = new LeagueViewModel(league, relationship.LeagueManager, validResult.PlayersInLeague,
            relationship.LeagueInvite, currentUser, relationship.InLeague, userIsFollowingLeague);

        var leagueYearViewModel = new LeagueYearViewModel(leagueViewModel, leagueYear, currentInstant,
            validResult.ActiveUsers, completePlayStatus, systemWideValues,
            validResult.InvitedPlayers, relationship.InLeague, relationship.InvitedToLeague, relationship.LeagueManager,
            currentUser, managerMessages, previousYearWinner, publicBiddingGames, counterPickedByDictionary,
            activeTrades, activeSpecialAuctions, privatePublisherData, gameNewsViewModel);
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

        var validResult = leagueYearRecord.ValidResult!;
        var leagueYear = validResult.LeagueYear;
        var league = leagueYear.League;
        var relationship = validResult.Relationship;

        if (!league.PublicLeague && !relationship.HasPermissionToViewLeague)
        {
            return UnauthorizedOrForbid(validResult.CurrentUser is not null);
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
        var validResult = leagueYearRecord.ValidResult!;
        var leagueYear = validResult.LeagueYear;
        var league = leagueYear.League;
        var relationship = validResult.Relationship;

        if (!league.PublicLeague && !relationship.HasPermissionToViewLeague)
        {
            return UnauthorizedOrForbid(validResult.CurrentUser is not null);
        }

        var leagueActionSets = await _fantasyCriticService.GetLeagueActionProcessingSets(leagueYear);

        var currentDate = _clock.GetToday();
        var masterGameYears = await _interLeagueService.GetMasterGameYears(leagueYear.Year);
        var masterGameYearDictionary = masterGameYears.ToDictionary(x => x.MasterGame.MasterGameID);
        var viewModels = leagueActionSets.Where(x => x.HasActions).Select(x => new LeagueActionProcessingSetViewModel(x, currentDate, masterGameYearDictionary));
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
        var validResult = publisherRecord.ValidResult!;
        var leagueYear = validResult.LeagueYear;
        var league = leagueYear.League;
        var relationship = validResult.Relationship;
        var publisher = validResult.Publisher;

        if (!league.PublicLeague && !relationship.HasPermissionToViewLeague)
        {
            return UnauthorizedOrForbid(validResult.CurrentUser is not null);
        }

        SystemWideValues systemWideValues = await _interLeagueService.GetSystemWideValues();
        var counterPickedByDictionary = GameUtilities.GetCounterPickedByDictionary(leagueYear);

        var currentDate = _clock.GetToday();
        var publisherViewModel = new PublisherViewModel(leagueYear, publisher, currentDate, relationship.InLeague,
            relationship.InvitedToLeague, systemWideValues, counterPickedByDictionary);
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
        var validResult = leagueYearRecord.ValidResult!;
        var leagueYear = validResult.LeagueYear;
        var league = leagueYear.League;
        var relationship = validResult.Relationship;

        if (!league.PublicLeague && !relationship.HasPermissionToViewLeague)
        {
            return UnauthorizedOrForbid(validResult.CurrentUser is not null);
        }

        var leagueViewModel = new LeagueYearSettingsViewModel(leagueYear);
        return Ok(leagueViewModel);
    }

    [HttpPost]
    [Authorize("Write")]
    public async Task<IActionResult> AcceptInvite([FromBody] AcceptInviteRequest request)
    {
        var leagueRecord = await GetExistingLeague(request.LeagueID, RequiredRelationship.LoggedIn);
        if (leagueRecord.FailedResult is not null)
        {
            return leagueRecord.FailedResult;
        }
        var validResult = leagueRecord.ValidResult!;
        var currentUser = validResult.CurrentUser!;
        var league = validResult.League;

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
    [Authorize("Write")]
    public async Task<IActionResult> JoinWithInviteLink([FromBody] JoinWithInviteLinkRequest request)
    {
        var leagueRecord = await GetExistingLeague(request.LeagueID, RequiredRelationship.LoggedIn);
        if (leagueRecord.FailedResult is not null)
        {
            return leagueRecord.FailedResult;
        }
        var validResult = leagueRecord.ValidResult!;
        var currentUser = validResult.CurrentUser!;
        var league = validResult.League;

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
    [Authorize("Write")]
    public async Task<IActionResult> CreatePublisher([FromBody] CreatePublisherRequest request)
    {
        var leagueYearRecord = await GetExistingLeagueYear(request.LeagueID, request.Year, ActionProcessingModeBehavior.Allow, RequiredRelationship.ActiveInYear, RequiredYearStatus.YearNotFinishedDraftNotStarted);
        if (leagueYearRecord.FailedResult is not null)
        {
            return leagueYearRecord.FailedResult;
        }
        var validResult = leagueYearRecord.ValidResult!;
        var leagueYear = validResult.LeagueYear;
        var currentUser = validResult.CurrentUser!;

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
    [Authorize("Write")]
    public async Task<IActionResult> ChangePublisherName([FromBody] ChangePublisherNameRequest request)
    {
        var publisherRecord = await GetExistingLeagueYearAndPublisher(request.PublisherID, ActionProcessingModeBehavior.Allow, RequiredRelationship.BePublisher, RequiredYearStatus.Any);
        if (publisherRecord.FailedResult is not null)
        {
            return publisherRecord.FailedResult;
        }
        var validResult = publisherRecord.ValidResult!;
        var publisher = validResult.Publisher;

        if (string.IsNullOrWhiteSpace(request.PublisherName))
        {
            return BadRequest("You cannot have a blank name.");
        }

        await _publisherService.ChangePublisherName(publisher, request.PublisherName);
        return Ok();
    }

    [HttpPost]
    [Authorize("PlusUser")]
    [Authorize("Write")]
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
    [Authorize("PlusUser")]
    [Authorize("Write")]
    public async Task<IActionResult> ChangePublisherSlogan([FromBody] ChangePublisherSloganRequest request)
    {
        var publisherRecord = await GetExistingLeagueYearAndPublisher(request.PublisherID, ActionProcessingModeBehavior.Allow, RequiredRelationship.BePublisher, RequiredYearStatus.Any);
        if (publisherRecord.FailedResult is not null)
        {
            return publisherRecord.FailedResult;
        }
        var publisher = publisherRecord.ValidResult!.Publisher;

        await _publisherService.ChangePublisherSlogan(publisher, request.PublisherSlogan);
        return Ok();
    }

    [HttpPost]
    [Authorize("Write")]
    public async Task<IActionResult> SetAutoDraft([FromBody] SetAutoDraftRequest request)
    {
        var publisherRecord = await GetExistingLeagueYearAndPublisher(request.PublisherID, ActionProcessingModeBehavior.Allow, RequiredRelationship.BePublisher, RequiredYearStatus.YearNotFinishedDraftNotFinished);
        if (publisherRecord.FailedResult is not null)
        {
            return publisherRecord.FailedResult;
        }
        var validResult = publisherRecord.ValidResult!;
        var leagueYear = validResult.LeagueYear;
        var publisher = validResult.Publisher;

        await _publisherService.SetAutoDraft(publisher, request.AutoDraft);

        var draftComplete = await _draftService.RunAutoDraftAndCheckIfComplete(leagueYear);
        if (draftComplete)
        {
            await _hubContext.Clients.Group(leagueYear.GetGroupName).SendAsync("DraftFinished");
        }

        return Ok();
    }

    [HttpPost]
    [Authorize("Write")]
    public async Task<IActionResult> DeclineInvite([FromBody] DeleteInviteRequest request)
    {
        var currentUser = await GetCurrentUserOrThrow();

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
    [Authorize("Write")]
    public async Task<IActionResult> MakePickupBid([FromBody] PickupBidRequest request)
    {
        var publisherRecord = await GetExistingLeagueYearAndPublisher(request.PublisherID, ActionProcessingModeBehavior.Ban, RequiredRelationship.BePublisher, RequiredYearStatus.YearNotFinishedDraftFinished);
        if (publisherRecord.FailedResult is not null)
        {
            return publisherRecord.FailedResult;
        }
        var validResult = publisherRecord.ValidResult!;
        var leagueYear = validResult.LeagueYear;
        var publisher = validResult.Publisher;

        var masterGame = await _interLeagueService.GetMasterGame(request.MasterGameID);
        if (masterGame is null)
        {
            return BadRequest("That master game does not exist.");
        }

        var activeSpecialAuctions = await _gameAcquisitionService.GetActiveSpecialAuctionsForLeague(leagueYear);
        var publicBiddingGames = await _gameAcquisitionService.GetPublicBiddingGames(leagueYear, activeSpecialAuctions);
        bool publicBidIsValid = _gameAcquisitionService.PublicBidIsValid(leagueYear, masterGame, request.CounterPick, publicBiddingGames?.MasterGames, activeSpecialAuctions);
        if (!publicBidIsValid)
        {
            return BadRequest("During the public bidding window, you can only bid on a game that is already being bid on by at least one player.");
        }

        if (leagueYear.Options.OneShotMode)
        {
            return BadRequest("This league is in 'one shot mode', which doesn't support bids.");
        }

        PublisherGame? conditionalDropPublisherGame = null;
        if (request.ConditionalDropPublisherGameID.HasValue)
        {
            conditionalDropPublisherGame = publisher.GetPublisherGameByPublisherGameID(request.ConditionalDropPublisherGameID.Value);
        }

        ClaimResult bidResult = await _gameAcquisitionService.MakePickupBid(leagueYear, publisher, masterGame, conditionalDropPublisherGame, request.CounterPick, request.BidAmount, request.AllowIneligibleSlot);
        var viewModel = new PickupBidResultViewModel(bidResult);

        return Ok(viewModel);
    }

    [HttpPost]
    [Authorize("Write")]
    public async Task<IActionResult> EditPickupBid([FromBody] PickupBidEditRequest request)
    {
        var publisherRecord = await GetExistingLeagueYearAndPublisher(request.PublisherID, ActionProcessingModeBehavior.Ban, RequiredRelationship.BePublisher, RequiredYearStatus.YearNotFinishedDraftFinished);
        if (publisherRecord.FailedResult is not null)
        {
            return publisherRecord.FailedResult;
        }
        var validResult = publisherRecord.ValidResult!;
        var publisher = validResult.Publisher;

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

        ClaimResult bidResult = await _gameAcquisitionService.EditPickupBid(maybeBid, conditionalDropPublisherGame, request.BidAmount, request.AllowIneligibleSlot);
        var viewModel = new PickupBidResultViewModel(bidResult);

        return Ok(viewModel);
    }

    [HttpPost]
    [Authorize("Write")]
    public async Task<IActionResult> DeletePickupBid([FromBody] PickupBidDeleteRequest request)
    {
        var publisherRecord = await GetExistingLeagueYearAndPublisher(request.PublisherID, ActionProcessingModeBehavior.Ban, RequiredRelationship.BePublisher, RequiredYearStatus.YearNotFinishedDraftFinished);
        if (publisherRecord.FailedResult is not null)
        {
            return publisherRecord.FailedResult;
        }
        var validResult = publisherRecord.ValidResult!;
        var publisher = validResult.Publisher;

        var bid = await _gameAcquisitionService.GetPickupBid(request.BidID);
        if (bid is null)
        {
            return BadRequest("That bid does not exist.");
        }

        if (bid.Publisher.PublisherID != publisher.PublisherID)
        {
            return Forbid();
        }

        Result result = await _gameAcquisitionService.RemovePickupBid(bid);
        if (result.IsFailure)
        {
            return BadRequest(result.Error);
        }

        return Ok();
    }

    [HttpPost]
    [Authorize("Write")]
    public async Task<IActionResult> SetBidPriorities([FromBody] BidPriorityOrderRequest request)
    {
        var publisherRecord = await GetExistingLeagueYearAndPublisher(request.PublisherID, ActionProcessingModeBehavior.Ban, RequiredRelationship.BePublisher, RequiredYearStatus.YearNotFinishedDraftFinished);
        if (publisherRecord.FailedResult is not null)
        {
            return publisherRecord.FailedResult;
        }
        var validResult = publisherRecord.ValidResult!;
        var leagueYear = validResult.LeagueYear;
        var publisher = validResult.Publisher;

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
    [Authorize("Write")]
    public async Task<IActionResult> DraftGame([FromBody] DraftGameRequest request)
    {
        var publisherRecord = await GetExistingLeagueYearAndPublisher(request.PublisherID, ActionProcessingModeBehavior.Allow, RequiredRelationship.BePublisher, RequiredYearStatus.ActiveDraft);
        if (publisherRecord.FailedResult is not null)
        {
            return publisherRecord.FailedResult;
        }
        var validResult = publisherRecord.ValidResult!;
        var leagueYear = validResult.LeagueYear;
        var publisher = validResult.Publisher;

        var draftStatus = DraftFunctions.GetDraftStatus(leagueYear);
        if (draftStatus is null)
        {
            return BadRequest("Draft is not active.");
        }

        if (!draftStatus.NextDraftPublisher.Equals(publisher))
        {
            return BadRequest("It is not your turn to draft.");
        }

        MasterGame? masterGame = null;
        if (request.MasterGameID.HasValue)
        {
            masterGame = await _interLeagueService.GetMasterGame(request.MasterGameID.Value);
        }

        if (draftStatus.DraftPhase.Equals(DraftPhase.StandardGames))
        {
            if (request.CounterPick)
            {
                return BadRequest("Not drafting counterPicks now.");
            }
        }

        if (draftStatus.DraftPhase.Equals(DraftPhase.CounterPicks))
        {
            if (!request.CounterPick)
            {
                return BadRequest("Not drafting standard games now.");
            }
        }

        ClaimGameDomainRequest domainRequest = new ClaimGameDomainRequest(leagueYear, publisher, request.GameName, request.CounterPick, false, false, false, masterGame,
            draftStatus.DraftPosition, draftStatus.OverallDraftPosition);

        var draftResult = await _draftService.DraftGame(domainRequest, false, request.AllowIneligibleSlot);
        var viewModel = new PlayerClaimResultViewModel(draftResult.Result);
        await _hubContext.Clients.Group(leagueYear.GetGroupName).SendAsync("RefreshLeagueYear");

        if (draftResult.DraftComplete)
        {
            await _hubContext.Clients.Group(leagueYear.GetGroupName).SendAsync("DraftFinished");
        }

        return Ok(viewModel);
    }

    [HttpPost]
    [Authorize("Write")]
    public async Task<IActionResult> FollowLeague([FromBody] FollowLeagueRequest request)
    {
        var leagueRecord = await GetExistingLeague(request.LeagueID, RequiredRelationship.LoggedIn);
        if (leagueRecord.FailedResult is not null)
        {
            return leagueRecord.FailedResult;
        }
        var validResult = leagueRecord.ValidResult!;
        var currentUser = validResult.CurrentUser!;
        var league = validResult.League;

        Result result = await _fantasyCriticService.FollowLeague(league, currentUser);
        if (result.IsFailure)
        {
            return BadRequest(result.Error);
        }

        return Ok();
    }

    [HttpPost]
    [Authorize("Write")]
    public async Task<IActionResult> UnfollowLeague([FromBody] FollowLeagueRequest request)
    {
        var leagueRecord = await GetExistingLeague(request.LeagueID, RequiredRelationship.LoggedIn);
        if (leagueRecord.FailedResult is not null)
        {
            return leagueRecord.FailedResult;
        }
        var validResult = leagueRecord.ValidResult!;
        var currentUser = validResult.CurrentUser!;
        var league = validResult.League;

        Result result = await _fantasyCriticService.UnfollowLeague(league, currentUser);
        if (result.IsFailure)
        {
            return BadRequest(result.Error);
        }

        return Ok();
    }

    public async Task<ActionResult<GameNewsViewModel>> MyGameNews()
    {
        var currentUser = await GetCurrentUserOrThrow();
        var currentDate = _clock.GetToday();

        var myPublishers = await _publisherService.GetPublishersWithLeagueYears(currentUser);

        var gameNewsUpcoming = GameNewsFunctions.GetGameNewsForPublishers(myPublishers, currentDate, false);
        var gameNewsRecent = GameNewsFunctions.GetGameNewsForPublishers(myPublishers, currentDate, true);

        var leagueYearPublisherListsUpcoming = GameNewsFunctions.GetLeagueYearPublisherLists(myPublishers, gameNewsUpcoming);
        var leagueYearPublisherListsRecent = GameNewsFunctions.GetLeagueYearPublisherLists(myPublishers, gameNewsRecent);

        var upcomingGames = BuildUserGameNewsViewModel(currentDate, leagueYearPublisherListsUpcoming).ToList();
        var recentGames = BuildUserGameNewsViewModel(currentDate, leagueYearPublisherListsRecent).ToList();
        return new GameNewsViewModel(upcomingGames, recentGames);
    }

    public async Task<IActionResult> PossibleMasterGames(string gameName, int year, Guid leagueID)
    {
        var leagueYearRecord = await GetExistingLeagueYear(leagueID, year, ActionProcessingModeBehavior.Allow, RequiredRelationship.ActiveInYear, RequiredYearStatus.Any);
        if (leagueYearRecord.FailedResult is not null)
        {
            return leagueYearRecord.FailedResult;
        }
        var validResult = leagueYearRecord.ValidResult!;
        var leagueYear = validResult.LeagueYear;
        var currentUser = validResult.CurrentUser!;

        var userPublisher = leagueYear.GetUserPublisher(currentUser);
        if (userPublisher is null)
        {
            return BadRequest();
        }

        var currentDate = _clock.GetToday();
        var matchingGames = await _gameSearchingService.SearchGames(gameName, leagueYear, userPublisher);
        var viewModels = matchingGames.Select(x => new PossibleMasterGameYearViewModel(x, currentDate)).Take(50).ToList();

        return Ok(viewModels);
    }

    public async Task<IActionResult> TopAvailableGames(int year, Guid leagueID, string? slotInfo)
    {
        var leagueYearRecord = await GetExistingLeagueYear(leagueID, year, ActionProcessingModeBehavior.Allow, RequiredRelationship.ActiveInYear, RequiredYearStatus.Any);
        if (leagueYearRecord.FailedResult is not null)
        {
            return leagueYearRecord.FailedResult;
        }
        var validResult = leagueYearRecord.ValidResult!;
        var leagueYear = validResult.LeagueYear;
        var currentUser = validResult.CurrentUser!;

        var userPublisher = leagueYear.GetUserPublisher(currentUser);
        if (userPublisher is null)
        {
            return BadRequest();
        }

        IReadOnlyList<PossibleMasterGameYear> topAvailableGames;
        if (slotInfo is not null)
        {
            string slotInfoJSON = Encoding.UTF8.GetString(Convert.FromBase64String(slotInfo));
            PublisherSingleSlotRequirementsViewModel? slotInfoObject = JsonConvert.DeserializeObject<PublisherSingleSlotRequirementsViewModel>(slotInfoJSON);
            if (slotInfoObject is null)
            {
                return BadRequest();
            }
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
        var validResult = publisherRecord.ValidResult!;
        var leagueYear = validResult.LeagueYear;
        var publisher = validResult.Publisher;

        var availableCounterPicks = _draftService.GetAvailableCounterPicks(leagueYear, publisher);
        var currentDate = _clock.GetToday();
        var viewModels = availableCounterPicks
            .Select(x => new PublisherGameViewModel(x, currentDate, null, false))
            .OrderBy(x => x.GameName).ToList();

        return Ok(viewModels);
    }

    [HttpPost]
    [Authorize("Write")]
    public async Task<IActionResult> MakeDropRequest([FromBody] DropGameRequestRequest request)
    {
        var publisherRecord = await GetExistingLeagueYearAndPublisherGame(request.PublisherID, request.PublisherGameID, ActionProcessingModeBehavior.Ban, RequiredRelationship.BePublisher, RequiredYearStatus.YearNotFinishedDraftFinished);
        if (publisherRecord.FailedResult is not null)
        {
            return publisherRecord.FailedResult;
        }
        var validResult = publisherRecord.ValidResult!;
        var leagueYear = validResult.LeagueYear;
        var publisher = validResult.Publisher;
        var publisherGame = validResult.PublisherGame;

        if (leagueYear.Options.OneShotMode)
        {
            return BadRequest("This league is in 'one shot mode', which doesn't support drops.");
        }

        DropResult dropResult = await _gameAcquisitionService.MakeDropRequest(leagueYear, publisher, publisherGame, false);
        var viewModel = new DropGameResultViewModel(dropResult);

        return Ok(viewModel);
    }

    [HttpPost]
    [Authorize("Write")]
    public async Task<IActionResult> UseSuperDrop([FromBody] DropGameRequestRequest request)
    {
        var publisherRecord = await GetExistingLeagueYearAndPublisherGame(request.PublisherID, request.PublisherGameID, ActionProcessingModeBehavior.Ban, RequiredRelationship.BePublisher, RequiredYearStatus.YearNotFinishedDraftFinished);
        if (publisherRecord.FailedResult is not null)
        {
            return publisherRecord.FailedResult;
        }
        var validResult = publisherRecord.ValidResult!;
        var leagueYear = validResult.LeagueYear;
        var publisher = validResult.Publisher;
        var publisherGame = validResult.PublisherGame;

        DropResult dropResult = await _gameAcquisitionService.UseSuperDrop(leagueYear, publisher, publisherGame);
        var viewModel = new DropGameResultViewModel(dropResult);

        return Ok(viewModel);
    }

    [HttpPost]
    [Authorize("Write")]
    public async Task<IActionResult> DeleteDropRequest([FromBody] DropGameRequestDeleteRequest request)
    {
        var publisherRecord = await GetExistingLeagueYearAndPublisher(request.PublisherID, ActionProcessingModeBehavior.Ban, RequiredRelationship.BePublisher, RequiredYearStatus.YearNotFinishedDraftFinished);
        if (publisherRecord.FailedResult is not null)
        {
            return publisherRecord.FailedResult;
        }
        var validResult = publisherRecord.ValidResult!;
        var publisher = validResult.Publisher;

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
        var validResult = publisherRecord.ValidResult!;
        var leagueYear = validResult.LeagueYear;
        var publisher = validResult.Publisher;

        var queuedGames = await _publisherService.GetQueuedGames(publisher);

        var currentDate = _clock.GetToday();
        var queuedPossibleGames = await _gameSearchingService.GetQueuedPossibleGames(leagueYear, publisher, queuedGames);
        var viewModels = queuedPossibleGames.Select(x => new PossibleMasterGameYearViewModel(x, currentDate)).Take(100).ToList();

        return Ok(viewModels);
    }

    [HttpPost]
    [Authorize("Write")]
    public async Task<IActionResult> AddGameToQueue([FromBody] AddGameToQueueRequest request)
    {
        var publisherRecord = await GetExistingLeagueYearAndPublisher(request.PublisherID, ActionProcessingModeBehavior.Allow, RequiredRelationship.BePublisher, RequiredYearStatus.AnyYearNotFinished);
        if (publisherRecord.FailedResult is not null)
        {
            return publisherRecord.FailedResult;
        }
        var validResult = publisherRecord.ValidResult!;
        var leagueYear = validResult.LeagueYear;
        var publisher = validResult.Publisher;

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
    [Authorize("Write")]
    public async Task<IActionResult> DeleteQueuedGame([FromBody] QueuedGameDeleteRequest request)
    {
        var publisherRecord = await GetExistingLeagueYearAndPublisher(request.PublisherID, ActionProcessingModeBehavior.Allow, RequiredRelationship.BePublisher, RequiredYearStatus.AnyYearNotFinished);
        if (publisherRecord.FailedResult is not null)
        {
            return publisherRecord.FailedResult;
        }
        var validResult = publisherRecord.ValidResult!;
        var publisher = validResult.Publisher;

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
    [Authorize("Write")]
    public async Task<IActionResult> SetQueueRankings([FromBody] QueueRankingRequest request)
    {
        var publisherRecord = await GetExistingLeagueYearAndPublisher(request.PublisherID, ActionProcessingModeBehavior.Allow, RequiredRelationship.BePublisher, RequiredYearStatus.AnyYearNotFinished);
        if (publisherRecord.FailedResult is not null)
        {
            return publisherRecord.FailedResult;
        }
        var validResult = publisherRecord.ValidResult!;
        var publisher = validResult.Publisher;

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
    [Authorize("Write")]
    public async Task<IActionResult> ReorderPublisherGames([FromBody] ReorderPublisherGamesRequest request)
    {
        var publisherRecord = await GetExistingLeagueYearAndPublisher(request.PublisherID, ActionProcessingModeBehavior.Ban, RequiredRelationship.BePublisher, RequiredYearStatus.AnyYearNotFinished);
        if (publisherRecord.FailedResult is not null)
        {
            return publisherRecord.FailedResult;
        }
        var validResult = publisherRecord.ValidResult!;
        var leagueYear = validResult.LeagueYear;
        var publisher = validResult.Publisher;

        var publisherGameIDsBeingEdited = request.SlotStates.Where(x => x.Value.HasValue).Select(x => x.Value!.Value);
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
    [Authorize("Write")]
    public async Task<IActionResult> SetArchiveStatus([FromBody] SetArchiveStatusRequest request)
    {
        var leagueRecord = await GetExistingLeague(request.LeagueID, RequiredRelationship.InLeague);
        if (leagueRecord.FailedResult is not null)
        {
            return leagueRecord.FailedResult;
        }
        var validResult = leagueRecord.ValidResult!;
        var currentUser = validResult.CurrentUser!;
        var league = validResult.League;

        await _leagueMemberService.SetArchiveStatusForUser(league, request.Archive, currentUser);

        return Ok();
    }

    [HttpPost]
    [Authorize("Write")]
    public async Task<IActionResult> DismissManagerMessage([FromBody] DismissManagerMessageRequest request)
    {
        var currentUser = await GetCurrentUserOrThrow();

        Result result = await _fantasyCriticService.DismissManagerMessage(request.MessageID, currentUser.Id);
        if (result.IsFailure)
        {
            return BadRequest();
        }

        return Ok();
    }

    [HttpPost]
    [Authorize("Write")]
    public async Task<IActionResult> ProposeTrade([FromBody] ProposeTradeRequest request)
    {
        var publisherRecord = await GetExistingLeagueYearAndPublisher(request.ProposerPublisherID, ActionProcessingModeBehavior.Allow, RequiredRelationship.BePublisher, RequiredYearStatus.YearNotFinishedDraftFinished);
        if (publisherRecord.FailedResult is not null)
        {
            return publisherRecord.FailedResult;
        }
        var validResult = publisherRecord.ValidResult!;
        var leagueYear = validResult.LeagueYear;
        var publisher = validResult.Publisher;

        Result result = await _tradeService.ProposeTrade(leagueYear, publisher, request.CounterPartyPublisherID, request.ProposerPublisherGameIDs,
            request.CounterPartyPublisherGameIDs, request.ProposerBudgetSendAmount, request.CounterPartyBudgetSendAmount, request.Message);
        if (result.IsFailure)
        {
            return BadRequest(result.Error);
        }

        return Ok();
    }

    [HttpPost]
    [Authorize("Write")]
    public async Task<IActionResult> RescindTrade([FromBody] BasicTradeRequest request)
    {
        var leagueYearRecord = await GetExistingLeagueYear(request.LeagueID, request.Year, ActionProcessingModeBehavior.Allow, RequiredRelationship.ActiveInYear, RequiredYearStatus.YearNotFinishedDraftFinished);
        if (leagueYearRecord.FailedResult is not null)
        {
            return leagueYearRecord.FailedResult;
        }
        var validResult = leagueYearRecord.ValidResult!;
        var currentUser = validResult.CurrentUser!;

        var trade = await _tradeService.GetTrade(request.TradeID);
        if (trade is null)
        {
            return BadRequest();
        }

        bool userIsProposer = (currentUser.Id == trade.Proposer.User.Id);
        if (!userIsProposer)
        {
            return Forbid();
        }

        Result result = await _tradeService.RescindTrade(trade);
        if (result.IsFailure)
        {
            return BadRequest(result.Error);
        }

        return Ok();
    }

    [HttpPost]
    [Authorize("Write")]
    public async Task<IActionResult> AcceptTrade([FromBody] BasicTradeRequest request)
    {
        var leagueYearRecord = await GetExistingLeagueYear(request.LeagueID, request.Year, ActionProcessingModeBehavior.Allow, RequiredRelationship.ActiveInYear, RequiredYearStatus.YearNotFinishedDraftFinished);
        if (leagueYearRecord.FailedResult is not null)
        {
            return leagueYearRecord.FailedResult;
        }
        var validResult = leagueYearRecord.ValidResult!;
        var currentUser = validResult.CurrentUser!;

        var trade = await _tradeService.GetTrade(request.TradeID);
        if (trade is null)
        {
            return BadRequest();
        }

        bool userIsCounterParty = (currentUser.Id == trade.CounterParty.User.Id);
        if (!userIsCounterParty)
        {
            return Forbid();
        }

        Result result = await _tradeService.AcceptTrade(trade);
        if (result.IsFailure)
        {
            return BadRequest(result.Error);
        }

        return Ok();
    }

    [HttpPost]
    [Authorize("Write")]
    public async Task<IActionResult> RejectTrade([FromBody] BasicTradeRequest request)
    {
        var leagueYearRecord = await GetExistingLeagueYear(request.LeagueID, request.Year, ActionProcessingModeBehavior.Allow, RequiredRelationship.ActiveInYear, RequiredYearStatus.YearNotFinishedDraftFinished);
        if (leagueYearRecord.FailedResult is not null)
        {
            return leagueYearRecord.FailedResult;
        }
        var validResult = leagueYearRecord.ValidResult!;
        var currentUser = validResult.CurrentUser!;

        var trade = await _tradeService.GetTrade(request.TradeID);
        if (trade is null)
        {
            return BadRequest();
        }

        bool userIsCounterParty = (currentUser.Id == trade.CounterParty.User.Id);
        if (!userIsCounterParty)
        {
            return Forbid();
        }

        Result result = await _tradeService.RejectTradeByCounterParty(trade);
        if (result.IsFailure)
        {
            return BadRequest(result.Error);
        }

        return Ok();
    }

    [HttpPost]
    [Authorize("Write")]
    public async Task<IActionResult> VoteOnTrade([FromBody] TradeVoteRequest request)
    {
        var leagueYearRecord = await GetExistingLeagueYear(request.LeagueID, request.Year, ActionProcessingModeBehavior.Allow, RequiredRelationship.ActiveInYear, RequiredYearStatus.YearNotFinishedDraftFinished);
        if (leagueYearRecord.FailedResult is not null)
        {
            return leagueYearRecord.FailedResult;
        }
        var validResult = leagueYearRecord.ValidResult!;
        var leagueYear = validResult.LeagueYear;
        var currentUser = validResult.CurrentUser!;

        var trade = await _tradeService.GetTrade(request.TradeID);
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

        Result result = await _tradeService.VoteOnTrade(trade, currentUser, request.Approved, request.Comment);
        if (result.IsFailure)
        {
            return BadRequest(result.Error);
        }

        return Ok();
    }

    [HttpPost]
    [Authorize("Write")]
    public async Task<IActionResult> DeleteTradeVote([FromBody] BasicTradeRequest request)
    {
        var leagueYearRecord = await GetExistingLeagueYear(request.LeagueID, request.Year, ActionProcessingModeBehavior.Allow, RequiredRelationship.ActiveInYear, RequiredYearStatus.YearNotFinishedDraftFinished);
        if (leagueYearRecord.FailedResult is not null)
        {
            return leagueYearRecord.FailedResult;
        }
        var validResult = leagueYearRecord.ValidResult!;
        var leagueYear = validResult.LeagueYear;
        var currentUser = validResult.CurrentUser!;

        var trade = await _tradeService.GetTrade(request.TradeID);
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

        Result result = await _tradeService.DeleteTradeVote(trade, currentUser);
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
        var validResult = leagueYearRecord.ValidResult!;
        var leagueYear = validResult.LeagueYear;
        var league = leagueYear.League;
        var relationship = validResult.Relationship;

        if (!league.PublicLeague && !relationship.HasPermissionToViewLeague)
        {
            return UnauthorizedOrForbid(validResult.CurrentUser is not null);
        }

        var currentDate = _clock.GetToday();
        var trades = await _tradeService.GetTradesForLeague(leagueYear);
        var inactiveTrades = trades.Where(x => !x.Status.IsActive);
        var viewModels = inactiveTrades.Select(x => new TradeViewModel(x, currentDate));
        return Ok(viewModels);
    }

    private static IReadOnlyList<SingleGameNewsViewModel> BuildLeagueGameNewsViewModel(LeagueYear leagueYear, LocalDate currentDate, IReadOnlyList<IGrouping<MasterGameYear, PublisherGame>> gameNews)
    {
        var publishers = leagueYear.Publishers.Select(x => new LeagueYearPublisherPair(leagueYear, x)).ToList();
        var publisherLists = GameNewsFunctions.GetLeagueYearPublisherLists(publishers, gameNews);
        return publisherLists.Select(l => new SingleGameNewsViewModel(l.Key, l.Value, false, currentDate)).ToList();
    }

    private static IReadOnlyList<SingleGameNewsViewModel> BuildUserGameNewsViewModel(LocalDate currentDate, IReadOnlyDictionary<MasterGameYear, List<LeagueYearPublisherPair>> leagueYearPublisherLists)
    {
        return leagueYearPublisherLists.Select(l => new SingleGameNewsViewModel(l.Key, l.Value, true, currentDate)).ToList();
    }
}
