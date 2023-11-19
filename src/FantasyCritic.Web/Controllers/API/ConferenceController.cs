using FantasyCritic.Lib.Domain.Requests;
using FantasyCritic.Lib.Identity;
using FantasyCritic.Lib.Services;
using FantasyCritic.Web.Models.Requests.Conference;
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
}
