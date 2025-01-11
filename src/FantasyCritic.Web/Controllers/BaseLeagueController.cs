using FantasyCritic.Lib.Discord;
using FantasyCritic.Lib.Domain.Combinations;
using FantasyCritic.Lib.Domain.Draft;
using FantasyCritic.Lib.Extensions;
using FantasyCritic.Lib.Identity;
using FantasyCritic.Lib.Services;
using FantasyCritic.Web.Helpers;
using Microsoft.AspNetCore.SignalR;
using FantasyCritic.Web.Hubs;

namespace FantasyCritic.Web.Controllers;
public abstract class BaseLeagueController : FantasyCriticController
{
    protected readonly FantasyCriticService _fantasyCriticService;
    protected readonly InterLeagueService _interLeagueService;
    protected readonly LeagueMemberService _leagueMemberService;
    protected readonly ConferenceService _conferenceService;
    private readonly DiscordPushService _discordPushService;
    private readonly IHubContext<UpdateHub> _hubContext;

    protected BaseLeagueController(FantasyCriticUserManager userManager, FantasyCriticService fantasyCriticService, InterLeagueService interLeagueService,
        LeagueMemberService leagueMemberService, ConferenceService conferenceService, DiscordPushService discordPushService, IHubContext<UpdateHub> hubContext)
        : base(userManager)
    {
        _fantasyCriticService = fantasyCriticService;
        _interLeagueService = interLeagueService;
        _leagueMemberService = leagueMemberService;
        _conferenceService = conferenceService;
        _discordPushService = discordPushService;
        _hubContext = hubContext;
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
            isLeagueManager = league.LeagueManager.UserID == currentUserRecord.Value.Id;
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

        var leagueYearWithUserStatus = await _fantasyCriticService.GetLeagueYearWithUserStatus(leagueID, year);
        if (leagueYearWithUserStatus is null)
        {
            return GetFailedResult<LeagueYearRecord>(BadRequest("League year does not exist."));
        }

        var leagueYear = leagueYearWithUserStatus.LeagueYear;
        var combinedLeagueUserStatus = leagueYearWithUserStatus.UserStatus;

        var yearStatusValid = requiredYearStatus.StateIsValid(leagueYear);
        if (yearStatusValid.IsFailure)
        {
            return GetFailedResult<LeagueYearRecord>(BadRequest(yearStatusValid.Error));
        }

        bool isInLeague = false;
        LeagueInvite? leagueInvite = null;
        bool isActiveInYear = false;
        bool isLeagueManager = false;
        bool userIsAdmin = false;
        if (currentUserRecord.IsSuccess)
        {
            userIsAdmin = await _userManager.IsInRoleAsync(currentUserRecord.Value, "Admin");
            isLeagueManager = leagueYear.League.LeagueManager.UserID == currentUserRecord.Value.Id;
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
                isInLeague = combinedLeagueUserStatus.UsersWithRemoveStatus.Any(x => x.User.Id == currentUserRecord.Value.Id);
                if (!isInLeague)
                {
                    leagueInvite = combinedLeagueUserStatus.OutstandingInvites.GetMatchingInvite(currentUserRecord.Value.Email);
                }
                isActiveInYear = combinedLeagueUserStatus.ActivePlayersForLeagueYear.Any(x => x.Id == currentUserRecord.Value.Id);
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
        return new GenericResultRecord<LeagueYearRecord>(new LeagueYearRecord(currentUserRecord.ToNullable(), leagueYear,
            combinedLeagueUserStatus.UsersWithRemoveStatus, combinedLeagueUserStatus.ActivePlayersForLeagueYear, combinedLeagueUserStatus.OutstandingInvites, relationship), null);
    }

    protected async Task<GenericResultRecord<LeagueYearWithSupplementalDataRecord>> GetExistingLeagueYearWithSupplementalData(Guid leagueID, int year,
        ActionProcessingModeBehavior actionProcessingModeBehavior, RequiredRelationship requiredRelationship, RequiredYearStatus requiredYearStatus)
    {
        if (actionProcessingModeBehavior == ActionProcessingModeBehavior.Ban)
        {
            var systemWideSettings = await _interLeagueService.GetSystemWideSettings();
            if (systemWideSettings.ActionProcessingMode)
            {
                return GetFailedResult<LeagueYearWithSupplementalDataRecord>(BadRequest("Site is in read-only mode while actions process."));
            }
        }

        var currentUserRecord = await GetCurrentUser();
        if ((requiredRelationship.MustBeLoggedIn || requiredRelationship.MustBeInOrInvitedToLeague || requiredRelationship.MustBeActiveInYear || requiredRelationship.MustBeLeagueManager) && currentUserRecord.IsFailure)
        {
            return GetFailedResult<LeagueYearWithSupplementalDataRecord>(Unauthorized());
        }

        var leagueYearWithSupplementalData = await _fantasyCriticService.GetLeagueYearWithSupplementalData(leagueID, year, currentUserRecord.ToNullable());
        if (leagueYearWithSupplementalData is null)
        {
            return GetFailedResult<LeagueYearWithSupplementalDataRecord>(BadRequest("League year does not exist."));
        }

        var leagueYear = leagueYearWithSupplementalData.LeagueYear;
        var combinedLeagueUserStatus = leagueYearWithSupplementalData.UserStatus;

        var yearStatusValid = requiredYearStatus.StateIsValid(leagueYear);
        if (yearStatusValid.IsFailure)
        {
            return GetFailedResult<LeagueYearWithSupplementalDataRecord>(BadRequest(yearStatusValid.Error));
        }

        bool isInLeague = false;
        LeagueInvite? leagueInvite = null;
        bool isActiveInYear = false;
        bool isLeagueManager = false;
        bool userIsAdmin = false;
        if (currentUserRecord.IsSuccess)
        {
            userIsAdmin = await _userManager.IsInRoleAsync(currentUserRecord.Value, "Admin");
            isLeagueManager = leagueYear.League.LeagueManager.UserID == currentUserRecord.Value.Id;
            if (requiredRelationship.MustBeLeagueManager && !isLeagueManager)
            {
                return GetFailedResult<LeagueYearWithSupplementalDataRecord>(Forbid());
            }

            if (isLeagueManager)
            {
                isInLeague = true;
                isActiveInYear = true;
            }
            else
            {
                isInLeague = combinedLeagueUserStatus.UsersWithRemoveStatus.Any(x => x.User.Id == currentUserRecord.Value.Id);
                if (!isInLeague)
                {
                    leagueInvite = combinedLeagueUserStatus.OutstandingInvites.GetMatchingInvite(currentUserRecord.Value.Email);
                }
                isActiveInYear = combinedLeagueUserStatus.ActivePlayersForLeagueYear.Any(x => x.Id == currentUserRecord.Value.Id);
            }
        }

        if (!isInLeague && leagueInvite is null && requiredRelationship.MustBeInOrInvitedToLeague)
        {
            return UnauthorizedOrForbid<LeagueYearWithSupplementalDataRecord>(currentUserRecord.IsSuccess);
        }

        if (!isActiveInYear && requiredRelationship.MustBeActiveInYear)
        {
            return UnauthorizedOrForbid<LeagueYearWithSupplementalDataRecord>(currentUserRecord.IsSuccess);
        }

        LeagueYearUserRelationship relationship = new LeagueYearUserRelationship(leagueInvite, isInLeague, isActiveInYear, isLeagueManager, userIsAdmin);
        return new GenericResultRecord<LeagueYearWithSupplementalDataRecord>(new LeagueYearWithSupplementalDataRecord(currentUserRecord.ToNullable(), leagueYear, leagueYearWithSupplementalData.SupplementalData,
            combinedLeagueUserStatus.UsersWithRemoveStatus, combinedLeagueUserStatus.ActivePlayersForLeagueYear, combinedLeagueUserStatus.OutstandingInvites, relationship), null);
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

    protected async Task<GenericResultRecord<ConferenceRecord>> GetExistingConference(Guid conferenceID, ConferenceRequiredRelationship requiredRelationship)
    {
        var currentUserRecord = await GetCurrentUser();
        if ((requiredRelationship.MustBeLoggedIn || requiredRelationship.MustBeInConference || requiredRelationship.MustBeConferenceManager) && currentUserRecord.IsFailure)
        {
            return GetFailedResult<ConferenceRecord>(Unauthorized());
        }

        var conference = await _conferenceService.GetConference(conferenceID);
        if (conference is null)
        {
            return GetFailedResult<ConferenceRecord>(BadRequest("Conference does not exist."));
        }

        var playersInConference = await _conferenceService.GetPlayersInConference(conference);
        bool isInConference = false;
        bool isConferenceManager = false;
        bool userIsAdmin = false;
        if (currentUserRecord.IsSuccess)
        {
            userIsAdmin = await _userManager.IsInRoleAsync(currentUserRecord.Value, "Admin");
            isConferenceManager = conference.ConferenceManager.UserID == currentUserRecord.Value.Id;
            if (requiredRelationship.MustBeConferenceManager && !isConferenceManager)
            {
                return GetFailedResult<ConferenceRecord>(Forbid());
            }

            isInConference = isConferenceManager || playersInConference.Any(x => x.User.UserID == currentUserRecord.Value.Id);
        }

        if (!isInConference && requiredRelationship.MustBeInConference)
        {
            return UnauthorizedOrForbid<ConferenceRecord>(currentUserRecord.IsSuccess);
        }

        var conferenceLeagues = await _conferenceService.GetLeaguesInConference(conference);
        ConferenceUserRelationship relationship = new ConferenceUserRelationship(isInConference, isConferenceManager, userIsAdmin);
        return new GenericResultRecord<ConferenceRecord>(new ConferenceRecord(currentUserRecord.ToNullable(), conference, playersInConference, relationship, conferenceLeagues), null);
    }

    protected async Task<GenericResultRecord<ConferenceYearRecord>> GetExistingConferenceYear(Guid conferenceID, int year, ConferenceRequiredRelationship requiredRelationship)
    {
        var currentUserRecord = await GetCurrentUser();
        if ((requiredRelationship.MustBeLoggedIn || requiredRelationship.MustBeInConference || requiredRelationship.MustBeConferenceManager) && currentUserRecord.IsFailure)
        {
            return GetFailedResult<ConferenceYearRecord>(Unauthorized());
        }

        var conferenceYear = await _conferenceService.GetConferenceYear(conferenceID, year);
        if (conferenceYear is null)
        {
            return GetFailedResult<ConferenceYearRecord>(BadRequest("Conference year does not exist."));
        }

        var playersInConference = await _conferenceService.GetPlayersInConference(conferenceYear.Conference);

        bool isInConference = false;
        bool isConferenceManager = false;
        bool userIsAdmin = false;
        if (currentUserRecord.IsSuccess)
        {
            userIsAdmin = await _userManager.IsInRoleAsync(currentUserRecord.Value, "Admin");
            isConferenceManager = conferenceYear.Conference.ConferenceManager.UserID == currentUserRecord.Value.Id;
            if (requiredRelationship.MustBeConferenceManager && !isConferenceManager)
            {
                return GetFailedResult<ConferenceYearRecord>(Forbid());
            }

            isInConference = isConferenceManager || playersInConference.Any(x => x.User.UserID == currentUserRecord.Value.Id);
        }

        if (!isInConference && requiredRelationship.MustBeInConference)
        {
            return UnauthorizedOrForbid<ConferenceYearRecord>(currentUserRecord.IsSuccess);
        }

        ConferenceUserRelationship relationship = new ConferenceUserRelationship(isInConference, isConferenceManager, userIsAdmin);
        return new GenericResultRecord<ConferenceYearRecord>(new ConferenceYearRecord(currentUserRecord.ToNullable(), conferenceYear, relationship), null);
    }

    protected async Task<GenericResultRecord<ConferenceYearWithSupplementalDataRecord>> GetExistingConferenceYearWithSupplementalData(Guid conferenceID, int year, ConferenceRequiredRelationship requiredRelationship)
    {
        var currentUserRecord = await GetCurrentUser();
        if ((requiredRelationship.MustBeLoggedIn || requiredRelationship.MustBeInConference || requiredRelationship.MustBeConferenceManager) && currentUserRecord.IsFailure)
        {
            return GetFailedResult<ConferenceYearWithSupplementalDataRecord>(Unauthorized());
        }

        var conferenceYearData = await _conferenceService.GetConferenceYearData(conferenceID, year);
        if (conferenceYearData is null)
        {
            return GetFailedResult<ConferenceYearWithSupplementalDataRecord>(BadRequest("Conference year does not exist."));
        }

        bool isInConference = false;
        bool isConferenceManager = false;
        bool userIsAdmin = false;
        if (currentUserRecord.IsSuccess)
        {
            userIsAdmin = await _userManager.IsInRoleAsync(currentUserRecord.Value, "Admin");
            isConferenceManager = conferenceYearData.ConferenceYear.Conference.ConferenceManager.UserID == currentUserRecord.Value.Id;
            if (requiredRelationship.MustBeConferenceManager && !isConferenceManager)
            {
                return GetFailedResult<ConferenceYearWithSupplementalDataRecord>(Forbid());
            }

            isInConference = isConferenceManager || conferenceYearData.PlayersInConference.Any(x => x.User.UserID == currentUserRecord.Value.Id);
        }

        if (!isInConference && requiredRelationship.MustBeInConference)
        {
            return UnauthorizedOrForbid<ConferenceYearWithSupplementalDataRecord>(currentUserRecord.IsSuccess);
        }

        ConferenceUserRelationship relationship = new ConferenceUserRelationship(isInConference, isConferenceManager, userIsAdmin);
        return new GenericResultRecord<ConferenceYearWithSupplementalDataRecord>(new ConferenceYearWithSupplementalDataRecord(currentUserRecord.ToNullable(), conferenceYearData.ConferenceYear,
            conferenceYearData.PlayersInConference, relationship, conferenceYearData.LeagueYears, conferenceYearData.ConferenceYearStandings, conferenceYearData.ManagerMessages), null);
    }

    protected async Task PushDraftMessages(LeagueYear leagueYear, bool draftComplete)
    {
        await _hubContext.Clients.Group(leagueYear.GetGroupName).SendAsync("RefreshLeagueYear");
        if (draftComplete)
        {
            var updatedDraftStatus = DraftFunctions.GetDraftStatus(leagueYear);
            if (updatedDraftStatus != null)
            {
                await _discordPushService.SendNextDraftPublisherMessage(leagueYear, updatedDraftStatus.NextDraftPublisher,
                updatedDraftStatus.DraftPhase.Equals(DraftPhase.CounterPicks));
            }
        }
        else
        {
            await _hubContext.Clients.Group(leagueYear.GetGroupName).SendAsync("DraftFinished");
            await _discordPushService.SendDraftStartEndMessage(leagueYear, true);
        }
    }
}
