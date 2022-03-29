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

        Linked = publisherGame.MasterGame.HasValueTempoTemp;
        if (Linked)
        {
            GameName = publisherGame.MasterGame.ValueTempoTemp.MasterGame.GameName;
            EstimatedReleaseDate = publisherGame.MasterGame.ValueTempoTemp.MasterGame.EstimatedReleaseDate;
            if (publisherGame.MasterGame.ValueTempoTemp.MasterGame.ReleaseDate.HasValue)
            {
                ReleaseDate = publisherGame.MasterGame.ValueTempoTemp.MasterGame.ReleaseDate.Value.ToDateTimeUnspecified();
            }

            CriticScore = publisherGame.MasterGame.ValueTempoTemp.MasterGame.CriticScore;
            Released = publisherGame.MasterGame.ValueTempoTemp.MasterGame.IsReleased(currentDate);
            if (publisherGame.MasterGame.HasValueTempoTemp)
            {
                MasterGame = new MasterGameYearViewModel(publisherGame.MasterGame.ValueTempoTemp, currentDate);
            }
        }

        if (publisherGame.ManualCriticScore.HasValue)
        {
            CriticScore = publisherGame.ManualCriticScore;
            ManualCriticScore = true;
        }

        WillRelease = publisherGame.WillRelease();
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

        Linked = publisherGame.PublisherGame.MasterGame.HasValueTempoTemp;
        if (Linked)
        {
            GameName = publisherGame.PublisherGame.MasterGame.ValueTempoTemp.MasterGame.GameName;
            EstimatedReleaseDate = publisherGame.PublisherGame.MasterGame.ValueTempoTemp.MasterGame.EstimatedReleaseDate;
            if (publisherGame.PublisherGame.MasterGame.ValueTempoTemp.MasterGame.ReleaseDate.HasValue)
            {
                ReleaseDate = publisherGame.PublisherGame.MasterGame.ValueTempoTemp.MasterGame.ReleaseDate.Value.ToDateTimeUnspecified();
            }

            CriticScore = publisherGame.PublisherGame.MasterGame.ValueTempoTemp.MasterGame.CriticScore;
            Released = publisherGame.PublisherGame.MasterGame.ValueTempoTemp.MasterGame.IsReleased(currentDate);
            if (publisherGame.PublisherGame.MasterGame.HasValueTempoTemp)
            {
                MasterGame = new MasterGameYearViewModel(publisherGame.PublisherGame.MasterGame.ValueTempoTemp, currentDate);
            }
        }

        if (publisherGame.PublisherGame.ManualCriticScore.HasValue)
        {
            CriticScore = publisherGame.PublisherGame.ManualCriticScore;
            ManualCriticScore = true;
        }

        WillRelease = publisherGame.PublisherGame.WillRelease();
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
    public string EstimatedReleaseDate { get; }
    public DateTime? ReleaseDate { get; }
    public decimal? FantasyPoints { get; }
    public decimal? CriticScore { get; }

    public MasterGameYearViewModel MasterGame { get; }
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
    public string RemovedNote { get; }
}
