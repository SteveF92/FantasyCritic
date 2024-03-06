namespace FantasyCritic.Lib.Domain;
public class LeagueYearWithMasterGame
{
    public Guid LeagueID { get; set; }
    public string? LeagueName { get; set; }
    public int Year { get; set; }
    public bool IsCounterPick { get; set; }
}
