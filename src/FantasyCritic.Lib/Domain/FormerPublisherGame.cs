namespace FantasyCritic.Lib.Domain;

public class FormerPublisherGame : IPublisherGame
{
    public FormerPublisherGame(Guid publisherID, Guid publisherGameID, string gameName, Instant timestamp, bool counterPick, decimal? manualCriticScore, bool manualWillNotRelease,
        decimal? fantasyPoints, MasterGameYear? masterGame, int? pickNumber, int? overallPickNumber, uint? bidAmount, Guid? acquiredInTradeID, Guid? draftID,
        Instant removedTimestamp, string removedNote)
    {
        PublisherID = publisherID;
        PublisherGameID = publisherGameID;
        OriginalGameName = gameName;
        Timestamp = timestamp;
        CounterPick = counterPick;
        ManualCriticScore = manualCriticScore;
        ManualWillNotRelease = manualWillNotRelease;
        FantasyPoints = fantasyPoints;
        MasterGame = masterGame;
        PickNumber = pickNumber;
        OverallPickNumber = overallPickNumber;
        BidAmount = bidAmount;
        AcquiredInTradeID = acquiredInTradeID;
        DraftID = draftID;
        RemovedTimestamp = removedTimestamp;
        RemovedNote = removedNote;
    }

    public Guid PublisherID { get; }
    public Guid PublisherGameID { get; }
    public string OriginalGameName { get; }
    public Instant Timestamp { get; }
    public bool CounterPick { get; }
    public decimal? ManualCriticScore { get; }
    public bool ManualWillNotRelease { get; }
    public decimal? FantasyPoints { get; }
    public MasterGameYear? MasterGame { get; }
    public int? PickNumber { get; }
    public int? OverallPickNumber { get; }
    public Guid? DraftID { get; }
    public uint? BidAmount { get; }
    public Guid? AcquiredInTradeID { get; }
    public Instant RemovedTimestamp { get; }
    public string RemovedNote { get; }
    public bool IsFormerPublisherGame => true;

    public string GameName => MasterGame?.MasterGame.GameName ?? OriginalGameName;

    public override string ToString() => GameName;

    public WillReleaseStatus WillRelease(LeagueYear leagueYear)
    {
        IPublisherGame thisGameAsInterface = this;
        return thisGameAsInterface.WillRelease(leagueYear);
    }

    public bool CouldRelease(LeagueYear leagueYear)
    {
        IPublisherGame thisGameAsInterface = this;
        return thisGameAsInterface.CouldRelease(leagueYear);
    }
}
