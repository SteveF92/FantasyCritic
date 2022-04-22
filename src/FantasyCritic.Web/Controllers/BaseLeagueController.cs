using FantasyCritic.Lib.Extensions;
using FantasyCritic.Lib.Identity;
using FantasyCritic.Lib.Services;
using FantasyCritic.Web.Helpers;

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

    protected async Task<GenericResultRecord<LeagueRecord>> GetExistingLeague(Guid leagueID, RequiredRelationship requiredRelationship)
    {
        var currentUserRecord = await GetCurrentUser();
        if ((requiredRelationship.MustBeLoggedIn || requiredRelationship.MustBeInOrInvitedToLeague || requiredRelationship.MustBeLeagueManager) && currentUserRecord.IsFailure)
        {
            return GetFailedResult<LeagueRecord>(Unauthorized());
        }

        var league = await _fantasyCriticService.GetLeagueByID(leagueID);
        if (league is null)
        {
            return GetFailedResult<LeagueRecord>(BadRequest("League does not exist."));
        }

        var playersInLeague = await _leagueMemberService.GetUsersWithRemoveStatus(league);
        bool isInLeague = false;
        LeagueInvite? leagueInvite = null;
        bool isLeagueManager = false;
        bool userIsAdmin = false;
        if (currentUserRecord.IsSuccess)
        {
            userIsAdmin = await _userManager.IsInRoleAsync(currentUserRecord.Value, "Admin");
            isLeagueManager = league.LeagueManager.Id == currentUserRecord.Value.Id;
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
                    var inviteesToLeague = await _leagueMemberService.GetOutstandingInvitees(league);
                    leagueInvite = inviteesToLeague.GetMatchingInvite(currentUserRecord.Value.Email);
                }
            }
        }

        if (!isInLeague && leagueInvite is null && requiredRelationship.MustBeInOrInvitedToLeague)
        {
            return UnauthorizedOrForbid<LeagueRecord>(currentUserRecord.IsSuccess);
        }

        LeagueUserRelationship relationship = new LeagueUserRelationship(leagueInvite, isInLeague, isLeagueManager, userIsAdmin);
        return new GenericResultRecord<LeagueRecord>(new LeagueRecord(currentUserRecord.ToNullable(), league, playersInLeague, relationship), null);
    }

    protected async Task<GenericResultRecord<LeagueYearRecord>> GetExistingLeagueYear(Guid leagueID, int year,
        ActionProcessingModeBehavior actionProcessingModeBehavior, RequiredRelationship requiredRelationship, RequiredYearStatus requiredYearStatus)
    {
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
            return GetFailedResult<LeagueYearRecord>(Unauthorized());
        }

        var leagueYear = await _fantasyCriticService.GetLeagueYear(leagueID, year);
        if (leagueYear is null)
        {
            return GetFailedResult<LeagueYearRecord>(BadRequest("League year does not exist."));
        }

        var yearStatusValid = requiredYearStatus.StateIsValid(leagueYear);
        if (yearStatusValid.IsFailure)
        {
            return GetFailedResult<LeagueYearRecord>(BadRequest(yearStatusValid.Error));
        }

        var playersInLeague = await _leagueMemberService.GetUsersWithRemoveStatus(leagueYear.League);
        var inviteesToLeague = await _leagueMemberService.GetOutstandingInvitees(leagueYear.League);
        var activeUsers = await _leagueMemberService.GetActivePlayersForLeagueYear(leagueYear.League, year);
        bool isInLeague = false;
        LeagueInvite? leagueInvite = null;
        bool isActiveInYear = false;
        bool isLeagueManager = false;
        bool userIsAdmin = false;
        if (currentUserRecord.IsSuccess)
        {
            userIsAdmin = await _userManager.IsInRoleAsync(currentUserRecord.Value, "Admin");
            isLeagueManager = leagueYear.League.LeagueManager.Id == currentUserRecord.Value.Id;
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

        if (!isInLeague && leagueInvite is null && requiredRelationship.MustBeInOrInvitedToLeague)
        {
            return UnauthorizedOrForbid<LeagueYearRecord>(currentUserRecord.IsSuccess);
        }

        if (!isActiveInYear && requiredRelationship.MustBeActiveInYear)
        {
            return UnauthorizedOrForbid<LeagueYearRecord>(currentUserRecord.IsSuccess);
        }

        LeagueYearUserRelationship relationship = new LeagueYearUserRelationship(leagueInvite, isInLeague, isActiveInYear, isLeagueManager, userIsAdmin);
        return new GenericResultRecord<LeagueYearRecord>(new LeagueYearRecord(currentUserRecord.ToNullable(), leagueYear, playersInLeague, activeUsers, inviteesToLeague, relationship), null);
    }

    protected async Task<GenericResultRecord<LeagueYearPublisherRecord>> GetExistingLeagueYearAndPublisher(Guid leagueID, int year, Guid publisherID,
        ActionProcessingModeBehavior actionProcessingModeBehavior, RequiredRelationship requiredRelationship, RequiredYearStatus requiredYearStatus)
    {
        var leagueYearRecord = await GetExistingLeagueYear(leagueID, year, actionProcessingModeBehavior, requiredRelationship, requiredYearStatus);
        if (leagueYearRecord.FailedResult is not null)
        {
            return GetFailedResult<LeagueYearPublisherRecord>(leagueYearRecord.FailedResult);
        }

        var publisher = leagueYearRecord.ValidResult!.LeagueYear.GetPublisherByID(publisherID);
        if (publisher is null)
        {
            return GetFailedResult<LeagueYearPublisherRecord>(BadRequest("Publisher does not exist in that league."));
        }

        bool userIsPublisher = leagueYearRecord.ValidResult.CurrentUser is not null &&
                               leagueYearRecord.ValidResult.CurrentUser.Id == publisher.User.Id;
        if (requiredRelationship.MustBePublisher && !userIsPublisher)
        {
            return UnauthorizedOrForbid<LeagueYearPublisherRecord>(leagueYearRecord.ValidResult.CurrentUser is not null);
        }

        var publisherRelationship = new PublisherUserRelationship(leagueYearRecord.ValidResult.Relationship, userIsPublisher);

        return new GenericResultRecord<LeagueYearPublisherRecord>(new LeagueYearPublisherRecord(leagueYearRecord.ValidResult.CurrentUser, leagueYearRecord.ValidResult.LeagueYear, publisher, publisherRelationship), null);
    }

    protected async Task<GenericResultRecord<LeagueYearPublisherRecord>> GetExistingLeagueYearAndPublisher(Guid publisherID,
        ActionProcessingModeBehavior actionProcessingModeBehavior, RequiredRelationship requiredRelationship, RequiredYearStatus requiredYearStatus)
    {
        LeagueYearKey? leagueYearKey = await _fantasyCriticService.GetLeagueYearKeyForPublisherID(publisherID);
        if (leagueYearKey is null)
        {
            return GetFailedResult<LeagueYearPublisherRecord>(BadRequest("Publisher does not exist."));
        }

        var leagueYearRecord = await GetExistingLeagueYear(leagueYearKey.LeagueID, leagueYearKey.Year, actionProcessingModeBehavior, requiredRelationship, requiredYearStatus);
        if (leagueYearRecord.FailedResult is not null)
        {
            return GetFailedResult<LeagueYearPublisherRecord>(leagueYearRecord.FailedResult);
        }

        var publisher = leagueYearRecord.ValidResult!.LeagueYear.GetPublisherByID(publisherID);
        if (publisher is null)
        {
            return GetFailedResult<LeagueYearPublisherRecord>(BadRequest("Publisher does not exist in that league."));
        }

        bool userIsPublisher = leagueYearRecord.ValidResult.CurrentUser is not null &&
                               leagueYearRecord.ValidResult.CurrentUser.Id == publisher.User.Id;
        if (requiredRelationship.MustBePublisher && !userIsPublisher)
        {
            return GetFailedResult<LeagueYearPublisherRecord>(Forbid());
        }

        var publisherRelationship = new PublisherUserRelationship(leagueYearRecord.ValidResult.Relationship, userIsPublisher);

        return new GenericResultRecord<LeagueYearPublisherRecord>(new LeagueYearPublisherRecord(leagueYearRecord.ValidResult.CurrentUser, leagueYearRecord.ValidResult.LeagueYear, publisher, publisherRelationship), null);
    }

    protected async Task<GenericResultRecord<LeagueYearPublisherRecord>> GetExistingLeagueYearAndPublisher(Guid leagueID, int year, FantasyCriticUser userForPublisher,
        ActionProcessingModeBehavior actionProcessingModeBehavior, RequiredRelationship requiredRelationship, RequiredYearStatus requiredYearStatus)
    {
        var leagueYearRecord = await GetExistingLeagueYear(leagueID, year, actionProcessingModeBehavior, requiredRelationship, requiredYearStatus);
        if (leagueYearRecord.FailedResult is not null)
        {
            return GetFailedResult<LeagueYearPublisherRecord>(leagueYearRecord.FailedResult);
        }

        var publisher = leagueYearRecord.ValidResult!.LeagueYear.GetUserPublisher(userForPublisher);
        if (publisher is null)
        {
            return GetFailedResult<LeagueYearPublisherRecord>(BadRequest("That user does not have a publisher in that league."));
        }

        bool userIsPublisher = leagueYearRecord.ValidResult.CurrentUser is not null &&
                               leagueYearRecord.ValidResult.CurrentUser.Id == publisher.User.Id;
        if (requiredRelationship.MustBePublisher && !userIsPublisher)
        {
            return GetFailedResult<LeagueYearPublisherRecord>(Forbid());
        }

        var publisherRelationship = new PublisherUserRelationship(leagueYearRecord.ValidResult.Relationship, userIsPublisher);

        return new GenericResultRecord<LeagueYearPublisherRecord>(new LeagueYearPublisherRecord(leagueYearRecord.ValidResult.CurrentUser, leagueYearRecord.ValidResult.LeagueYear, publisher, publisherRelationship), null);
    }

    protected async Task<GenericResultRecord<LeagueYearPublisherGameRecord>> GetExistingLeagueYearAndPublisherGame(Guid leagueID, int year, Guid publisherID, Guid publisherGameID,
        ActionProcessingModeBehavior actionProcessingModeBehavior, RequiredRelationship requiredRelationship, RequiredYearStatus requiredYearStatus)
    {
        var leagueYearPublisherRecord = await GetExistingLeagueYearAndPublisher(leagueID, year, publisherID, actionProcessingModeBehavior, requiredRelationship, requiredYearStatus);
        if (leagueYearPublisherRecord.FailedResult is not null)
        {
            return GetFailedResult<LeagueYearPublisherGameRecord>(leagueYearPublisherRecord.FailedResult);
        }

        var publisherGame = leagueYearPublisherRecord.ValidResult!.Publisher.PublisherGames.SingleOrDefault(x => x.PublisherGameID == publisherGameID);
        if (publisherGame is null)
        {
            return GetFailedResult<LeagueYearPublisherGameRecord>(BadRequest("That publisher game does not exist."));
        }

        return new GenericResultRecord<LeagueYearPublisherGameRecord>(new LeagueYearPublisherGameRecord(leagueYearPublisherRecord.ValidResult.CurrentUser, leagueYearPublisherRecord.ValidResult.LeagueYear,
            leagueYearPublisherRecord.ValidResult.Publisher, publisherGame, leagueYearPublisherRecord.ValidResult.Relationship), null);
    }

    protected async Task<GenericResultRecord<LeagueYearPublisherGameRecord>> GetExistingLeagueYearAndPublisherGame(Guid publisherID, Guid publisherGameID,
        ActionProcessingModeBehavior actionProcessingModeBehavior, RequiredRelationship requiredRelationship, RequiredYearStatus requiredYearStatus)
    {
        LeagueYearKey? leagueYearKey = await _fantasyCriticService.GetLeagueYearKeyForPublisherID(publisherID);
        if (leagueYearKey is null)
        {
            return GetFailedResult<LeagueYearPublisherGameRecord>(BadRequest("Publisher does not exist."));
        }

        var leagueYearPublisherRecord = await GetExistingLeagueYearAndPublisher(leagueYearKey.LeagueID, leagueYearKey.Year, publisherID, actionProcessingModeBehavior, requiredRelationship, requiredYearStatus);
        if (leagueYearPublisherRecord.FailedResult is not null)
        {
            return GetFailedResult<LeagueYearPublisherGameRecord>(leagueYearPublisherRecord.FailedResult);
        }

        var publisherGame = leagueYearPublisherRecord.ValidResult!.Publisher.PublisherGames.SingleOrDefault(x => x.PublisherGameID == publisherGameID);
        if (publisherGame is null)
        {
            return GetFailedResult<LeagueYearPublisherGameRecord>(BadRequest("That publisher game does not exist."));
        }

        return new GenericResultRecord<LeagueYearPublisherGameRecord>(new LeagueYearPublisherGameRecord(
            leagueYearPublisherRecord.ValidResult.CurrentUser, leagueYearPublisherRecord.ValidResult.LeagueYear,
            leagueYearPublisherRecord.ValidResult.Publisher, publisherGame,
            leagueYearPublisherRecord.ValidResult.Relationship), null);
    }
}
