using FantasyCritic.Lib.Domain.Conferences;

namespace FantasyCritic.Lib.Domain.Combinations;

public record ConferenceYearData(ConferenceYear ConferenceYear, IReadOnlyList<ConferencePlayer> PlayersInConference, IReadOnlyList<ConferenceLeagueYear> LeagueYears,
    IReadOnlyList<ConferenceYearStanding> ConferenceYearStandings, IReadOnlyList<ManagerMessage> ManagerMessages);
