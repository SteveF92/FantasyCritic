using FantasyCritic.Lib.Domain.LeagueActions;

namespace FantasyCritic.Lib.Domain.Requests;

public record NewDraftLeagueSettingsChanges(
    int StandardGames,
    int CounterPicks,
    IReadOnlyList<SpecialGameSlot> NewSpecialGameSlots,
    LeagueManagerAction LeagueManagerAction);
