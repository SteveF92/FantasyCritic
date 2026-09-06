namespace FantasyCritic.Lib.Domain;

public class QueuedGame
{
    public const int MaximumNotesLength = 1000;

    public QueuedGame(Publisher publisher, MasterGame masterGame, int rank, string? notes)
    {
        Publisher = publisher;
        MasterGame = masterGame;
        Rank = rank;
        Notes = notes;
    }

    public Publisher Publisher { get; }
    public MasterGame MasterGame { get; }
    public int Rank { get; }
    public string? Notes { get; }

    public override string ToString()
    {
        return $"{Publisher.PublisherName}|{MasterGame.GameName}|{Rank}";
    }
}
