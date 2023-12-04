using FantasyCritic.Lib.Domain.Conferences;
using FantasyCritic.Lib.Identity;

namespace FantasyCritic.Lib.Interfaces;
public interface IConferenceRepo
{
    Task CreateConference(Conference conference, League primaryLeague, int year, LeagueOptions options);
    Task<Conference?> GetConference(Guid conferenceID);
    Task<ConferenceYear?> GetConferenceYear(Guid conferenceID, int year);
    Task<IReadOnlyList<Conference>> GetConferencesForUser(FantasyCriticUser user);
    Task<IReadOnlyList<FantasyCriticUser>> GetUsersInConference(Conference conference);
    Task<IReadOnlyList<ConferenceLeague>> GetLeaguesInConference(Conference conference);
    Task<IReadOnlyList<ConferenceLeagueYear>> GetLeagueYearsInConferenceYear(ConferenceYear conferenceYear);
}
