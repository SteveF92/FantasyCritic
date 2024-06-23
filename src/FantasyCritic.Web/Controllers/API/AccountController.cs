using FantasyCritic.Lib.Identity;
using FantasyCritic.Lib.SharedSerialization.API;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

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

        IList<string> userRoles;
        var sessionUserRoles = HttpContext.Session.GetString("current-user-roles");
        if (!string.IsNullOrWhiteSpace(sessionUserRoles))
        {
            userRoles = JsonConvert.DeserializeObject<string[]>(sessionUserRoles)!;
        }
        else
        {
            userRoles = await _userManager.GetRolesAsync(currentUser);
            var jsonString = JsonConvert.SerializeObject(userRoles);
            HttpContext.Session.SetString("current-user-roles", jsonString);
        }

        FantasyCriticUserViewModel vm = new FantasyCriticUserViewModel(currentUser, userRoles);
        return Ok(vm);
    }
}
