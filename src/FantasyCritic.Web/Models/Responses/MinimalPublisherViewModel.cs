namespace FantasyCritic.Web.Models.Responses;

public class MinimalPublisherViewModel
{
    public MinimalPublisherViewModel(Publisher domain)
    {
        PublisherID = domain.PublisherID;
        Year = domain.LeagueYearKey.Year;
        PublisherName = domain.PublisherName;
    }

    public Guid PublisherID { get; }
    public int Year { get; }
    public string PublisherName { get; }
}
