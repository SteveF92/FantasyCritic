namespace FantasyCritic.Web.Models.Responses;

public class LeaguePublisherViewModel
{
    public LeaguePublisherViewModel(Guid publisherID, string publisherName, Guid leagueID, string leagueName, int year)
    {
        PublisherID = publisherID;
        LeagueID = leagueID;
        LeagueName = leagueName;
        Year = year;
        PublisherName = publisherName;
    }

    public Guid PublisherID { get; }
    public string PublisherName { get; }
    public Guid LeagueID { get; }
    public string LeagueName { get; }
    public int Year { get; }
}
