using FantasyCritic.Lib.Identity;
using FantasyCritic.Web.Models.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace FantasyCritic.Web.Controllers.API;

[Route("api/[controller]/[action]")]
[Authorize]
public class AccountController : FantasyCriticController
{
    private readonly FantasyCriticUserManager _userManager;
    private readonly FantasyCriticRoleManager _roleManager;
    private readonly IEmailSender _emailSender;
    private readonly ILogger _logger;
    private readonly IClock _clock;

    public AccountController(FantasyCriticUserManager userManager, FantasyCriticRoleManager roleManager,
        IEmailSender emailSender, ILogger<AccountController> logger, IClock clock) :
        base(userManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _emailSender = emailSender;
        _logger = logger;
        _clock = clock;
    }

    public async Task<ActionResult> CurrentUser()
    {
        var currentUserResult = await GetCurrentUser();
        if (currentUserResult.FailedResult.HasValue)
        {
            return BadRequest(currentUserResult.FailedResult);
        }
        var currentUser = currentUserResult.ValidResult.Value;

        var roles = await _userManager.GetRolesAsync(currentUser);
        FantasyCriticUserViewModel vm = new FantasyCriticUserViewModel(currentUser, roles);
        return Ok(vm);
    }
}
