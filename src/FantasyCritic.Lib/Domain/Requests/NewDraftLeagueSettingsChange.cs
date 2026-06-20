namespace FantasyCritic.Lib.Domain.Requests;

public record NewDraftLeagueSettingsChange(int StandardGames, int CounterPicks, IReadOnlyList<SpecialGameSlot> NewSpecialGameSlots);
