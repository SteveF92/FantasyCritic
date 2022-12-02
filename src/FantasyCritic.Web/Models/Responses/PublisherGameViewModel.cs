namespace FantasyCritic.Web.Models.Responses;

public class PublisherGameViewModel
{
    public PublisherGameViewModel(PublisherGame publisherGame, LocalDate currentDate, bool counterPicked, bool counterPicksBlockDrops)
    {
        PublisherGameID = publisherGame.PublisherGameID;
        GameName = publisherGame.GameName;

        Timestamp = publisherGame.Timestamp.ToDateTimeUtc();
        CounterPick = publisherGame.CounterPick;
        FantasyPoints = publisherGame.FantasyPoints;

        Linked = publisherGame.MasterGame is not null;
        if (Linked)
        {
            GameName = publisherGame.MasterGame!.MasterGame.GameName;
            EstimatedReleaseDate = publisherGame.MasterGame.MasterGame.EstimatedReleaseDate;
            if (publisherGame.MasterGame.MasterGame.ReleaseDate.HasValue)
            {
                ReleaseDate = publisherGame.MasterGame.MasterGame.ReleaseDate.Value.ToDateTimeUnspecified();
            }

            CriticScore = publisherGame.MasterGame.MasterGame.CriticScore;
            Released = publisherGame.MasterGame.MasterGame.IsReleased(currentDate);
            if (publisherGame.MasterGame is not null)
            {
                MasterGame = new MasterGameYearViewModel(publisherGame.MasterGame, currentDate);
            }
        }

        if (publisherGame.ManualCriticScore.HasValue)
        {
            CriticScore = publisherGame.ManualCriticScore;
            ManualCriticScore = true;
        }

        WillRelease = publisherGame.CouldRelease();
        ManualWillNotRelease = publisherGame.ManualWillNotRelease;
        OverallDraftPosition = publisherGame.OverallDraftPosition;
        BidAmount = publisherGame.BidAmount;
        AcquiredInTradeID = publisherGame.AcquiredInTradeID;
        SlotNumber = publisherGame.SlotNumber;
        CounterPicked = counterPicked;
        DropBlocked = counterPicked && counterPicksBlockDrops;
    }

    public PublisherGameViewModel(FormerPublisherGame publisherGame, LocalDate currentDate)
    {
        PublisherGameID = publisherGame.PublisherGame.PublisherGameID;
        GameName = publisherGame.PublisherGame.GameName;

        Timestamp = publisherGame.PublisherGame.Timestamp.ToDateTimeUtc();
        CounterPick = publisherGame.PublisherGame.CounterPick;
        FantasyPoints = publisherGame.PublisherGame.FantasyPoints;

        Linked = publisherGame.PublisherGame.MasterGame is not null;
        if (Linked)
        {
            GameName = publisherGame.PublisherGame.MasterGame!.MasterGame.GameName;
            EstimatedReleaseDate = publisherGame.PublisherGame.MasterGame.MasterGame.EstimatedReleaseDate;
            if (publisherGame.PublisherGame.MasterGame.MasterGame.ReleaseDate.HasValue)
            {
                ReleaseDate = publisherGame.PublisherGame.MasterGame.MasterGame.ReleaseDate.Value.ToDateTimeUnspecified();
            }

            CriticScore = publisherGame.PublisherGame.MasterGame.MasterGame.CriticScore;
            Released = publisherGame.PublisherGame.MasterGame.MasterGame.IsReleased(currentDate);
            if (publisherGame.PublisherGame.MasterGame is not null)
            {
                MasterGame = new MasterGameYearViewModel(publisherGame.PublisherGame.MasterGame, currentDate);
            }
        }

        if (publisherGame.PublisherGame.ManualCriticScore.HasValue)
        {
            CriticScore = publisherGame.PublisherGame.ManualCriticScore;
            ManualCriticScore = true;
        }

        WillRelease = publisherGame.PublisherGame.CouldRelease();
        ManualWillNotRelease = publisherGame.PublisherGame.ManualWillNotRelease;
        OverallDraftPosition = publisherGame.PublisherGame.OverallDraftPosition;
        BidAmount = publisherGame.PublisherGame.BidAmount;
        AcquiredInTradeID = publisherGame.PublisherGame.AcquiredInTradeID;
        SlotNumber = publisherGame.PublisherGame.SlotNumber;
        RemovedTimestamp = publisherGame.RemovedTimestamp;
        RemovedNote = publisherGame.RemovedNote;
    }

    public Guid PublisherGameID { get; }
    public string GameName { get; }
    public DateTime Timestamp { get; }
    public bool CounterPick { get; }
    public string? EstimatedReleaseDate { get; }
    public DateTime? ReleaseDate { get; }
    public decimal? FantasyPoints { get; }
    public decimal? CriticScore { get; }

    public MasterGameYearViewModel? MasterGame { get; }
    public int? OverallDraftPosition { get; }
    public uint? BidAmount { get; }
    public Guid? AcquiredInTradeID { get; }
    public int SlotNumber { get; }

    public bool Linked { get; }
    public bool Released { get; }
    public bool WillRelease { get; }
    public bool ManualCriticScore { get; }
    public bool ManualWillNotRelease { get; }
    public bool CounterPicked { get; }
    public bool DropBlocked { get; }
    public Instant? RemovedTimestamp { get; }
    public string? RemovedNote { get; }
}
