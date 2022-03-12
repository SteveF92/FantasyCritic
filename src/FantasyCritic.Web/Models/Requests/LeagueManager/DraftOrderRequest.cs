namespace FantasyCritic.Web.Models.Requests.LeagueManager;

public class DraftOrderRequest
{
    public Guid LeagueID { get; set; }
    public int Year { get; set; }
    public List<Guid> PublisherDraftPositions { get; set; }
}