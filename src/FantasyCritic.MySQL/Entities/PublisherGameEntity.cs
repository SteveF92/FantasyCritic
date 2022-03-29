namespace FantasyCritic.MySQL.Entities;

internal class PublisherGameEntity
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
        DraftPosition = publisherGame.DraftPosition;
        OverallDraftPosition = publisherGame.OverallDraftPosition;
        if (publisherGame.MasterGame.HasValueTempoTemp)
        {
            MasterGameID = publisherGame.MasterGame.ValueTempoTemp.MasterGame.MasterGameID;
        }

        BidAmount = publisherGame.BidAmount;
        AcquiredInTradeID = publisherGame.AcquiredInTradeID;
    }

    public Guid PublisherGameID { get; set; }
    public Guid PublisherID { get; set; }
    public string GameName { get; set; }
    public Instant Timestamp { get; set; }
    public bool CounterPick { get; set; }
    public decimal? ManualCriticScore { get; set; }
    public bool ManualWillNotRelease { get; set; }
    public decimal? FantasyPoints { get; set; }
    public Guid? MasterGameID { get; set; }
    public int SlotNumber { get; set; }
    public int? DraftPosition { get; set; }
    public int? OverallDraftPosition { get; set; }
    public uint? BidAmount { get; set; }
    public Guid? AcquiredInTradeID { get; set; }

    public PublisherGame ToDomain(Maybe<MasterGameYear> masterGame)
    {
        PublisherGame domain = new PublisherGame(PublisherID, PublisherGameID, GameName, Timestamp, CounterPick,
            ManualCriticScore, ManualWillNotRelease, FantasyPoints, masterGame, SlotNumber, DraftPosition, OverallDraftPosition, BidAmount, AcquiredInTradeID);
        return domain;
    }
}
