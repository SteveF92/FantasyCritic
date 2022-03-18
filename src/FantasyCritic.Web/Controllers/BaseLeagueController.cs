using FantasyCritic.Lib.Identity;
using FantasyCritic.Lib.Services;
using Microsoft.AspNetCore.Mvc;

namespace FantasyCritic.Web.Controllers;
public abstract class BaseLeagueController : FantasyCriticController
{
    protected readonly FantasyCriticService _fantasyCriticService;
    protected readonly InterLeagueService _interLeagueService;

    protected BaseLeagueController(FantasyCriticUserManager userManager, FantasyCriticService fantasyCriticService, InterLeagueService interLeagueService) : base(userManager)
    {
        _fantasyCriticService = fantasyCriticService;
        _interLeagueService = interLeagueService;
    }

    protected async Task<(Maybe<LeagueYearRecord> LeagueYearRecord, Maybe<IActionResult> FailedResult)> GetExistingLeagueYear(Guid leagueID, int year, bool failIfActionProcessing, bool mustBeLeagueManager)
    {
        if (!ModelState.IsValid)
        {
            return GetFailedResult<LeagueYearRecord>(BadRequest("Invalid request."));
        }

        var currentUserResult = await GetCurrentUser();
        if (currentUserResult.IsFailure)
        {
            return GetFailedResult<LeagueYearRecord>(BadRequest(currentUserResult.Error));
        }
        var currentUser = currentUserResult.Value;

        if (failIfActionProcessing)
        {
            var systemWideSettings = await _interLeagueService.GetSystemWideSettings();
            if (systemWideSettings.ActionProcessingMode)
            {
                return GetFailedResult<LeagueYearRecord>(BadRequest("Site is in read-only mode while actions process."));
            }
        }

        var leagueYear = await _fantasyCriticService.GetLeagueYear(leagueID, year);
        if (leagueYear.HasNoValue)
        {
            return GetFailedResult<LeagueYearRecord>(BadRequest("League year does not exist."));
        }

        if (mustBeLeagueManager && leagueYear.Value.League.LeagueManager.Id != currentUser.Id)
        {
            return GetFailedResult<LeagueYearRecord>(Forbid("You are not the manager of that league."));
        }

        return (new LeagueYearRecord(currentUser, leagueYear.Value), Maybe<IActionResult>.None);
    }

    protected async Task<(Maybe<LeagueYearPublisherRecord> LeagueYearPublisherRecord, Maybe<IActionResult> FailedResult)> GetExistingLeagueYearAndPublisher(Guid leagueID, int year, Guid publisherID, bool failIfActionProcessing, bool mustBeLeagueManager)
    {
        var leagueYearRecord = await GetExistingLeagueYear(leagueID, year, failIfActionProcessing, mustBeLeagueManager);
        if (leagueYearRecord.FailedResult.HasValue)
        {
            return (Maybe<LeagueYearPublisherRecord>.None, leagueYearRecord.FailedResult);
        }

        var publisher = leagueYearRecord.LeagueYearRecord.Value.LeagueYear.GetPublisherByID(publisherID);
        if (publisher.HasNoValue)
        {
            return GetFailedResult<LeagueYearPublisherRecord>(BadRequest("Publisher does not exist in that league."));
        }

        return (new LeagueYearPublisherRecord(leagueYearRecord.LeagueYearRecord.Value.CurrentUser, leagueYearRecord.LeagueYearRecord.Value.LeagueYear, publisher.Value), Maybe<IActionResult>.None);
    }

    protected async Task<(Maybe<LeagueYearPublisherRecord> LeagueYearPublisherRecord, Maybe<IActionResult> FailedResult)> GetExistingLeagueYearAndPublisher(Guid leagueID, int year, FantasyCriticUser userForPublisher, bool failIfActionProcessing, bool mustBeLeagueManager)
    {
        var leagueYearRecord = await GetExistingLeagueYear(leagueID, year, failIfActionProcessing, mustBeLeagueManager);
        if (leagueYearRecord.FailedResult.HasValue)
        {
            return (Maybe<LeagueYearPublisherRecord>.None, leagueYearRecord.FailedResult);
        }

        var publisher = leagueYearRecord.LeagueYearRecord.Value.LeagueYear.GetUserPublisher(userForPublisher);
        if (publisher.HasNoValue)
        {
            return GetFailedResult<LeagueYearPublisherRecord>(BadRequest("That user does not have a publisher in that league."));
        }

        return (new LeagueYearPublisherRecord(leagueYearRecord.LeagueYearRecord.Value.CurrentUser, leagueYearRecord.LeagueYearRecord.Value.LeagueYear, publisher.Value), Maybe<IActionResult>.None);
    }

    protected async Task<(Maybe<LeagueYearPublisherGameRecord> LeagueYearPublisherGameRecord, Maybe<IActionResult> FailedResult)> GetExistingLeagueYearAndPublisherGame(Guid leagueID, int year, Guid publisherID, Guid publisherGameID, bool failIfActionProcessing, bool mustBeLeagueManager)
    {
        var leagueYearPublisherRecord = await GetExistingLeagueYearAndPublisher(leagueID, year, publisherID, failIfActionProcessing, mustBeLeagueManager);
        if (leagueYearPublisherRecord.FailedResult.HasValue)
        {
            return (Maybe<LeagueYearPublisherGameRecord>.None, leagueYearPublisherRecord.FailedResult);
        }

        var publisherGame = leagueYearPublisherRecord.LeagueYearPublisherRecord.Value.Publisher.PublisherGames.SingleOrDefault(x => x.PublisherGameID == publisherGameID);
        if (publisherGame is null)
        {
            return GetFailedResult<LeagueYearPublisherGameRecord>(BadRequest("That publisher game does not exist."));
        }

        return (new LeagueYearPublisherGameRecord(leagueYearPublisherRecord.LeagueYearPublisherRecord.Value.CurrentUser, leagueYearPublisherRecord.LeagueYearPublisherRecord.Value.LeagueYear,
            leagueYearPublisherRecord.LeagueYearPublisherRecord.Value.Publisher, publisherGame), Maybe<IActionResult>.None);
    }

    private static (Maybe<T> ValidRecord, Maybe<IActionResult> FailedResult) GetFailedResult<T>(IActionResult failedResult) => (Maybe<T>.None, Maybe<IActionResult>.From(failedResult));
}
