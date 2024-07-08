using FantasyCritic.Lib.Extensions;
using FantasyCritic.Lib.Identity;
using FantasyCritic.Lib.Royale;
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
    private readonly RoyaleService _royaleService;
    private readonly IClock _clock;

    public CombinedDataController(FantasyCriticUserManager userManager, FantasyCriticService fantasyCriticService,
        InterLeagueService interLeagueService, RoyaleService royaleService, IClock clock)
        : base(userManager)
    {
        _fantasyCriticService = fantasyCriticService;
        _interLeagueService = interLeagueService;
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
        var myGameNews = MyGameNewsSet.BuildMyGameNews(homePageData.MyGameDetails, currentDate);
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

    [AllowAnonymous]
    [HttpGet("{year}/{quarter}")]
    public async Task<IActionResult> RoyaleData(int year, int quarter)
    {
        //Royale Year Quarter
        var royaleYearQuarter = await _royaleService.GetYearQuarter(year, quarter);
        if (royaleYearQuarter is null)
        {
            return NotFound();
        }

        var royaleYearQuarterViewModel = new RoyaleYearQuarterViewModel(royaleYearQuarter);

        var masterGameTags = await _interLeagueService.GetMasterGameTags();

        //User Royale Publisher
        RoyalePublisherViewModel? userRoyalePublisherViewModel = null;
        var currentUser = await GetCurrentUser();
        if (currentUser.IsSuccess)
        {
            RoyalePublisher? publisher = await _royaleService.GetPublisher(royaleYearQuarter, currentUser.Value);
            if (publisher is not null)
            {
                IReadOnlyList<RoyaleYearQuarter> quartersWon = await _royaleService.GetQuartersWonByUser(publisher.User);
                var currentDate = _clock.GetToday();
                userRoyalePublisherViewModel = new RoyalePublisherViewModel(publisher, currentDate, null, quartersWon, masterGameTags, true);
            }
        }

        //Standings
        IReadOnlyList<RoyalePublisher> publishers = await _royaleService.GetAllPublishers(year, quarter);
        var publishersToShow = publishers.Where(x => x.PublisherGames.Any()).OrderByDescending(x => x.GetTotalFantasyPoints());
        int ranking = 1;
        List<RoyalePublisherViewModel> publisherViewModels = new List<RoyalePublisherViewModel>();
        IReadOnlyDictionary<FantasyCriticUser, IReadOnlyList<RoyaleYearQuarter>> previousWinners = await _royaleService.GetRoyaleWinners();
        foreach (var publisher in publishersToShow)
        {
            int? thisRanking = null;
            if (publisher.GetTotalFantasyPoints() > 0)
            {
                thisRanking = ranking;
                ranking++;
            }

            var winningQuarters = previousWinners.GetValueOrDefault(publisher.User, new List<RoyaleYearQuarter>());
            bool thisPlayerIsViewing = false;
            if (currentUser.IsSuccess)
            {
                thisPlayerIsViewing = currentUser.Value.Id == publisher.User.Id;
            }

            var currentDate = _clock.GetToday();
            var publisherViewModel = new RoyalePublisherViewModel(publisher, currentDate, thisRanking, winningQuarters, masterGameTags, thisPlayerIsViewing);
            publisherViewModels.Add(publisherViewModel);
        }


        var vm = new
        {
            RoyaleYearQuarter = royaleYearQuarterViewModel,
            UserRoyalePublisher = userRoyalePublisherViewModel,
            RoyaleStandings = publisherViewModels
        };

        return Ok(vm);
    }
}
