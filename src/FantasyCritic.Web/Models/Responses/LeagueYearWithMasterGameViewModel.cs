namespace FantasyCritic.Web.Models.Responses;

public class LeagueYearWithMasterGameViewModel
{
    public LeagueYearWithMasterGameViewModel(LeagueYearWithMasterGame domain)
    {
        LeagueID = domain.LeagueID;
        LeagueName = domain.LeagueName;
        Year = domain.Year;
        IsCounterPick = domain.IsCounterPick;
    }

    public Guid LeagueID { get; }
    public string LeagueName { get; }
    public int Year { get; }
    public bool IsCounterPick { get; }
}
