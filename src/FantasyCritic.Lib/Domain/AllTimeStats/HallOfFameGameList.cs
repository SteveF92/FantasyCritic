namespace FantasyCritic.Lib.Domain.AllTimeStats;

public record HallOfFameGameList(string Name, string StatName, IReadOnlyList<HallOfFameGame> Games);
