namespace FantasyCritic.Lib.SharedSerialization.Database;

public class PublisherGameEntity
{
    public PublisherGameEntity()
    {

    }

    public PublisherGameEntity(PublisherGame publisherGame)
    {
        PublisherGameID = publisherGame.PublisherGameID;
        PublisherID = publisherGame.PublisherID;
        GameName = publisherGame.GameName;
        Timestamp = publisherGame.Timestamp;
        CounterPick = publisherGame.CounterPick;
        ManualCriticScore = publisherGame.ManualCriticScore;
        ManualWillNotRelease = publisherGame.ManualWillNotRelease;
        FantasyPoints = publisherGame.FantasyPoints;

        SlotNumber = publisherGame.SlotNumber;
        PickNumber = publisherGame.PickNumber;
        OverallPickNumber = publisherGame.OverallPickNumber;
        DraftID = publisherGame.DraftID;
        if (publisherGame.MasterGame is not null)
        {
            MasterGameID = publisherGame.MasterGame.MasterGame.MasterGameID;
        }

        BidAmount = publisherGame.BidAmount;
        AcquiredInTradeID = publisherGame.AcquiredInTradeID;
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
    public int SlotNumber { get; set; }
    public int? PickNumber { get; set; }
    public int? OverallPickNumber { get; set; }
    public Guid? DraftID { get; set; }
    public uint? BidAmount { get; set; }
    public Guid? AcquiredInTradeID { get; set; }

    public PublisherGame ToDomain(MasterGameYear? masterGame)
    {
        PublisherGame domain = new PublisherGame(PublisherID, PublisherGameID, GameName, Timestamp, CounterPick,
            ManualCriticScore, ManualWillNotRelease, FantasyPoints, masterGame, SlotNumber, PickNumber, OverallPickNumber, BidAmount, AcquiredInTradeID, DraftID);
        return domain;
    }
}
