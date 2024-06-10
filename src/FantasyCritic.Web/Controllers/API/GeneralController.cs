using FantasyCritic.Lib.Extensions;
using FantasyCritic.Lib.Identity;
using FantasyCritic.Lib.Services;
using FantasyCritic.SharedSerialization.API;
using FantasyCritic.Web.Models.Responses;
using Microsoft.AspNetCore.Mvc;

namespace FantasyCritic.Web.Controllers.API;

[Route("api/[controller]/[action]")]
public class GeneralController : FantasyCriticController
{
    private readonly InterLeagueService _interLeagueService;
    private readonly IClock _clock;
    private readonly GameController _gameController;
    private readonly LeagueController _leagueController;

    public GeneralController(InterLeagueService interLeagueService, FantasyCriticUserManager userManager, IClock clock,
        GameController gameController, LeagueController leagueController) : base(userManager)
    {
        _interLeagueService = interLeagueService;
        _clock = clock;
        _gameController = gameController;
        _leagueController = leagueController;
    }

    public async Task<ActionResult<SiteCountsViewModel>> SiteCounts()
    {
        var counts = await _interLeagueService.GetSiteCounts();
        return Ok(new SiteCountsViewModel(counts));
    }

    public async Task<ActionResult<List<string>>> Donors()
    {
        var donors = await _userManager.GetDonors();
        return Ok(donors);
    }

    public async Task<ActionResult<BidTimesViewModel>> BidTimes()
    {
        var systemWideSettings = await _interLeagueService.GetSystemWideSettings();
        var vm = BuildBidTimesViewModel(systemWideSettings);
        return Ok(vm);
    }

    public async Task<ActionResult> BasicData()
    {
        var basicData = await _interLeagueService.GetBasicData();

        var bidTimes = BuildBidTimesViewModel(basicData.SystemWideSettings);
        var masterGameTags = basicData.MasterGameTags.Select(x => new MasterGameTagViewModel(x)).ToList();
        var leagueOptions = BuildLeagueOptionsViewModel(basicData.SupportedYears);
        var vm = new
        {
            BidTimes = bidTimes,
            MasterGameTags = masterGameTags,
            LeagueOptions = leagueOptions,
        };

        return Ok(vm);
    }

    private BidTimesViewModel BuildBidTimesViewModel(SystemWideSettings systemWideSettings)
    {
        var nextPublicRevealTime = _clock.GetNextPublicRevealTime();
        var nextBidTime = _clock.GetNextBidTime();
        return new BidTimesViewModel(nextPublicRevealTime, nextBidTime, systemWideSettings.ActionProcessingMode);
    }
}
