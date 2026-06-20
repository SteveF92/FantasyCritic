namespace FantasyCritic.Lib.Domain.Requests;

public class CreateLeagueDraftParameters
{
    public CreateLeagueDraftParameters(LeagueYearKey leagueYearKey, string name, LocalDate? scheduledDate,
        int gamesToDraft, int counterPicksToDraft, int additionalStandardGames, int additionalCounterPicks, IEnumerable<SpecialGameSlot> newSpecialSlots)
    {
        LeagueYearKey = leagueYearKey;
        Name = name;
        ScheduledDate = scheduledDate;
        GamesToDraft = gamesToDraft;
        CounterPicksToDraft = counterPicksToDraft;
        AdditionalStandardGames = additionalStandardGames;
        AdditionalCounterPicks = additionalCounterPicks;
        NewSpecialSlots = newSpecialSlots.ToList();
    }

    public LeagueYearKey LeagueYearKey { get; }
    public string Name { get; }
    public LocalDate? ScheduledDate { get; }
    public int GamesToDraft { get; }
    public int CounterPicksToDraft { get; }
    public int AdditionalStandardGames { get; }
    public int AdditionalCounterPicks { get; }
    public List<SpecialGameSlot> NewSpecialSlots { get; }
}
