namespace FantasyCritic.Web.Models.Requests.LeagueManager;

public class LeaguePlayerActiveRequest
{
    public Guid LeagueID { get; set; }
    public int Year { get; set; }
    public Dictionary<Guid, bool> ActiveStatus { get; set; }
}