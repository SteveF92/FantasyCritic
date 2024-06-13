using FantasyCritic.Lib.Identity;
using FantasyCritic.Lib.Services;
using FantasyCritic.SharedSerialization.API;
using FantasyCritic.Web.Models.Responses;
using FantasyCritic.Web.Models.Responses.Conferences;
using FantasyCritic.Web.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FantasyCritic.Web.Controllers.API;

[Authorize]
[Route("api/[controller]/[action]")]
public class CombinedDataController : FantasyCriticController
{
    private readonly InterLeagueService _interLeagueService;
    private readonly LeagueMemberService _leagueMemberService;
    private readonly ConferenceService _conferenceService;
    private readonly IClock _clock;

    public CombinedDataController(FantasyCriticUserManager userManager, InterLeagueService interLeagueService,
        LeagueMemberService leagueMemberService, ConferenceService conferenceService,
        IClock clock) : base(userManager)
    {
        _interLeagueService = interLeagueService;
        _leagueMemberService = leagueMemberService;
        _conferenceService = conferenceService;
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
        var viewModels = conferences
            .Select(conference => new MinimalConferenceViewModel(conference, conference.ConferenceManager.UserID == currentUser.UserID))
            .OrderBy(x => x.ConferenceName)
            .ToList();

        var vm = new
        {
            MyLeagues = myLeagueViewModels,
            MyInvites = myInviteViewModels
        };

        return Ok(vm);
    }
}
