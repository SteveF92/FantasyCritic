namespace FantasyCritic.Lib.Domain.Requests;

public class AssociateGameDomainRequest
{
    public AssociateGameDomainRequest(Publisher publisher, PublisherGame publisherGame, MasterGame masterGame, bool managerOverride)
    {
        Publisher = publisher;
        PublisherGame = publisherGame;
        MasterGame = masterGame;
        ManagerOverride = managerOverride;
    }

    public Publisher Publisher { get; }
    public PublisherGame PublisherGame { get; }
    public MasterGame MasterGame { get; }
    public bool ManagerOverride { get; }
}