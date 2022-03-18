using FantasyCritic.Lib.Identity;
using Microsoft.AspNetCore.Mvc;

namespace FantasyCritic.Web.Controllers;

public abstract class FantasyCriticController : ControllerBase
{
    private readonly FantasyCriticUserManager _userManager;

    protected FantasyCriticController(FantasyCriticUserManager userManager)
    {
        _userManager = userManager;
    }

    protected async Task<(Maybe<FantasyCriticUser> ValidResult, Maybe<IActionResult> FailedResult)> GetCurrentUser()
    {
        if (!ModelState.IsValid)
        {
            return GetFailedResult<FantasyCriticUser>(BadRequest("Invalid request."));
        }

        var currentUserResult = await GetCurrentUserInternal();
        if (currentUserResult.IsFailure)
        {
            return GetFailedResult<FantasyCriticUser>(BadRequest(currentUserResult.Error));
        }

        return (currentUserResult.Value, Maybe<IActionResult>.None);
    }

    private async Task<Result<FantasyCriticUser>> GetCurrentUserInternal()
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

    protected static (Maybe<T> ValidRecord, Maybe<IActionResult> FailedResult) GetFailedResult<T>(IActionResult failedResult) => (Maybe<T>.None, Maybe<IActionResult>.From(failedResult));
}
