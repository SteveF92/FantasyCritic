namespace FantasyCritic.Web.Models.Responses;

public class PublisherGameViewModel
{
    public PublisherGameViewModel(PublisherGame publisherGame, LeagueYear leagueYear, LocalDate currentDate, Publisher publishedBy, Publisher? counterPickedBy, bool counterPicksBlockDrops)
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

        PublisherName = publishedBy.PublisherName;
        WillRelease = publisherGame.CouldRelease(leagueYear);
        WillReleaseEarlyAccessOnly = publisherGame.WillRelease(leagueYear).Equals(WillReleaseStatus.WillReleaseEarlyAccessOnly);
        ManualWillNotRelease = publisherGame.ManualWillNotRelease;
        OverallPickNumber = publisherGame.OverallPickNumber;
        DraftID = publisherGame.DraftID;
        BidAmount = publisherGame.BidAmount;
        AcquiredInTradeID = publisherGame.AcquiredInTradeID;
        SlotNumber = leagueYear.Options.LeagueSlotDictionary[publisherGame.LeagueSlotID].SlotNumber;
        CounterPicked = counterPickedBy is not null;
        CounterPickedBy = counterPickedBy?.PublisherName;
        DropBlocked = CounterPicked && counterPicksBlockDrops;
    }

    public PublisherGameViewModel(FormerPublisherGame publisherGame, LeagueYear leagueYear, Publisher publishedBy, LocalDate currentDate)
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

        PublisherName = publishedBy.PublisherName;
        WillRelease = publisherGame.CouldRelease(leagueYear);
        WillReleaseEarlyAccessOnly = publisherGame.WillRelease(leagueYear).Equals(WillReleaseStatus.WillReleaseEarlyAccessOnly);
        ManualWillNotRelease = publisherGame.ManualWillNotRelease;
        OverallPickNumber = publisherGame.OverallPickNumber;
        DraftID = publisherGame.DraftID;
        BidAmount = publisherGame.BidAmount;
        AcquiredInTradeID = publisherGame.AcquiredInTradeID;
        SlotNumber = 0; //TODO this is a small, SlotNumber should probably be nullable at the view model layer. But this is how the old system worked, so we're preserving it for now.
        RemovedTimestamp = publisherGame.RemovedTimestamp;
        RemovedNote = publisherGame.RemovedNote;
    }

    public Guid PublisherGameID { get; }
    public string? PublisherName { get; }
    public string GameName { get; }
    public DateTime Timestamp { get; }
    public bool CounterPick { get; }
    public string? EstimatedReleaseDate { get; }
    public DateTime? ReleaseDate { get; }
    public decimal? FantasyPoints { get; }
    public decimal? CriticScore { get; }

    public MasterGameYearViewModel? MasterGame { get; }
    public int? OverallPickNumber { get; }
    public Guid? DraftID { get; }
    public uint? BidAmount { get; }
    public Guid? AcquiredInTradeID { get; }
    public int SlotNumber { get; }

    public bool Linked { get; }
    public bool Released { get; }
    public bool WillRelease { get; }
    public bool WillReleaseEarlyAccessOnly { get; }
    public bool ManualCriticScore { get; }
    public bool ManualWillNotRelease { get; }
    public bool CounterPicked { get; }
    public string? CounterPickedBy { get; }
    public bool DropBlocked { get; }
    public Instant? RemovedTimestamp { get; }
    public string? RemovedNote { get; }
}
