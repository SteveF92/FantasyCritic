using FantasyCritic.Lib.Domain.Conferences;
using FantasyCritic.Lib.Identity;

namespace FantasyCritic.Lib.Interfaces;
public interface IConferenceRepo
{
    Task<IReadOnlyList<MinimalConference>> GetConferencesForUser(FantasyCriticUser user);
    Task CreateConference(Conference conference, League primaryLeague, int year, LeagueOptions options);
    Task AddLeagueToConference(Conference conference, LeagueYear primaryLeagueYear, League newLeague);
    Task<Result> AddNewConferenceYear(Conference conference, int year);
    Task<Conference?> GetConference(Guid conferenceID);
    Task<ConferenceYear?> GetConferenceYear(Guid conferenceID, int year);
    Task<IReadOnlyList<FantasyCriticUser>> GetUsersInConference(Conference conference);
    Task<IReadOnlyList<ConferencePlayer>> GetPlayersInConference(Conference conference);
    Task RemovePlayerFromConference(Conference conference, FantasyCriticUser removeUser);
    Task<IReadOnlyList<ConferenceLeague>> GetLeaguesInConference(Conference conference);
    Task EditConference(Conference conference, string newConferenceName, bool newCustomRulesConference);

    Task<IReadOnlyList<ConferenceInviteLink>> GetInviteLinks(Conference conference);
    Task SaveInviteLink(ConferenceInviteLink inviteLink);
    Task DeactivateInviteLink(ConferenceInviteLink inviteLink);
    Task<ConferenceInviteLink?> GetInviteLinkByInviteCode(Guid inviteCode);
    Task AddPlayerToConference(Conference conference, FantasyCriticUser inviteUser);
    Task TransferConferenceManager(Conference conference, FantasyCriticUser newManager);
    Task SetConferenceLeagueLockStatus(LeagueYear leagueYear, bool locked);
    Task<Result> AssignLeaguePlayers(ConferenceYear conferenceYear, IReadOnlyList<ConferenceLeague> conferenceLeagues, IReadOnlyDictionary<ConferenceLeague, IReadOnlyList<FantasyCriticUser>> userAssignments);

    Task PostNewManagerMessage(ConferenceYear conferenceYear, ManagerMessage message);
    Task<Result> DeleteManagerMessage(ConferenceYear conferenceYear, Guid messageID);
    Task<Result> DismissManagerMessage(Guid messageID, Guid userId);
}
