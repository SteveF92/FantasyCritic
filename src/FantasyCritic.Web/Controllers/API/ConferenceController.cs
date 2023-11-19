using FantasyCritic.Lib.Identity;
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

  }
