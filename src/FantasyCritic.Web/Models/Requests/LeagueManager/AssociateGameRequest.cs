namespace FantasyCritic.Web.Models.Requests.LeagueManager;

public class AssociateGameRequest
{
    public Guid PublisherID { get; set; }
    public Guid PublisherGameID { get; set; }
    public Guid MasterGameID { get; set; }
    public bool ManagerOverride { get; set; }
}
