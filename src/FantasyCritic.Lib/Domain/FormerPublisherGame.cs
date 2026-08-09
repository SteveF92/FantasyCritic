namespace FantasyCritic.Lib.Domain;

public class FormerPublisherGame : IPublisherGame
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

    public Guid PublisherID => PublisherGame.PublisherID;
    public Guid PublisherGameID => PublisherGame.PublisherGameID;
    public string OriginalGameName => PublisherGame.OriginalGameName;
    public Instant Timestamp => PublisherGame.Timestamp;
    public bool CounterPick => PublisherGame.CounterPick;
    public decimal? ManualCriticScore => PublisherGame.ManualCriticScore;
    public bool ManualWillNotRelease => PublisherGame.ManualWillNotRelease;
    public decimal? FantasyPoints => PublisherGame.FantasyPoints;
    public MasterGameYear? MasterGame => PublisherGame.MasterGame;
    public int SlotNumber => PublisherGame.SlotNumber;
    public int? PickNumber => PublisherGame.PickNumber;
    public int? OverallPickNumber => PublisherGame.OverallPickNumber;
    public Guid? DraftID => PublisherGame.DraftID;
    public uint? BidAmount => PublisherGame.BidAmount;
    public Guid? AcquiredInTradeID => PublisherGame.AcquiredInTradeID;
    public string GameName => PublisherGame.GameName;
    public bool IsFormerPublisherGame => true;
}
