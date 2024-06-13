using FantasyCritic.Lib.BusinessLogicFunctions;
using FantasyCritic.Lib.Extensions;
using FantasyCritic.Lib.Identity;
using FantasyCritic.Lib.Royale;
using FantasyCritic.Lib.Services;
using FantasyCritic.SharedSerialization.API;
using FantasyCritic.Web.Models.Responses;
using FantasyCritic.Web.Models.Responses.Conferences;
using FantasyCritic.Web.Models.Responses.Royale;
using FantasyCritic.Web.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FantasyCritic.Web.Controllers.API;

[Authorize]
[Route("api/[controller]/[action]")]
public class CombinedDataController : FantasyCriticController
{
    private readonly FantasyCriticService _fantasyCriticService;
    private readonly InterLeagueService _interLeagueService;
    private readonly PublisherService _publisherService;
    private readonly LeagueMemberService _leagueMemberService;
    private readonly ConferenceService _conferenceService;
    private readonly RoyaleService _royaleService;
    private readonly IClock _clock;

    public CombinedDataController(FantasyCriticUserManager userManager, FantasyCriticService fantasyCriticService,
        InterLeagueService interLeagueService, PublisherService publisherService, LeagueMemberService leagueMemberService,
        ConferenceService conferenceService, RoyaleService royaleService, IClock clock)
        : base(userManager)
    {
        _fantasyCriticService = fantasyCriticService;
        _interLeagueService = interLeagueService;
        _publisherService = publisherService;
        _leagueMemberService = leagueMemberService;
        _conferenceService = conferenceService;
        _royaleService = royaleService;
        _clock = clock;
    }

    [AllowAnonymous]
    public async Task<IActionResult> BasicData()
    {
        var basicData = await _interLeagueService.GetBasicData();

        var bidTimes = DomainControllerUtilities.BuildBidTimesViewModel(_clock, basicData.SystemWideSettings);
        var masterGameTags = basicData.MasterGameTags.Select(x => new MasterGameTagViewModel(x)).ToList();
        var leagueOptions = BuildLeagueOptionsViewModel(basicData.SupportedYears);
        var supportedYears = basicData.SupportedYears.Select(x => new SupportedYearViewModel(x)).ToList();

        var vm = new
        {
            BidTimes = bidTimes,
            MasterGameTags = masterGameTags,
            LeagueOptions = leagueOptions,
            SupportedYears = supportedYears
        };

        return Ok(vm);
    }

    public async Task<IActionResult> HomePageData()
    {
        var currentUser = await GetCurrentUserOrThrow();
        var currentDate = _clock.GetToday();

        //My Leagues
        var myLeagues = await _leagueMemberService.GetLeaguesForUser(currentUser);
        var myLeagueViewModels = myLeagues
            .Select(league => new LeagueWithStatusViewModel(league, currentUser))
            .OrderBy(l => l.LeagueName)
            .ToList();

        //My Invites
        var invitedLeagues = await _leagueMemberService.GetLeagueInvites(currentUser);
        var myInviteViewModels = invitedLeagues.Select(x => LeagueInviteViewModel.CreateWithDisplayName(x, currentUser));

        //My Conferences
        var conferences = await _conferenceService.GetConferencesForUser(currentUser);
        var myConferenceViewModels = conferences
            .Select(conference => new MinimalConferenceViewModel(conference, conference.ConferenceManager.UserID == currentUser.UserID))
            .OrderBy(x => x.ConferenceName)
            .ToList();

        //Top Bids and Drops
        var processingDatesWithData = await _interLeagueService.GetProcessingDatesForTopBidsAndDrops();
        TopBidsAndDropsSetViewModel? completeTopBidsAndDropsViewModel = null;
        if (processingDatesWithData.Any())
        {
            var dateToUse = processingDatesWithData.Max();
            var topBidsAndDrops = await _interLeagueService.GetTopBidsAndDrops(dateToUse);
            var topBidsAndDropsViewModels = topBidsAndDrops.Select(x => new TopBidsAndDropsGameViewModel(x, currentDate)).ToList();
            completeTopBidsAndDropsViewModel = new TopBidsAndDropsSetViewModel(topBidsAndDropsViewModels, dateToUse);
        }

        //My Game News
        var myPublishers = await _publisherService.GetPublishersWithLeagueYears(currentUser);
        var gameNewsUpcoming = GameNewsFunctions.GetGameNewsForPublishers(myPublishers, currentDate, false);
        var gameNewsRecent = GameNewsFunctions.GetGameNewsForPublishers(myPublishers, currentDate, true);

        var leagueYearPublisherListsUpcoming = GameNewsFunctions.GetLeagueYearPublisherLists(myPublishers, gameNewsUpcoming);
        var leagueYearPublisherListsRecent = GameNewsFunctions.GetLeagueYearPublisherLists(myPublishers, gameNewsRecent);

        var upcomingGames = DomainControllerUtilities.BuildUserGameNewsViewModel(currentDate, leagueYearPublisherListsUpcoming).ToList();
        var recentGames = DomainControllerUtilities.BuildUserGameNewsViewModel(currentDate, leagueYearPublisherListsRecent).ToList();
        var myGameNewsViewModel = new GameNewsViewModel(upcomingGames, recentGames);

        //Public Leagues
        var supportedYears = await _interLeagueService.GetSupportedYears();
        var selectedYear = supportedYears.Where(x => x.OpenForPlay).Select(x => x.Year).Min();
        var publicLeagueYears = await _fantasyCriticService.GetPublicLeagueYears(selectedYear);
        var publicLeagueViewModels = publicLeagueYears.Select(leagueYear => new PublicLeagueYearViewModel(leagueYear)).Take(10).ToList();

        //Active Royale Quarter
        var activeQuarter = await _royaleService.GetActiveYearQuarter();
        var activeRoyaleQuarterViewModel = new RoyaleYearQuarterViewModel(activeQuarter);

        //User Royale Publisher
        RoyalePublisherViewModel? royalePublisherViewModel = null;
        RoyalePublisher? publisher = await _royaleService.GetPublisher(activeQuarter, currentUser);
        if (publisher is not null)
        {
            IReadOnlyList<RoyaleYearQuarter> quartersWon = await _royaleService.GetQuartersWonByUser(publisher.User);
            var masterGameTags = await _interLeagueService.GetMasterGameTags();
            royalePublisherViewModel = new RoyalePublisherViewModel(publisher, currentDate, null, quartersWon, masterGameTags, true);
        }

        var vm = new
        {
            MyLeagues = myLeagueViewModels,
            MyInvites = myInviteViewModels,
            MyConferences = myConferenceViewModels,
            TopBidsAndDrops = completeTopBidsAndDropsViewModel,
            MyGameNews = myGameNewsViewModel,
            PublicLeagues = publicLeagueViewModels,
            ActiveRoyaleQuarter = activeRoyaleQuarterViewModel,
            UserRoyalePublisher = royalePublisherViewModel
        };

        return Ok(vm);
    }
}
