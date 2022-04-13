using FantasyCritic.Lib.Identity;
using FantasyCritic.Web.Models.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FantasyCritic.Web.Controllers.API;

[Route("api/[controller]/[action]")]
[Authorize]
public class AccountController : FantasyCriticController
{
    public AccountController(FantasyCriticUserManager userManager) :
        base(userManager)
    {

    }

    public async Task<ActionResult> CurrentUser()
    {
        var currentUserResult = await GetCurrentUser();
        if (currentUserResult.IsFailure)
        {
            return BadRequest(currentUserResult.Error);
        }
        var currentUser = currentUserResult.Value;

        var roles = await _userManager.GetRolesAsync(currentUser);
        FantasyCriticUserViewModel vm = new FantasyCriticUserViewModel(currentUser, roles);
        return Ok(vm);
    }
}
