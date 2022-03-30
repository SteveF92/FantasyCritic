using FantasyCritic.Lib.Domain.Requests;

namespace FantasyCritic.Web.Models.Requests.LeagueManager;

public class PublisherEditRequest
{
    public PublisherEditRequest(Guid publisherID, string publisherName, int budget, int freeGamesDropped, int willNotReleaseGamesDropped, int willReleaseGamesDropped)
    {
        PublisherID = publisherID;
        PublisherName = publisherName;
        Budget = budget;
        FreeGamesDropped = freeGamesDropped;
        WillNotReleaseGamesDropped = willNotReleaseGamesDropped;
        WillReleaseGamesDropped = willReleaseGamesDropped;
    }

    public Guid PublisherID { get; }
    public string PublisherName { get; }
    public int Budget { get; }
    public int FreeGamesDropped { get; }
    public int WillNotReleaseGamesDropped { get; }
    public int WillReleaseGamesDropped { get; }

    public EditPublisherRequest ToDomain(LeagueYear leagueYear, Publisher publisher)
    {
        return new EditPublisherRequest(leagueYear, publisher, PublisherName, Budget, FreeGamesDropped, WillNotReleaseGamesDropped, WillReleaseGamesDropped);
    }
}
