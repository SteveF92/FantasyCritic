using FantasyCritic.Lib.BusinessLogicFunctions;
using FantasyCritic.Lib.Domain.Combinations;
using FantasyCritic.Lib.Domain.Conferences;
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

        //*********************Get data from services/database**************************

        //My Leagues
        IReadOnlyList<LeagueWithMostRecentYearStatus> myLeagues = await _leagueMemberService.GetLeaguesForUser(currentUser);

        //My Invites
        IReadOnlyList<LeagueInvite> invitedLeagues = await _leagueMemberService.GetLeagueInvites(currentUser);

        //My Conferences
        IReadOnlyList<Conference> myConferences = await _conferenceService.GetConferencesForUser(currentUser);

        //Top Bids and Drops
        var processingDatesWithData = await _interLeagueService.GetProcessingDatesForTopBidsAndDrops();
        LocalDate? topBidsAndDropsDateToUse = null;
        IReadOnlyList<TopBidsAndDropsGame> topBidsAndDrops = new List<TopBidsAndDropsGame>();
        if (processingDatesWithData.Any())
        {
            topBidsAndDropsDateToUse = processingDatesWithData.Max();
            topBidsAndDrops = await _interLeagueService.GetTopBidsAndDrops(topBidsAndDropsDateToUse.Value);
        }

        //My Game News
        IReadOnlyList<LeagueYearPublisherPair> myPublishers = await _publisherService.GetPublishersWithLeagueYears(currentUser);

        //Public Leagues
        var supportedYears = await _interLeagueService.GetSupportedYears();
        var selectedYear = supportedYears.Where(x => x.OpenForPlay).Select(x => x.Year).Min();
        IReadOnlyList<PublicLeagueYearStats> publicLeagueYears = await _fantasyCriticService.GetPublicLeagueYears(selectedYear, 10);

        //Active Royale Quarter
        RoyaleYearQuarter activeQuarter = await _royaleService.GetActiveYearQuarter();

        //User Royale Publisher
        RoyalePublisher? userRoyalePublisher = await _royaleService.GetPublisher(activeQuarter, currentUser);

        //var homePageData = await _fantasyCriticService.GetHomePageData(currentUser);

        //*********************Build View Models**************************

        var currentDate = _clock.GetToday();

        //My Leagues
        var myLeagueViewModels = myLeagues
            .Select(league => new LeagueWithStatusViewModel(league, currentUser))
            .OrderBy(l => l.LeagueName)
            .ToList();

        //My Invites
        var myInviteViewModels = invitedLeagues.Select(x => LeagueInviteViewModel.CreateWithDisplayName(x, currentUser));

        //My Conferences
        var myConferenceViewModels = myConferences
            .Select(conference => new MinimalConferenceViewModel(conference, conference.ConferenceManager.UserID == currentUser.UserID))
            .OrderBy(x => x.ConferenceName)
            .ToList();

        //Top Bids and Drops
        TopBidsAndDropsSetViewModel? completeTopBidsAndDropsViewModel = null;
        if (topBidsAndDropsDateToUse.HasValue)
        {
            var topBidsAndDropsViewModels = topBidsAndDrops.Select(x => new TopBidsAndDropsGameViewModel(x, currentDate)).ToList();
            completeTopBidsAndDropsViewModel = new TopBidsAndDropsSetViewModel(topBidsAndDropsViewModels, topBidsAndDropsDateToUse.Value);
        }

        //My Game News
        var gameNewsUpcoming = GameNewsFunctions.GetGameNews(myPublishers, currentDate, false);
        var gameNewsRecent = GameNewsFunctions.GetGameNews(myPublishers, currentDate, true);

        var leagueYearPublisherListsUpcoming = GameNewsFunctions.GetLeagueYearPublisherLists(myPublishers, gameNewsUpcoming);
        var leagueYearPublisherListsRecent = GameNewsFunctions.GetLeagueYearPublisherLists(myPublishers, gameNewsRecent);

        var upcomingGames = DomainControllerUtilities.BuildUserGameNewsViewModel(currentDate, leagueYearPublisherListsUpcoming).ToList();
        var recentGames = DomainControllerUtilities.BuildUserGameNewsViewModel(currentDate, leagueYearPublisherListsRecent).ToList();
        var myGameNewsViewModel = new GameNewsViewModel(upcomingGames, recentGames);

        //Public Leagues
        var publicLeagueViewModels = publicLeagueYears.Select(leagueYear => new PublicLeagueYearViewModel(leagueYear)).ToList();

        //Active Royale Quarter
        var activeRoyaleQuarterViewModel = new RoyaleYearQuarterViewModel(activeQuarter);

        var vm = new
        {
            MyLeagues = myLeagueViewModels,
            MyInvites = myInviteViewModels,
            MyConferences = myConferenceViewModels,
            TopBidsAndDrops = completeTopBidsAndDropsViewModel,
            MyGameNews = myGameNewsViewModel,
            PublicLeagues = publicLeagueViewModels,
            ActiveRoyaleQuarter = activeRoyaleQuarterViewModel,
            UserRoyalePublisherID = userRoyalePublisher?.PublisherID
        };

        return Ok(vm);
    }
}
