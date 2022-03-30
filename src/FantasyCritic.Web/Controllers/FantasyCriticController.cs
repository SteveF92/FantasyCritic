using FantasyCritic.Lib.Identity;
using FantasyCritic.Web.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace FantasyCritic.Web.Controllers;

[ApiController]
public abstract class FantasyCriticController : ControllerBase
{
    protected readonly FantasyCriticUserManager _userManager;

    protected FantasyCriticController(FantasyCriticUserManager userManager)
    {
        _userManager = userManager;
    }

    protected async Task<Result<FantasyCriticUser>> GetCurrentUser()
    {
        var userID = User.Claims.SingleOrDefault(x => x.Type == "sub")?.Value;
        if (userID is null)
        {
            return Result.Failure<FantasyCriticUser>("Can't get User ID");
        }

        var currentUser = await _userManager.FindByIdAsync(userID);
        if (currentUser is null)
        {
            return Result.Failure<FantasyCriticUser>("User does not exist.");
        }

        return Result.Success(currentUser);
    }

    protected static GenericResultRecord<T> GetFailedResult<T>(IActionResult failedResult) where T : class => new GenericResultRecord<T>(null, failedResult);
}
