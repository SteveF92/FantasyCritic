namespace FantasyCritic.Lib.Domain;

public class FormerPublisherGame
{
    public FormerPublisherGame(PublisherGame publisherGame, Instant removedTimestamp, string removedNote)
    {
        PublisherGame = publisherGame;
        RemovedTimestamp = removedTimestamp;
        RemovedNote = removedNote;
    }

    public PublisherGame PublisherGame { get; }
    public Instant RemovedTimestamp { get; }
    public string RemovedNote { get; }

    public override string ToString() => PublisherGame.ToString();
}