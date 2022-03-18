using FantasyCritic.Lib.Identity;
using FantasyCritic.Lib.Services;
using FantasyCritic.Web.Helpers;
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

    protected async Task<(Maybe<LeagueRecord> ValidResult, Maybe<IActionResult> FailedResult)> GetExistingLeague(Guid leagueID, bool failIfActionProcessing, bool mustBeLeagueManager)
    {
        var currentUserRecord = await GetCurrentUser();
        if (currentUserRecord.FailedResult.HasValue)
        {
            return (Maybe<LeagueRecord>.None, currentUserRecord.FailedResult);
        }

        if (failIfActionProcessing)
        {
            var systemWideSettings = await _interLeagueService.GetSystemWideSettings();
            if (systemWideSettings.ActionProcessingMode)
            {
                return GetFailedResult<LeagueRecord>(BadRequest("Site is in read-only mode while actions process."));
            }
        }

        var league = await _fantasyCriticService.GetLeagueByID(leagueID);
        if (league.HasNoValue)
        {
            return GetFailedResult<LeagueRecord>(BadRequest("League does not exist."));
        }

        if (mustBeLeagueManager && league.Value.LeagueManager.Id != currentUserRecord.ValidResult.Value.Id)
        {
            return GetFailedResult<LeagueRecord>(Forbid("You are not the manager of that league."));
        }

        return (new LeagueRecord(currentUserRecord.ValidResult.Value, league.Value), Maybe<IActionResult>.None);
    }

    protected async Task<(Maybe<LeagueYearRecord> ValidResult, Maybe<IActionResult> FailedResult)> GetExistingLeagueYear(Guid leagueID, int year, bool failIfActionProcessing, bool mustBeLeagueManager)
    {
        var currentUserRecord = await GetCurrentUser();
        if (currentUserRecord.FailedResult.HasValue)
        {
            return (Maybe<LeagueYearRecord>.None, currentUserRecord.FailedResult);
        }

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

        if (mustBeLeagueManager && leagueYear.Value.League.LeagueManager.Id != currentUserRecord.ValidResult.Value.Id)
        {
            return GetFailedResult<LeagueYearRecord>(Forbid("You are not the manager of that league."));
        }

        return (new LeagueYearRecord(currentUserRecord.ValidResult.Value, leagueYear.Value), Maybe<IActionResult>.None);
    }

    protected async Task<(Maybe<LeagueYearPublisherRecord> ValidResult, Maybe<IActionResult> FailedResult)> GetExistingLeagueYearAndPublisher(Guid leagueID, int year, Guid publisherID, bool failIfActionProcessing, bool mustBeLeagueManager)
    {
        var leagueYearRecord = await GetExistingLeagueYear(leagueID, year, failIfActionProcessing, mustBeLeagueManager);
        if (leagueYearRecord.FailedResult.HasValue)
        {
            return (Maybe<LeagueYearPublisherRecord>.None, leagueYearRecord.FailedResult);
        }

        var publisher = leagueYearRecord.ValidResult.Value.LeagueYear.GetPublisherByID(publisherID);
        if (publisher.HasNoValue)
        {
            return GetFailedResult<LeagueYearPublisherRecord>(BadRequest("Publisher does not exist in that league."));
        }

        return (new LeagueYearPublisherRecord(leagueYearRecord.ValidResult.Value.CurrentUser, leagueYearRecord.ValidResult.Value.LeagueYear, publisher.Value), Maybe<IActionResult>.None);
    }

    protected async Task<(Maybe<LeagueYearPublisherRecord> LeagueYearPublisherRecord, Maybe<IActionResult> FailedResult)> GetExistingLeagueYearAndPublisher(Guid leagueID, int year, FantasyCriticUser userForPublisher, bool failIfActionProcessing, bool mustBeLeagueManager)
    {
        var leagueYearRecord = await GetExistingLeagueYear(leagueID, year, failIfActionProcessing, mustBeLeagueManager);
        if (leagueYearRecord.FailedResult.HasValue)
        {
            return (Maybe<LeagueYearPublisherRecord>.None, leagueYearRecord.FailedResult);
        }

        var publisher = leagueYearRecord.ValidResult.Value.LeagueYear.GetUserPublisher(userForPublisher);
        if (publisher.HasNoValue)
        {
            return GetFailedResult<LeagueYearPublisherRecord>(BadRequest("That user does not have a publisher in that league."));
        }

        return (new LeagueYearPublisherRecord(leagueYearRecord.ValidResult.Value.CurrentUser, leagueYearRecord.ValidResult.Value.LeagueYear, publisher.Value), Maybe<IActionResult>.None);
    }

    protected async Task<(Maybe<LeagueYearPublisherGameRecord> ValidResult, Maybe<IActionResult> FailedResult)> GetExistingLeagueYearAndPublisherGame(Guid leagueID, int year, Guid publisherID, Guid publisherGameID, bool failIfActionProcessing, bool mustBeLeagueManager)
    {
        var leagueYearPublisherRecord = await GetExistingLeagueYearAndPublisher(leagueID, year, publisherID, failIfActionProcessing, mustBeLeagueManager);
        if (leagueYearPublisherRecord.FailedResult.HasValue)
        {
            return (Maybe<LeagueYearPublisherGameRecord>.None, leagueYearPublisherRecord.FailedResult);
        }

        var publisherGame = leagueYearPublisherRecord.ValidResult.Value.Publisher.PublisherGames.SingleOrDefault(x => x.PublisherGameID == publisherGameID);
        if (publisherGame is null)
        {
            return GetFailedResult<LeagueYearPublisherGameRecord>(BadRequest("That publisher game does not exist."));
        }

        return (new LeagueYearPublisherGameRecord(leagueYearPublisherRecord.ValidResult.Value.CurrentUser, leagueYearPublisherRecord.ValidResult.Value.LeagueYear,
            leagueYearPublisherRecord.ValidResult.Value.Publisher, publisherGame), Maybe<IActionResult>.None);
    }
}
