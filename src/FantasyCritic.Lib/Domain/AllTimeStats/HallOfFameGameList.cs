namespace FantasyCritic.Lib.Domain.AllTimeStats;

public record HallOfFameGameList(string Name, IReadOnlyList<HallOfFameStatType> StatTypes, IReadOnlyList<HallOfFameGame> Games);
public record HallOfFameStatType(string StatName, string StatType);
