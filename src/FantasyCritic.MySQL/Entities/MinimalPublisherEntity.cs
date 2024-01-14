namespace FantasyCritic.MySQL.Entities;
internal class MinimalPublisherEntity
{
    public MinimalPublisherEntity()
    {

    }

    public MinimalPublisherEntity(Guid publisherID, string publisherName, Guid leagueID, string leagueName, int year)
    {
        PublisherID = publisherID;
        LeagueID = leagueID;
        LeagueName = leagueName;
        Year = year;
        PublisherName = publisherName;
    }

    public Guid PublisherID { get; set; }
    public string PublisherName { get; set; } = null!;
    public Guid LeagueID { get; set; }
    public string LeagueName { get; set; } = null!;
    public int Year { get; set; }

    public MinimalPublisher ToDomain()
    {
        return new MinimalPublisher(PublisherID, PublisherName!, LeagueID, LeagueName!, Year);
    }
}
