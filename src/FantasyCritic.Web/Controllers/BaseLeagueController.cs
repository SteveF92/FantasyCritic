using FantasyCritic.Lib.Extensions;
using FantasyCritic.Lib.Identity;
using FantasyCritic.Lib.Services;
using FantasyCritic.Web.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace FantasyCritic.Web.Controllers;
public abstract class BaseLeagueController : FantasyCriticController
{
    protected readonly FantasyCriticService _fantasyCriticService;
    protected readonly InterLeagueService _interLeagueService;
    protected readonly LeagueMemberService _leagueMemberService;

    protected BaseLeagueController(FantasyCriticUserManager userManager, FantasyCriticService fantasyCriticService, InterLeagueService interLeagueService, LeagueMemberService leagueMemberService) : base(userManager)
    {
        _fantasyCriticService = fantasyCriticService;
        _interLeagueService = interLeagueService;
        _leagueMemberService = leagueMemberService;
    }

    protected async Task<(Maybe<LeagueRecord> ValidResult, Maybe<IActionResult> FailedResult)> GetExistingLeague(Guid leagueID, RequiredRelationship requiredRelationship)
    {
        if (!ModelState.IsValid)
        {
            return GetFailedResult<LeagueRecord>(BadRequest("Invalid request."));
        }

        var currentUserRecord = await GetCurrentUser();
        if ((requiredRelationship.MustBeLoggedIn || requiredRelationship.MustBeInOrInvitedToLeague || requiredRelationship.MustBeLeagueManager) && currentUserRecord.IsFailure)
        {
            return (Maybe<LeagueRecord>.None, Unauthorized());
        }

        var league = await _fantasyCriticService.GetLeagueByID(leagueID);
        if (league.HasNoValueTempoTemp)
        {
            return GetFailedResult<LeagueRecord>(BadRequest("League does not exist."));
        }

        var playersInLeague = await _leagueMemberService.GetUsersWithRemoveStatus(league.ValueTempoTemp);
        bool isInLeague = false;
        Maybe<LeagueInvite> leagueInvite = Maybe<LeagueInvite>.None;
        bool isLeagueManager = false;
        bool userIsAdmin = false;
        if (currentUserRecord.IsSuccess)
        {
            userIsAdmin = await _userManager.IsInRoleAsync(currentUserRecord.Value, "Admin");
            isLeagueManager = league.ValueTempoTemp.LeagueManager.Id == currentUserRecord.Value.Id;
            if (requiredRelationship.MustBeLeagueManager && !isLeagueManager)
            {
                return GetFailedResult<LeagueRecord>(Forbid());
            }

            if (isLeagueManager)
            {
                isInLeague = true;
            }
            else
            {
                isInLeague = playersInLeague.Any(x => x.User.Id == currentUserRecord.Value.Id);
                if (!isInLeague)
                {
                    var inviteesToLeague = await _leagueMemberService.GetOutstandingInvitees(league.ValueTempoTemp);
                    leagueInvite = inviteesToLeague.GetMatchingInvite(currentUserRecord.Value.Email);
                }
            }
        }

        if (!isInLeague && leagueInvite.HasNoValueTempoTemp && requiredRelationship.MustBeInOrInvitedToLeague)
        {
            return GetFailedResult<LeagueRecord>(Forbid());
        }

        LeagueUserRelationship relationship = new LeagueUserRelationship(leagueInvite, isInLeague, isLeagueManager, userIsAdmin);
        return (new LeagueRecord(currentUserRecord.ToMaybe(), league.ValueTempoTemp, playersInLeague, relationship), Maybe<IActionResult>.None);
    }

