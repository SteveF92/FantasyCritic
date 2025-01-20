namespace FantasyCritic.Lib.Domain.AllTimeStats;

public record LeagueAllTimeStats(IReadOnlyList<LeagueYear> LeagueYears, IReadOnlyList<LeaguePlayerAllTimeStats> PlayerAllTimeStats, IReadOnlyList<HallOfFameGameList> HallOfFameGameLists);
