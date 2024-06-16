using FantasyCritic.Lib.Domain.ScoringSystems;
using FantasyCritic.Lib.Identity;
using FantasyCritic.Lib.SharedSerialization.Database;
using FantasyCritic.Web.Helpers;
using FantasyCritic.Web.Models.Responses;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using NodaTime.Serialization.JsonNet;

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

    //The null conditional after User is required! Don't remove it even if Visual Studio says it's unneeded.
    protected string? GetUserIDFromClaims() => User?.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;

    protected async Task<Result<FantasyCriticUser>> GetCurrentUser()
    {
        var userID = GetUserIDFromClaims();
        if (userID is null)
        {
            return Result.Failure<FantasyCriticUser>("Can't get User ID");
        }

        if (_currentUser is not null && _currentUser.Id.ToString() == userID)
        {
            return Result.Success(_currentUser);
        }

        var sessionUserString = HttpContext.Session.GetString("current-user");
        if (!string.IsNullOrWhiteSpace(sessionUserString))
        {
            var deserialized = JsonConvert.DeserializeObject<FantasyCriticUserEntity>(sessionUserString, new JsonSerializerSettings().ConfigureForNodaTime(DateTimeZoneProviders.Tzdb));
            var domain = deserialized?.ToDomain();
            if (domain is not null && domain.Id.ToString() == userID)
            {
                return domain;
            }
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

    protected static LeagueOptionsViewModel BuildLeagueOptionsViewModel(IReadOnlyList<SupportedYear> supportedYears)
    {
        var openYears = supportedYears.Where(x => x.OpenForCreation && !x.Finished).Select(x => x.Year);
        LeagueOptionsViewModel viewModel = new LeagueOptionsViewModel(openYears, DraftSystem.GetAllPossibleValues(),
            PickupSystem.GetAllPossibleValues(), TiebreakSystem.GetAllPossibleValues(),
            ScoringSystem.GetAllPossibleValues(), TradingSystem.GetAllPossibleValues(),
            ReleaseSystem.GetAllPossibleValues());

        return viewModel;
    }
}