    protected async Task<(Maybe<LeagueYearRecord> ValidResult, Maybe<IActionResult> FailedResult)> GetExistingLeagueYear(Guid leagueID, int year,
        ActionProcessingModeBehavior actionProcessingModeBehavior, RequiredRelationship requiredRelationship, RequiredYearStatus requiredYearStatus)
    {
        if (!ModelState.IsValid)
        {
            return GetFailedResult<LeagueYearRecord>(BadRequest("Invalid request."));
        }

        if (actionProcessingModeBehavior == ActionProcessingModeBehavior.Ban)
        {
            var systemWideSettings = await _interLeagueService.GetSystemWideSettings();
            if (systemWideSettings.ActionProcessingMode)
            {
                return GetFailedResult<LeagueYearRecord>(BadRequest("Site is in read-only mode while actions process."));
            }
        }

        var currentUserRecord = await GetCurrentUser();
        if ((requiredRelationship.MustBeLoggedIn || requiredRelationship.MustBeInOrInvitedToLeague || requiredRelationship.MustBeActiveInYear || requiredRelationship.MustBeLeagueManager) && currentUserRecord.IsFailure)
        {
            return (Maybe<LeagueYearRecord>.None, Unauthorized());
        }

        var leagueYear = await _fantasyCriticService.GetLeagueYear(leagueID, year);
        if (leagueYear.HasNoValueTempoTemp)
        {
            return GetFailedResult<LeagueYearRecord>(BadRequest("League year does not exist."));
        }

        var yearStatusValid = requiredYearStatus.StateIsValid(leagueYear.ValueTempoTemp);
        if (yearStatusValid.IsFailure)
        {
            return GetFailedResult<LeagueYearRecord>(BadRequest(yearStatusValid.Error));
        }

        var playersInLeague = await _leagueMemberService.GetUsersWithRemoveStatus(leagueYear.ValueTempoTemp.League);
        var inviteesToLeague = await _leagueMemberService.GetOutstandingInvitees(leagueYear.ValueTempoTemp.League);
        var activeUsers = await _leagueMemberService.GetActivePlayersForLeagueYear(leagueYear.ValueTempoTemp.League, year);
        bool isInLeague = false;
        Maybe<LeagueInvite> leagueInvite = Maybe<LeagueInvite>.None;
        bool isActiveInYear = false;
        bool isLeagueManager = false;
        bool userIsAdmin = false;
        if (currentUserRecord.IsSuccess)
        {
            userIsAdmin = await _userManager.IsInRoleAsync(currentUserRecord.Value, "Admin");
            isLeagueManager = leagueYear.ValueTempoTemp.League.LeagueManager.Id == currentUserRecord.Value.Id;
            if (requiredRelationship.MustBeLeagueManager && !isLeagueManager)
            {
                return GetFailedResult<LeagueYearRecord>(Forbid());
            }

            if (isLeagueManager)
            {
                isInLeague = true;
                isActiveInYear = true;
            }
            else
            {
                isInLeague = playersInLeague.Any(x => x.User.Id == currentUserRecord.Value.Id);
                if (!isInLeague)
                {
                    leagueInvite = inviteesToLeague.GetMatchingInvite(currentUserRecord.Value.Email);
                }
                isActiveInYear = activeUsers.Any(x => x.Id == currentUserRecord.Value.Id);
            }
        }

        if (!isInLeague && leagueInvite.HasNoValueTempoTemp && requiredRelationship.MustBeInOrInvitedToLeague)
        {
            return GetFailedResult<LeagueYearRecord>(Forbid());
        }

        if (!isActiveInYear && requiredRelationship.MustBeActiveInYear)
        {
            return GetFailedResult<LeagueYearRecord>(Forbid());
        }

        LeagueYearUserRelationship relationship = new LeagueYearUserRelationship(leagueInvite, isInLeague, isActiveInYear, isLeagueManager, userIsAdmin);
        return (new LeagueYearRecord(currentUserRecord.ToMaybe(), leagueYear.ValueTempoTemp, playersInLeague, activeUsers, inviteesToLeague, relationship), Maybe<IActionResult>.None);
    }

    protected async Task<(Maybe<LeagueYearPublisherRecord> ValidResult, Maybe<IActionResult> FailedResult)> GetExistingLeagueYearAndPublisher(Guid leagueID, int year, Guid publisherID,
        ActionProcessingModeBehavior actionProcessingModeBehavior, RequiredRelationship requiredRelationship, RequiredYearStatus requiredYearStatus)
    {
        var leagueYearRecord = await GetExistingLeagueYear(leagueID, year, actionProcessingModeBehavior, requiredRelationship, requiredYearStatus);
        if (leagueYearRecord.FailedResult.HasValueTempoTemp)
        {
            return (Maybe<LeagueYearPublisherRecord>.None, leagueYearRecord.FailedResult);
        }

        var publisher = leagueYearRecord.ValidResult.ValueTempoTemp.LeagueYear.GetPublisherByID(publisherID);
        if (publisher.HasNoValueTempoTemp)
        {
            return GetFailedResult<LeagueYearPublisherRecord>(BadRequest("Publisher does not exist in that league."));
        }

        bool userIsPublisher = leagueYearRecord.ValidResult.ValueTempoTemp.CurrentUser.HasValueTempoTemp &&
                               leagueYearRecord.ValidResult.ValueTempoTemp.CurrentUser.ValueTempoTemp.Id == publisher.ValueTempoTemp.User.Id;
        if (requiredRelationship.MustBePublisher && !userIsPublisher)
        {
            return GetFailedResult<LeagueYearPublisherRecord>(Forbid());
        }

        var publisherRelationship = new PublisherUserRelationship(leagueYearRecord.ValidResult.ValueTempoTemp.Relationship, userIsPublisher);

        return (new LeagueYearPublisherRecord(leagueYearRecord.ValidResult.ValueTempoTemp.CurrentUser, leagueYearRecord.ValidResult.ValueTempoTemp.LeagueYear, publisher.ValueTempoTemp, publisherRelationship), Maybe<IActionResult>.None);
    }

