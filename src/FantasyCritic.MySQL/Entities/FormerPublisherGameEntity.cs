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
        PublisherGameID = publisherGame.PublisherGame.PublisherGameID;
        PublisherID = publisherGame.PublisherGame.PublisherID;
        GameName = publisherGame.PublisherGame.GameName;
        Timestamp = publisherGame.PublisherGame.Timestamp;
        CounterPick = publisherGame.PublisherGame.CounterPick;
        ManualCriticScore = publisherGame.PublisherGame.ManualCriticScore;
        ManualWillNotRelease = publisherGame.PublisherGame.ManualWillNotRelease;
        FantasyPoints = publisherGame.PublisherGame.FantasyPoints;

        PickNumber = publisherGame.PublisherGame.PickNumber;
        OverallPickNumber = publisherGame.PublisherGame.OverallPickNumber;
        DraftID = publisherGame.PublisherGame.DraftID;
        if (publisherGame.PublisherGame.MasterGame is not null)
        {
            MasterGameID = publisherGame.PublisherGame.MasterGame.MasterGame.MasterGameID;
        }

        BidAmount = publisherGame.PublisherGame.BidAmount;
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
        PublisherGame domain = new PublisherGame(PublisherID, PublisherGameID, GameName, Timestamp, CounterPick,
            ManualCriticScore, ManualWillNotRelease, FantasyPoints, masterGame, 0, PickNumber, OverallPickNumber, BidAmount, AcquiredInTradeID, DraftID);
        return new FormerPublisherGame(domain, RemovedTimestamp, RemovedNote);
    }
}
