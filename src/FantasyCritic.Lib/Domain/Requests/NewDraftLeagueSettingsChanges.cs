using FantasyCritic.Lib.Domain.LeagueActions;

namespace FantasyCritic.Lib.Domain.Requests;

public record NewDraftLeagueSettingsChanges(
    LeagueOptions NewLeagueOptions,
    IReadOnlyList<SpecialGameSlot> NewSpecialGameSlots,
    IReadOnlyDictionary<Guid, int> SlotAssignments,
    LeagueManagerAction LeagueManagerAction);
