using FantasyCritic.Lib.Extensions;
using FantasyCritic.Lib.Identity;
using FantasyCritic.Lib.Services;
using FantasyCritic.Web.Models.Responses;
using Microsoft.AspNetCore.Mvc;

namespace FantasyCritic.Web.Controllers.API;

[Route("api/[controller]/[action]")]
public class GeneralController : ControllerBase
{
    private readonly FantasyCriticService _fantasyCriticService;
    private readonly InterLeagueService _interLeagueService;
    private readonly FantasyCriticUserManager _userManager;
    private readonly IClock _clock;

    public GeneralController(FantasyCriticService fantasyCriticService, InterLeagueService interLeagueService, FantasyCriticUserManager userManager, IClock clock)
    {
        _fantasyCriticService = fantasyCriticService;
        _interLeagueService = interLeagueService;
        _userManager = userManager;
        _clock = clock;
    }

    public async Task<IActionResult> SiteCounts()
    {
        var counts = await _interLeagueService.GetSiteCounts();
        return Ok(new SiteCountsViewModel(counts));
    }

    public async Task<IActionResult> Donors()
    {
        var donors = await _userManager.GetDonors();
        return Ok(donors);
    }

    public async Task<IActionResult> BidTimes()
    {
        var nextPublicRevealTime = _clock.GetNextPublicRevealTime();
        var nextBidTime = _clock.GetNextBidTime();
        var systemWideSettings = await _interLeagueService.GetSystemWideSettings();
        return Ok(new BidTimesViewModel(nextPublicRevealTime, nextBidTime, systemWideSettings.ActionProcessingMode));
    }
}
