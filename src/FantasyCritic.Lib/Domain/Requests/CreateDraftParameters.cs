namespace FantasyCritic.Lib.Domain.Requests;

public class CreateDraftParameters
{
    public CreateDraftParameters(LeagueYearKey leagueYearKey, int gamesToDraft, int counterPicksToDraft, int? newStandardGames)
    {
        LeagueYearKey = leagueYearKey;
        GamesToDraft = gamesToDraft;
        CounterPicksToDraft = counterPicksToDraft;
        NewStandardGames = newStandardGames;
    }

    public LeagueYearKey LeagueYearKey { get; }
    public int GamesToDraft { get; }
    public int CounterPicksToDraft { get; }
    public int? NewStandardGames { get; }
}
