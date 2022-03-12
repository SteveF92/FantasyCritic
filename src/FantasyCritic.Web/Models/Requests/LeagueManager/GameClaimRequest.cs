namespace FantasyCritic.Web.Models.Requests.LeagueManager;

public class ClaimGameRequest
{
    public Guid PublisherID { get; set; }
    public string GameName { get; set; }
    public bool CounterPick { get; set; }
    public Guid? MasterGameID { get; set; }
    public bool ManagerOverride { get; set; }
}