using FantasyCritic.Lib.Domain.Conferences;

namespace FantasyCritic.Lib.Domain.Combinations;

public record ConferenceYearData(ConferenceYear ConferenceYear, IReadOnlyList<ConferencePlayer> PlayersInConference, IReadOnlyList<LeagueYear> LeagueYears, SystemWideValues SystemWideValues, IReadOnlyList<ManagerMessage> ManagerMessages);

public record ConferenceYearDataWithStandings(ConferenceYear ConferenceYear, IReadOnlyList<ConferencePlayer> PlayersInConference, IReadOnlyList<LeagueYear> LeagueYears,
    IReadOnlyList<ManagerMessage> ManagerMessages, IReadOnlyList<ConferenceYearStanding> ConferenceYearStandings);
