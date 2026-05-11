using FantasyCritic.Lib.Domain.Requests;

namespace FantasyCritic.Web.Models.Requests.LeagueManager;

public class PublisherEditRequest
{
    public PublisherEditRequest(Guid publisherID, string publisherName, int budget,
        int unrestrictedReleaseStatusGamesDropped, int willNotReleaseGamesDropped, int willReleaseGamesDropped, int superDropsAvailable)
    {
        PublisherID = publisherID;
        PublisherName = publisherName;
        Budget = budget;
        UnrestrictedReleaseStatusGamesDropped = unrestrictedReleaseStatusGamesDropped;
        WillNotReleaseGamesDropped = willNotReleaseGamesDropped;
        WillReleaseGamesDropped = willReleaseGamesDropped;
        SuperDropsAvailable = superDropsAvailable;
    }
    

    public Guid PublisherID { get; }
    public string PublisherName { get; }
    public int Budget { get; }
    public int UnrestrictedReleaseStatusGamesDropped { get; }
    public int WillNotReleaseGamesDropped { get; }
    public int WillReleaseGamesDropped { get; }
    public int SuperDropsAvailable { get; }

    public EditPublisherRequest ToDomain(LeagueYear leagueYear, Publisher publisher)
    {
        return new EditPublisherRequest(leagueYear, publisher, PublisherName, Budget,
            UnrestrictedReleaseStatusGamesDropped, WillNotReleaseGamesDropped, WillReleaseGamesDropped, SuperDropsAvailable);
    }
}
