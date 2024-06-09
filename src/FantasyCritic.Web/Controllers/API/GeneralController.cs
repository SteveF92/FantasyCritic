using FantasyCritic.Lib.Extensions;
using FantasyCritic.Lib.Identity;
using FantasyCritic.Lib.Services;
using FantasyCritic.Web.Models.Responses;
using Microsoft.AspNetCore.Mvc;

namespace FantasyCritic.Web.Controllers.API;

[Route("api/[controller]/[action]")]
public class GeneralController : ControllerBase
{
    private readonly InterLeagueService _interLeagueService;
    private readonly FantasyCriticUserManager _userManager;
    private readonly IClock _clock;
    private readonly GameController _gameController;
    private readonly LeagueController _leagueController;

    public GeneralController(InterLeagueService interLeagueService, FantasyCriticUserManager userManager, IClock clock,
        GameController gameController, LeagueController leagueController)
    {
        _interLeagueService = interLeagueService;
        _userManager = userManager;
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
        var nextPublicRevealTime = _clock.GetNextPublicRevealTime();
        var nextBidTime = _clock.GetNextBidTime();
        var systemWideSettings = await _interLeagueService.GetSystemWideSettings();
        return Ok(new BidTimesViewModel(nextPublicRevealTime, nextBidTime, systemWideSettings.ActionProcessingMode));
    }

    public async Task<ActionResult> BasicData()
    {
        var bidsTask = BidTimes();
        var tagsTask = _gameController.GetMasterGameTags();
        var leagueOptions = _leagueController.LeagueOptions();

        await Task.WhenAll([bidsTask, tagsTask, leagueOptions]);

        var vm = new
        {
            BidTimes = (await bidsTask).Value,
            MasterGameTags = (await tagsTask).Value,
            LeagueOptions = (await leagueOptions).Value,
        };

        return Ok(vm);
    }
}
