
namespace FantasyCritic.Lib.Domain.AllTimeStats;
public record HallOfFameGame(MasterGameYear Game, Publisher PickedBy, IReadOnlyDictionary<string, HallOfFameGameStat> Stats);
public record HallOfFameGameStat(object Stat, string StatType);
