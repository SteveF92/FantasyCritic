namespace FantasyCritic.Lib.Domain.Requests;

public class EditLeagueDraftParameters
{
    public EditLeagueDraftParameters(LeagueYearKey leagueYearKey, string name, LocalDate? scheduledDraft, int gamesToDraft, int counterPicksToDraft)
    {
        LeagueYearKey = leagueYearKey;
        Name = name;
        ScheduledDraft = scheduledDraft;
        GamesToDraft = gamesToDraft;
        CounterPicksToDraft = counterPicksToDraft;
    }

    public LeagueYearKey LeagueYearKey { get; }
    public string Name { get; }
    public LocalDate? ScheduledDraft { get; }
    public int GamesToDraft { get; }
    public int CounterPicksToDraft { get; }
}
