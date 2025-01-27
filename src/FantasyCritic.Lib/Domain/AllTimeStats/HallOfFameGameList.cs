namespace FantasyCritic.Lib.Domain.AllTimeStats;

public record HallOfFameGameList(string Name, IReadOnlyList<HallOfFameGame> Games);
