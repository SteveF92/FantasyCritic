namespace FantasyCritic.Lib.Domain.AllTimeStats;

public record HallOfFameGameList(string Name, string StatName, string StatType, IReadOnlyList<HallOfFameGame> Games);
