using FantasyCritic.Lib.Identity;
using FantasyCritic.Web.Models.Requests.Conference;
using FantasyCritic.Web.Models.Requests.League;
using FantasyCritic.Web.Models.Requests.LeagueManager;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace FantasyCritic.Web.Controllers.API;

[Route("api/[controller]/[action]")]
[Authorize]
public class ConferenceController : FantasyCriticController
{
    private readonly IClock _clock;
    private readonly ILogger<ConferenceController> _logger;

    public ConferenceController(IClock clock, ILogger<ConferenceController> logger, FantasyCriticUserManager userManager) : base(userManager)
    {
        _clock = clock;
        _logger = logger;
    }

    [HttpPost]
    [Authorize("Write")]
    public async Task<IActionResult> CreateConference([FromBody] CreateConferenceRequest request)
    {
        var currentUser = await GetCurrentUserOrThrow();

        var requestValid = request.IsValid();
        if (requestValid.IsFailure)
        {
            return BadRequest(requestValid.Error);
        }

        throw new NotImplementedException();
    }

    [HttpPost]
    [Authorize("Write")]
    public async Task<IActionResult> EditConference([FromBody] EditConferenceRequest request)
    {
        var currentUser = await GetCurrentUserOrThrow();
        throw new NotImplementedException();
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetConference(int id)
    {
        var currentUser = await GetCurrentUserOrThrow();
        throw new NotImplementedException();
    }

    [HttpPost]
    [Authorize("Write")]
    public async Task<IActionResult> CreateInviteLink([FromBody] CreateInviteLinkRequest request)
    {
        var currentUser = await GetCurrentUserOrThrow();
        throw new NotImplementedException();
    }

    [HttpPost]
    [Authorize("Write")]
    public async Task<IActionResult> DeleteInviteLink([FromBody] DeleteInviteLinkRequest request)
    {
        var currentUser = await GetCurrentUserOrThrow();
        throw new NotImplementedException();
    }

    [HttpPost]
    [Authorize("Write")]
    public async Task<IActionResult> JoinWithInviteLink([FromBody] JoinWithInviteLinkRequest request)
    {
        var currentUser = await GetCurrentUserOrThrow();
        throw new NotImplementedException();
    }

    [HttpPost]
    [Authorize("Write")]
    public async Task<IActionResult> AddLeagueToConference([FromBody] AddLeagueToConferenceRequest request)
    {
        var currentUser = await GetCurrentUserOrThrow();
        throw new NotImplementedException();
    }

    [HttpPost]
    [Authorize("Write")]
    public async Task<IActionResult> PromoteNewConferenceManager([FromBody] PromoteNewConferenceManagerRequest request)
    {
        var currentUser = await GetCurrentUserOrThrow();
        throw new NotImplementedException();
    }

    [HttpPost]
    [Authorize("Write")]
    public async Task<IActionResult> ReassignLeagueManager([FromBody] ReassignLeagueManagerRequest request)
    {
        var currentUser = await GetCurrentUserOrThrow();
        throw new NotImplementedException();
    }

    [HttpPost]
    [Authorize("Write")]
    public async Task<IActionResult> AssignLeaguePlayers([FromBody] AssignLeaguePlayersRequest request)
    {
        var currentUser = await GetCurrentUserOrThrow();
        throw new NotImplementedException();
    }

    [HttpPost]
    [Authorize("Write")]
    public async Task<IActionResult> EditDraftStatusForConferenceYear([FromBody] EditDraftStatusForConferenceYearRequest request)
    {
        var currentUser = await GetCurrentUserOrThrow();
        throw new NotImplementedException();
    }

    [HttpPost]
    [Authorize("Write")]
    public async Task<IActionResult> CreateConferenceManagerMessage([FromBody] CreateConferenceManagerMessageRequest request)
    {
        var currentUser = await GetCurrentUserOrThrow();
        throw new NotImplementedException();
    }

    [HttpPost]
    [Authorize("Write")]
    public async Task<IActionResult> DeleteConferenceManagerMessage([FromBody] DeleteConferenceManagerMessageRequest request)
    {
        var currentUser = await GetCurrentUserOrThrow();
        throw new NotImplementedException();
    }

    [HttpPost]
    [Authorize("Write")]
    public async Task<IActionResult> DismissManagerMessage([FromBody] DismissManagerMessageRequest request)
    {
        var currentUser = await GetCurrentUserOrThrow();
        throw new NotImplementedException();
    }
}