    protected async Task<(Maybe<LeagueYearPublisherRecord> ValidResult, Maybe<IActionResult> FailedResult)> GetExistingLeagueYearAndPublisher(Guid publisherID,
        ActionProcessingModeBehavior actionProcessingModeBehavior, RequiredRelationship requiredRelationship, RequiredYearStatus requiredYearStatus)
    {
        Maybe<LeagueYearKey> leagueYearKey = await _fantasyCriticService.GetLeagueYearKeyForPublisherID(publisherID);
        if (leagueYearKey.HasNoValueTempoTemp)
        {
            return GetFailedResult<LeagueYearPublisherRecord>(BadRequest("Publisher does not exist."));
        }

        var leagueYearRecord = await GetExistingLeagueYear(leagueYearKey.ValueTempoTemp.LeagueID, leagueYearKey.ValueTempoTemp.Year, actionProcessingModeBehavior, requiredRelationship, requiredYearStatus);
        if (leagueYearRecord.FailedResult.HasValueTempoTemp)
        {
            return (Maybe<LeagueYearPublisherRecord>.None, leagueYearRecord.FailedResult);
        }

        var publisher = leagueYearRecord.ValidResult.ValueTempoTemp.LeagueYear.GetPublisherByID(publisherID);
        if (publisher.HasNoValueTempoTemp)
        {
            return GetFailedResult<LeagueYearPublisherRecord>(BadRequest("Publisher does not exist in that league."));
        }

        bool userIsPublisher = leagueYearRecord.ValidResult.ValueTempoTemp.CurrentUser.HasValueTempoTemp &&
                               leagueYearRecord.ValidResult.ValueTempoTemp.CurrentUser.ValueTempoTemp.Id == publisher.ValueTempoTemp.User.Id;
        if (requiredRelationship.MustBePublisher && !userIsPublisher)
        {
            return GetFailedResult<LeagueYearPublisherRecord>(Forbid());
        }

        var publisherRelationship = new PublisherUserRelationship(leagueYearRecord.ValidResult.ValueTempoTemp.Relationship, userIsPublisher);

        return (new LeagueYearPublisherRecord(leagueYearRecord.ValidResult.ValueTempoTemp.CurrentUser, leagueYearRecord.ValidResult.ValueTempoTemp.LeagueYear, publisher.ValueTempoTemp, publisherRelationship), Maybe<IActionResult>.None);
    }

    protected async Task<(Maybe<LeagueYearPublisherRecord> LeagueYearPublisherRecord, Maybe<IActionResult> FailedResult)> GetExistingLeagueYearAndPublisher(Guid leagueID, int year, FantasyCriticUser userForPublisher,
        ActionProcessingModeBehavior actionProcessingModeBehavior, RequiredRelationship requiredRelationship, RequiredYearStatus requiredYearStatus)
    {
        var leagueYearRecord = await GetExistingLeagueYear(leagueID, year, actionProcessingModeBehavior, requiredRelationship, requiredYearStatus);
        if (leagueYearRecord.FailedResult.HasValueTempoTemp)
        {
            return (Maybe<LeagueYearPublisherRecord>.None, leagueYearRecord.FailedResult);
        }

        var publisher = leagueYearRecord.ValidResult.ValueTempoTemp.LeagueYear.GetUserPublisher(userForPublisher);
        if (publisher.HasNoValueTempoTemp)
        {
            return GetFailedResult<LeagueYearPublisherRecord>(BadRequest("That user does not have a publisher in that league."));
        }

        bool userIsPublisher = leagueYearRecord.ValidResult.ValueTempoTemp.CurrentUser.HasValueTempoTemp &&
                               leagueYearRecord.ValidResult.ValueTempoTemp.CurrentUser.ValueTempoTemp.Id == publisher.ValueTempoTemp.User.Id;
        if (requiredRelationship.MustBePublisher && !userIsPublisher)
        {
            return GetFailedResult<LeagueYearPublisherRecord>(Forbid());
        }

        var publisherRelationship = new PublisherUserRelationship(leagueYearRecord.ValidResult.ValueTempoTemp.Relationship, userIsPublisher);

        return (new LeagueYearPublisherRecord(leagueYearRecord.ValidResult.ValueTempoTemp.CurrentUser, leagueYearRecord.ValidResult.ValueTempoTemp.LeagueYear, publisher.ValueTempoTemp, publisherRelationship), Maybe<IActionResult>.None);
    }

