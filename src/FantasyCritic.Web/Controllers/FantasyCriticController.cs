using FantasyCritic.Lib.Identity;
using FantasyCritic.Web.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace FantasyCritic.Web.Controllers;

[ApiController]
public abstract class FantasyCriticController : ControllerBase
{
    protected readonly FantasyCriticUserManager _userManager;
    private FantasyCriticUser? _currentUser;

    protected FantasyCriticController(FantasyCriticUserManager userManager)
    {
        _userManager = userManager;
    }

    protected async Task<Result<FantasyCriticUser>> GetCurrentUser()
    {
        if (User is null)
        {
            return Result.Failure<FantasyCriticUser>("Not authenticated.");
        }

        var userID = User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;
        if (userID is null)
        {
            return Result.Failure<FantasyCriticUser>("Can't get User ID");
        }

        if (_currentUser is not null && _currentUser.Id.ToString() == userID)
        {
            return Result.Success(_currentUser);
        }

        var currentUser = await _userManager.FindByIdAsync(userID);
        if (currentUser is null)
        {
            return Result.Failure<FantasyCriticUser>("User does not exist.");
        }

        _currentUser = currentUser;
        return Result.Success(currentUser);
    }

    protected async Task<FantasyCriticUser> GetCurrentUserOrThrow()
    {
        var userResult = await GetCurrentUser();
        if (userResult.IsFailure)
        {
            throw new Exception(userResult.Error);
        }

        return userResult.Value;
    }

    protected static GenericResultRecord<T> GetFailedResult<T>(IActionResult failedResult) where T : class => new GenericResultRecord<T>(null, failedResult);
    protected GenericResultRecord<T> UnauthorizedOrForbid<T>(bool hasUser) where T : class
    {
        if (hasUser)
        {
            return new GenericResultRecord<T>(null, StatusCode(403));
        }

        return new GenericResultRecord<T>(null, Unauthorized());
    }

    protected IActionResult UnauthorizedOrForbid(bool hasUser)
    {
        if (hasUser)
        {
            return StatusCode(403);
        }

        return Unauthorized();
    }
}
