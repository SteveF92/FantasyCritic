
namespace FantasyCritic.Lib.Domain.AllTimeStats;
public record HallOfFameGame(MasterGameYear Game, Publisher PickedBy, IReadOnlyDictionary<string, object> Stats);
