namespace FantasyCritic.Lib.Domain.Requests;

public class AssociateGameDomainRequest
{
    public AssociateGameDomainRequest(LeagueYear leagueYear, Publisher publisher, PublisherGame publisherGame, MasterGame masterGame, bool managerOverride)
    {
        LeagueYear = leagueYear;
        Publisher = publisher;
        PublisherGame = publisherGame;
        MasterGame = masterGame;
        ManagerOverride = managerOverride;
    }

    public LeagueYear LeagueYear { get; }
    public Publisher Publisher { get; }
    public PublisherGame PublisherGame { get; }
    public MasterGame MasterGame { get; }
    public bool ManagerOverride { get; }
}
