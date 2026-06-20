namespace FantasyCritic.Lib.Domain.Requests;

public class CreateLeagueDraftParameters
{
    public CreateLeagueDraftParameters(LeagueYearKey leagueYearKey, string name, LocalDate? scheduledDraft,
        int gamesToDraft, int counterPicksToDraft, int additionalStandardGames, int additionalCounterPicks, IEnumerable<SpecialGameSlot> newSpecialSlots)
    {
        LeagueYearKey = leagueYearKey;
        Name = name;
        ScheduledDraft = scheduledDraft;
        GamesToDraft = gamesToDraft;
        CounterPicksToDraft = counterPicksToDraft;
        AdditionalStandardGames = additionalStandardGames;
        AdditionalCounterPicks = additionalCounterPicks;
        NewSpecialSlots = newSpecialSlots.ToList();
    }

    public LeagueYearKey LeagueYearKey { get; }
    public string Name { get; }
    public LocalDate? ScheduledDraft { get; }
    public int GamesToDraft { get; }
    public int CounterPicksToDraft { get; }
    public int AdditionalStandardGames { get; }
    public int AdditionalCounterPicks { get; }
    public List<SpecialGameSlot> NewSpecialSlots { get; }
}