    protected async Task<(Maybe<LeagueYearPublisherGameRecord> ValidResult, Maybe<IActionResult> FailedResult)> GetExistingLeagueYearAndPublisherGame(Guid leagueID, int year, Guid publisherID, Guid publisherGameID,
        ActionProcessingModeBehavior actionProcessingModeBehavior, RequiredRelationship requiredRelationship, RequiredYearStatus requiredYearStatus)
    {
        var leagueYearPublisherRecord = await GetExistingLeagueYearAndPublisher(leagueID, year, publisherID, actionProcessingModeBehavior, requiredRelationship, requiredYearStatus);
        if (leagueYearPublisherRecord.FailedResult.HasValueTempoTemp)
        {
            return (Maybe<LeagueYearPublisherGameRecord>.None, leagueYearPublisherRecord.FailedResult);
        }

        var publisherGame = leagueYearPublisherRecord.ValidResult.ValueTempoTemp.Publisher.PublisherGames.SingleOrDefault(x => x.PublisherGameID == publisherGameID);
        if (publisherGame is null)
        {
            return GetFailedResult<LeagueYearPublisherGameRecord>(BadRequest("That publisher game does not exist."));
        }

        return (new LeagueYearPublisherGameRecord(leagueYearPublisherRecord.ValidResult.ValueTempoTemp.CurrentUser, leagueYearPublisherRecord.ValidResult.ValueTempoTemp.LeagueYear,
            leagueYearPublisherRecord.ValidResult.ValueTempoTemp.Publisher, publisherGame, leagueYearPublisherRecord.ValidResult.ValueTempoTemp.Relationship), Maybe<IActionResult>.None);
    }

    protected async Task<(Maybe<LeagueYearPublisherGameRecord> ValidResult, Maybe<IActionResult> FailedResult)> GetExistingLeagueYearAndPublisherGame(Guid publisherID, Guid publisherGameID,
        ActionProcessingModeBehavior actionProcessingModeBehavior, RequiredRelationship requiredRelationship, RequiredYearStatus requiredYearStatus)
    {
        Maybe<LeagueYearKey> leagueYearKey = await _fantasyCriticService.GetLeagueYearKeyForPublisherID(publisherID);
        if (leagueYearKey.HasNoValueTempoTemp)
        {
            return GetFailedResult<LeagueYearPublisherGameRecord>(BadRequest("Publisher does not exist."));
        }

        var leagueYearPublisherRecord = await GetExistingLeagueYearAndPublisher(leagueYearKey.ValueTempoTemp.LeagueID, leagueYearKey.ValueTempoTemp.Year, publisherID, actionProcessingModeBehavior, requiredRelationship, requiredYearStatus);
        if (leagueYearPublisherRecord.FailedResult.HasValueTempoTemp)
        {
            return (Maybe<LeagueYearPublisherGameRecord>.None, leagueYearPublisherRecord.FailedResult);
        }

        var publisherGame = leagueYearPublisherRecord.ValidResult.ValueTempoTemp.Publisher.PublisherGames.SingleOrDefault(x => x.PublisherGameID == publisherGameID);
        if (publisherGame is null)
        {
            return GetFailedResult<LeagueYearPublisherGameRecord>(BadRequest("That publisher game does not exist."));
        }

        return (new LeagueYearPublisherGameRecord(leagueYearPublisherRecord.ValidResult.ValueTempoTemp.CurrentUser, leagueYearPublisherRecord.ValidResult.ValueTempoTemp.LeagueYear,
            leagueYearPublisherRecord.ValidResult.ValueTempoTemp.Publisher, publisherGame, leagueYearPublisherRecord.ValidResult.ValueTempoTemp.Relationship), Maybe<IActionResult>.None);
    }
}
