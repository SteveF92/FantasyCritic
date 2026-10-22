namespace FantasyCritic.MySQL.Entities;

public class FormerPublisherGameEntity
{
    public FormerPublisherGameEntity()
    {

    }

    public FormerPublisherGameEntity(Guid publisherGameID, Guid publisherID, string gameName, Instant timestamp, bool counterPick,
        decimal? manualCriticScore, bool manualWillNotRelease, decimal? fantasyPoints, Guid? masterGameID, int? pickNumber,
        int? overallPickNumber, uint? bidAmount, Guid? acquiredInTradeID, Instant removedTimestamp, string removedNote)
    {
        PublisherGameID = publisherGameID;
        PublisherID = publisherID;
        GameName = gameName;
        Timestamp = timestamp;
        CounterPick = counterPick;
        ManualCriticScore = manualCriticScore;
        ManualWillNotRelease = manualWillNotRelease;
        FantasyPoints = fantasyPoints;
        MasterGameID = masterGameID;
        PickNumber = pickNumber;
        OverallPickNumber = overallPickNumber;
        BidAmount = bidAmount;
        AcquiredInTradeID = acquiredInTradeID;
        RemovedTimestamp = removedTimestamp;
        RemovedNote = removedNote;
    }

    public FormerPublisherGameEntity(FormerPublisherGame publisherGame)
    {
        PublisherGameID = publisherGame.PublisherGameID;
        PublisherID = publisherGame.PublisherID;
        GameName = publisherGame.GameName;
        Timestamp = publisherGame.Timestamp;
        CounterPick = publisherGame.CounterPick;
        ManualCriticScore = publisherGame.ManualCriticScore;
        ManualWillNotRelease = publisherGame.ManualWillNotRelease;
        FantasyPoints = publisherGame.FantasyPoints;

        PickNumber = publisherGame.PickNumber;
        OverallPickNumber = publisherGame.OverallPickNumber;
        DraftID = publisherGame.DraftID;
        if (publisherGame.MasterGame is not null)
        {
            MasterGameID = publisherGame.MasterGame.MasterGame.MasterGameID;
        }

        BidAmount = publisherGame.BidAmount;
        RemovedTimestamp = publisherGame.RemovedTimestamp;
        RemovedNote = publisherGame.RemovedNote;
    }

    public Guid PublisherGameID { get; set; }
    public Guid PublisherID { get; set; }
    public string GameName { get; set; } = null!;
    public Instant Timestamp { get; set; }
    public bool CounterPick { get; set; }
    public decimal? ManualCriticScore { get; set; }
    public bool ManualWillNotRelease { get; set; }
    public decimal? FantasyPoints { get; set; }
    public Guid? MasterGameID { get; set; }
    public int? PickNumber { get; set; }
    public int? OverallPickNumber { get; set; }
    public Guid? DraftID { get; set; }
    public uint? BidAmount { get; set; }
    public Guid? AcquiredInTradeID { get; set; }
    public Instant RemovedTimestamp { get; set; }
    public string RemovedNote { get; set; } = null!;

    public FormerPublisherGame ToDomain(MasterGameYear? masterGame)
    {
        return new FormerPublisherGame(PublisherID, PublisherGameID, GameName, Timestamp, CounterPick,
            ManualCriticScore, ManualWillNotRelease, FantasyPoints, masterGame, PickNumber, OverallPickNumber, BidAmount, AcquiredInTradeID, DraftID, RemovedTimestamp, RemovedNote);
    }
}
