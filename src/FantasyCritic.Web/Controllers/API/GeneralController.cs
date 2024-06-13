using FantasyCritic.Lib.Identity;
using FantasyCritic.Lib.Services;
using FantasyCritic.Web.Models.Responses;
using FantasyCritic.Web.Utilities;
using Microsoft.AspNetCore.Mvc;

namespace FantasyCritic.Web.Controllers.API;

[Route("api/[controller]/[action]")]
public class GeneralController : FantasyCriticController
{
    private readonly InterLeagueService _interLeagueService;
    private readonly IClock _clock;


    public GeneralController(InterLeagueService interLeagueService, FantasyCriticUserManager userManager, IClock clock) : base(userManager)
    {
        _interLeagueService = interLeagueService;
        _clock = clock;
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
        var vm = DomainControllerUtilities.BuildBidTimesViewModel(_clock, systemWideSettings);
        return Ok(vm);
    }
}
