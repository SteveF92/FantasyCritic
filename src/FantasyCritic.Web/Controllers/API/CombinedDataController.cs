using FantasyCritic.Lib.Extensions;
using FantasyCritic.Lib.Identity;
using FantasyCritic.Lib.Services;
using FantasyCritic.Lib.SharedSerialization.API;
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
    private readonly IClock _clock;

    public CombinedDataController(FantasyCriticUserManager userManager, FantasyCriticService fantasyCriticService,
        InterLeagueService interLeagueService, IClock clock)
        : base(userManager)
    {
        _fantasyCriticService = fantasyCriticService;
        _interLeagueService = interLeagueService;
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
        var homePageData = await _fantasyCriticService.GetHomePageData(currentUser);

        var currentDate = _clock.GetToday();

        //My Leagues
        var myLeagueViewModels = homePageData.MyLeagues
            .Select(league => new LeagueWithStatusViewModel(league, currentUser))
            .OrderBy(l => l.LeagueName)
            .ToList();

        //My Invites
        var myInviteViewModels = homePageData.InvitedLeagues.Select(x => new CompleteLeagueInviteViewModel(x));

        //My Conferences
        var myConferenceViewModels = homePageData.MyConferences
            .Select(conference => new MinimalConferenceViewModel(conference, conference.ConferenceManager.UserID == currentUser.UserID))
            .OrderBy(x => x.ConferenceName)
            .ToList();

        //Top Bids and Drops
        TopBidsAndDropsSetViewModel? completeTopBidsAndDropsViewModel = null;
        if (homePageData.TopBidsAndDropsData is not null)
        {
            var topBidsAndDropsViewModels = homePageData.TopBidsAndDropsData.TopBidsAndDrops.Select(x => new TopBidsAndDropsGameViewModel(x, currentDate)).ToList();
            completeTopBidsAndDropsViewModel = new TopBidsAndDropsSetViewModel(topBidsAndDropsViewModels, homePageData.TopBidsAndDropsData.ProcessDate);
        }

        //My Game News
        var myGameNews = BuildMyGameNews(homePageData.MyGameDetails, currentDate);
        var myGameNewsViewModel = new GameNewsViewModel(myGameNews, currentDate);

        //Public Leagues
        var publicLeagueViewModels = homePageData.PublicLeagueYears.Select(leagueYear => new PublicLeagueYearViewModel(leagueYear)).ToList();

        //Active Royale Quarter
        var activeRoyaleQuarterViewModel = new RoyaleYearQuarterViewModel(homePageData.ActiveRoyaleYearQuarter);

        var vm = new
        {
            MyLeagues = myLeagueViewModels,
            MyInvites = myInviteViewModels,
            MyConferences = myConferenceViewModels,
            TopBidsAndDrops = completeTopBidsAndDropsViewModel,
            MyGameNews = myGameNewsViewModel,
            PublicLeagues = publicLeagueViewModels,
            ActiveRoyaleQuarter = activeRoyaleQuarterViewModel,
            UserRoyalePublisherID = homePageData.ActiveYearQuarterRoyalePublisherID
        };

        return Ok(vm);
    }

    private static MyGameNewsSet BuildMyGameNews(IReadOnlyList<SingleGameNews> myGameDetails, LocalDate currentDate)
    {
        var upcomingReleases = myGameDetails
            .Where(x => x.MasterGameYear.MasterGame.GetDefiniteMaximumReleaseDate() > currentDate)
            .Take(10)
            .OrderBy(x => x.MasterGameYear.MasterGame.GetDefiniteMaximumReleaseDate())
            .ToList();

        var recentReleases = myGameDetails
            .Where(x => x.MasterGameYear.MasterGame.GetDefiniteMaximumReleaseDate() <= currentDate)
            .Take(10)
            .OrderByDescending(x => x.MasterGameYear.MasterGame.GetDefiniteMaximumReleaseDate())
            .ToList();

        return new MyGameNewsSet(upcomingReleases, recentReleases);
    }
}
