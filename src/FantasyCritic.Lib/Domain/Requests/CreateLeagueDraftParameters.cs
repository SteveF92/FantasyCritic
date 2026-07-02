namespace FantasyCritic.Lib.Domain.Requests;

public class CreateLeagueDraftParameters
{
    public CreateLeagueDraftParameters(LeagueYearKey leagueYearKey, string name, LocalDate? scheduledDate,
        int gamesToDraft, int counterPicksToDraft, bool counterPicksMustBeFromThisDraft, int additionalStandardGames, int additionalCounterPicks,
        IEnumerable<SpecialGameSlot> newSpecialGameSlots)
    {
        LeagueYearKey = leagueYearKey;
        Name = name;
        ScheduledDate = scheduledDate;
        GamesToDraft = gamesToDraft;
        CounterPicksToDraft = counterPicksToDraft;
        CounterPicksMustBeFromThisDraft = counterPicksMustBeFromThisDraft;
        AdditionalStandardGames = additionalStandardGames;
        AdditionalCounterPicks = additionalCounterPicks;
        NewSpecialGameSlots = newSpecialGameSlots.ToList();
    }

    public LeagueYearKey LeagueYearKey { get; }
    public string Name { get; }
    public LocalDate? ScheduledDate { get; }
    public int GamesToDraft { get; }
    public int CounterPicksToDraft { get; }
    public bool CounterPicksMustBeFromThisDraft { get; }
    public int AdditionalStandardGames { get; }
    public int AdditionalCounterPicks { get; }
    public IReadOnlyList<SpecialGameSlot> NewSpecialGameSlots { get; }
}
