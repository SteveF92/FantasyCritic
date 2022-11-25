namespace FantasyCritic.MySQL.Entities;

public class QueuedGameEntity
{
    public QueuedGameEntity()
    {

    }

    public QueuedGameEntity(QueuedGame domain)
    {
        PublisherID = domain.Publisher.PublisherID;
        MasterGameID = domain.MasterGame.MasterGameID;
        Ranking = domain.Rank;
    }

    public Guid PublisherID { get; set; }
    public Guid MasterGameID { get; set; }
    public int Ranking { get; set; }

    public QueuedGame ToDomain(Publisher publisher, MasterGame masterGame)
    {
        return new QueuedGame(publisher, masterGame, Ranking);
    }
}
