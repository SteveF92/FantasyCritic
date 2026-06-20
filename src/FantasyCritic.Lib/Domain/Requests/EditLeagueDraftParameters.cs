namespace FantasyCritic.Lib.Domain.Requests;

public class EditLeagueDraftParameters
{
    public EditLeagueDraftParameters(LeagueYearKey leagueYearKey, string name, LocalDate? scheduledDate, int gamesToDraft, int counterPicksToDraft)
    {
        LeagueYearKey = leagueYearKey;
        Name = name;
        ScheduledDate = scheduledDate;
        GamesToDraft = gamesToDraft;
        CounterPicksToDraft = counterPicksToDraft;
    }

    public LeagueYearKey LeagueYearKey { get; }
    public string Name { get; }
    public LocalDate? ScheduledDate { get; }
    public int GamesToDraft { get; }
    public int CounterPicksToDraft { get; }
}
