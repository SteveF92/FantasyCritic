namespace FantasyCritic.Web.Models.Requests.Royale;

public class CreateRoyalePublisherRequest
{
    public int Year { get; set; }
    public int Quarter { get; set; }
    public string PublisherName { get; set; }
}