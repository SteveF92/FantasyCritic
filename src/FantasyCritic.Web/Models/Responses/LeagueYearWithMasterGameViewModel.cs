namespace FantasyCritic.Web.Models.Responses;

public class LeagueYearWithMasterGameViewModel(Guid leagueID, string? leagueName, int year, bool isCounterPick)
{
    public Guid LeagueID { get; set; } = leagueID;
    public string? LeagueName { get; set; } = leagueName;
    public int Year { get; set; } = year;
    public bool IsCounterPick { get; set; } = isCounterPick;
}
