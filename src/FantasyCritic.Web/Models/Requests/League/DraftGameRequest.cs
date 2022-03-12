namespace FantasyCritic.Web.Models.Requests.League;

public class DraftGameRequest
{
    public Guid PublisherID { get; set; }
    public string GameName { get; set; }
    public bool CounterPick { get; set; }
    public Guid? MasterGameID { get; set; }
}
