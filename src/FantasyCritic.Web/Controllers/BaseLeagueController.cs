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

    protected async Task<(Maybe<LeagueRecord> ValidResult, Maybe<IActionResult> FailedResult)> GetExistingLeague(Guid leagueID,
        bool failIfActionProcessing, RequiredRelationship requiredRelationship)
    {
        if (failIfActionProcessing)
        {
            var systemWideSettings = await _interLeagueService.GetSystemWideSettings();
            if (systemWideSettings.ActionProcessingMode)
            {
                return GetFailedResult<LeagueRecord>(BadRequest("Site is in read-only mode while actions process."));
            }
        }

        var currentUserRecord = await GetCurrentUser();
        if ((requiredRelationship.MustBeLoggedIn || requiredRelationship.MustBeInOrInvitedToLeague || requiredRelationship.MustBeLeagueManager) && currentUserRecord.IsFailure)
        {
            return (Maybe<LeagueRecord>.None, Forbid("You must be logged in."));
        }

        var league = await _fantasyCriticService.GetLeagueByID(leagueID);
        if (league.HasNoValue)
        {
            return GetFailedResult<LeagueRecord>(BadRequest("League does not exist."));
        }

        var playersInLeague = await _leagueMemberService.GetUsersWithRemoveStatus(league.Value);
        bool isInLeague = false;
        Maybe<LeagueInvite> leagueInvite = Maybe<LeagueInvite>.None;
        bool isLeagueManager = false;
        if (currentUserRecord.IsSuccess)
        {
            isLeagueManager = league.Value.LeagueManager.Id == currentUserRecord.Value.Id;
            if (requiredRelationship.MustBeLeagueManager && !isLeagueManager)
            {
                return GetFailedResult<LeagueRecord>(Forbid("You are not the manager of that league."));
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
                    var inviteesToLeague = await _leagueMemberService.GetOutstandingInvitees(league.Value);
                    leagueInvite = inviteesToLeague.GetMatchingInvite(currentUserRecord.Value.Email);
                }
            }
        }

        bool userIsAdmin = await _userManager.IsInRoleAsync(currentUserRecord.Value, "Admin");
        if (!isInLeague && leagueInvite.HasNoValue && requiredRelationship.MustBeInOrInvitedToLeague)
        {
            if (!requiredRelationship.AllowIfAdmin || !userIsAdmin)
            {
                return GetFailedResult<LeagueRecord>(Forbid("You are not in that league."));
            }
        }

        LeagueUserRelationship relationship = new LeagueUserRelationship(leagueInvite, isInLeague, isLeagueManager, userIsAdmin);
        return (new LeagueRecord(currentUserRecord.ToMaybe(), league.Value, playersInLeague, relationship), Maybe<IActionResult>.None);
    }

    protected async Task<(Maybe<LeagueYearRecord> ValidResult, Maybe<IActionResult> FailedResult)> GetExistingLeagueYear(Guid leagueID, int year,
        bool failIfActionProcessing, RequiredRelationship requiredRelationship)
    {
        if (failIfActionProcessing)
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
            return (Maybe<LeagueYearRecord>.None, Forbid("You must be logged in."));
        }

        var leagueYear = await _fantasyCriticService.GetLeagueYear(leagueID, year);
        if (leagueYear.HasNoValue)
        {
            return GetFailedResult<LeagueYearRecord>(BadRequest("League year does not exist."));
        }

        var inviteesToLeague = await _leagueMemberService.GetOutstandingInvitees(leagueYear.Value.League);
        var activeUsers = await _leagueMemberService.GetActivePlayersForLeagueYear(leagueYear.Value.League, year);
        bool isInLeague = false;
        bool isInvitedToLeague = false;
        bool isActiveInYear = false;
        bool isLeagueManager = false;
        if (currentUserRecord.IsSuccess)
        {
            isLeagueManager = leagueYear.Value.League.LeagueManager.Id == currentUserRecord.Value.Id;
            if (requiredRelationship.MustBeLeagueManager && !isLeagueManager)
            {
                return GetFailedResult<LeagueYearRecord>(Forbid("You are not the manager of that league."));
            }

            if (isLeagueManager)
            {
                isInLeague = true;
                isActiveInYear = true;
            }
            else
            {
                var playersInLeague = await _leagueMemberService.GetUsersWithRemoveStatus(leagueYear.Value.League);
                isInLeague = playersInLeague.Any(x => x.User.Id == currentUserRecord.Value.Id);
                if (!isInLeague)
                {
                    isInvitedToLeague = inviteesToLeague.UserIsInvited(currentUserRecord.Value.Email);
                }
                isActiveInYear = activeUsers.Any(x => x.Id == currentUserRecord.Value.Id);
            }
        }

        bool userIsAdmin = await _userManager.IsInRoleAsync(currentUserRecord.Value, "Admin");
        if (!isInLeague && !isInvitedToLeague && requiredRelationship.MustBeInOrInvitedToLeague)
        {
            if (!requiredRelationship.AllowIfAdmin || !userIsAdmin)
            {
                return GetFailedResult<LeagueYearRecord>(Forbid("You are not in that league."));
            }
        }

        if (!isActiveInYear && requiredRelationship.MustBeActiveInYear)
        {
            return GetFailedResult<LeagueYearRecord>(Forbid("You are set as active in that league year."));
        }

        LeagueYearUserRelationship relationship = new LeagueYearUserRelationship(isInvitedToLeague, isInLeague, isActiveInYear, isLeagueManager, userIsAdmin);
        return (new LeagueYearRecord(currentUserRecord.ToMaybe(), leagueYear.Value, activeUsers, inviteesToLeague, relationship), Maybe<IActionResult>.None);
    }

    protected async Task<(Maybe<LeagueYearPublisherRecord> ValidResult, Maybe<IActionResult> FailedResult)> GetExistingLeagueYearAndPublisher(Guid leagueID, int year, Guid publisherID,
        bool failIfActionProcessing, RequiredRelationship requiredRelationship)
    {
        var leagueYearRecord = await GetExistingLeagueYear(leagueID, year, failIfActionProcessing, requiredRelationship);
        if (leagueYearRecord.FailedResult.HasValue)
        {
            return (Maybe<LeagueYearPublisherRecord>.None, leagueYearRecord.FailedResult);
        }

        var publisher = leagueYearRecord.ValidResult.Value.LeagueYear.GetPublisherByID(publisherID);
        if (publisher.HasNoValue)
        {
            return GetFailedResult<LeagueYearPublisherRecord>(BadRequest("Publisher does not exist in that league."));
        }

        bool userIsPublisher = leagueYearRecord.ValidResult.Value.CurrentUser.HasValue &&
                               leagueYearRecord.ValidResult.Value.CurrentUser.Value.Id == publisher.Value.User.Id;
        if (requiredRelationship.MustBePublisher)
        {
            return GetFailedResult<LeagueYearPublisherRecord>(BadRequest("You are not that publisher."));
        }

        var publisherRelationship = new PublisherUserRelationship(leagueYearRecord.ValidResult.Value.Relationship, userIsPublisher);

        return (new LeagueYearPublisherRecord(leagueYearRecord.ValidResult.Value.CurrentUser, leagueYearRecord.ValidResult.Value.LeagueYear, publisher.Value, publisherRelationship), Maybe<IActionResult>.None);
    }

    protected async Task<(Maybe<LeagueYearPublisherRecord> LeagueYearPublisherRecord, Maybe<IActionResult> FailedResult)> GetExistingLeagueYearAndPublisher(Guid leagueID, int year, FantasyCriticUser userForPublisher,
        bool failIfActionProcessing, RequiredRelationship requiredRelationship)
    {
        var leagueYearRecord = await GetExistingLeagueYear(leagueID, year, failIfActionProcessing, requiredRelationship);
        if (leagueYearRecord.FailedResult.HasValue)
        {
            return (Maybe<LeagueYearPublisherRecord>.None, leagueYearRecord.FailedResult);
        }

        var publisher = leagueYearRecord.ValidResult.Value.LeagueYear.GetUserPublisher(userForPublisher);
        if (publisher.HasNoValue)
        {
            return GetFailedResult<LeagueYearPublisherRecord>(BadRequest("That user does not have a publisher in that league."));
        }

        bool userIsPublisher = leagueYearRecord.ValidResult.Value.CurrentUser.HasValue &&
                               leagueYearRecord.ValidResult.Value.CurrentUser.Value.Id == publisher.Value.User.Id;
        if (requiredRelationship.MustBePublisher)
        {
            return GetFailedResult<LeagueYearPublisherRecord>(BadRequest("You are not that publisher."));
        }

        var publisherRelationship = new PublisherUserRelationship(leagueYearRecord.ValidResult.Value.Relationship, userIsPublisher);

        return (new LeagueYearPublisherRecord(leagueYearRecord.ValidResult.Value.CurrentUser, leagueYearRecord.ValidResult.Value.LeagueYear, publisher.Value, publisherRelationship), Maybe<IActionResult>.None);
    }

    protected async Task<(Maybe<LeagueYearPublisherGameRecord> ValidResult, Maybe<IActionResult> FailedResult)> GetExistingLeagueYearAndPublisherGame(Guid leagueID, int year, Guid publisherID, Guid publisherGameID,
        bool failIfActionProcessing, RequiredRelationship requiredRelationship)
    {
        var leagueYearPublisherRecord = await GetExistingLeagueYearAndPublisher(leagueID, year, publisherID, failIfActionProcessing, requiredRelationship);
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
            leagueYearPublisherRecord.ValidResult.Value.Publisher, publisherGame, leagueYearPublisherRecord.ValidResult.Value.Relationship), Maybe<IActionResult>.None);
    }
}
