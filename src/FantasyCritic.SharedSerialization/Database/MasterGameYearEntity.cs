using FantasyCritic.Lib.Domain;
using FantasyCritic.Lib.Identity;
using NodaTime;

namespace FantasyCritic.SharedSerialization.Database;

public class MasterGameYearEntity
{
    public MasterGameYearEntity()
    {

    }

    public MasterGameYearEntity(MasterGameCalculatedStats masterGameStats)
    {
        MasterGameID = masterGameStats.MasterGame.MasterGameID;
        Year = masterGameStats.Year;
        GameName = masterGameStats.MasterGame.GameName;
        EstimatedReleaseDate = masterGameStats.MasterGame.EstimatedReleaseDate;
        MinimumReleaseDate = masterGameStats.MasterGame.MinimumReleaseDate.ToDateTimeUnspecified();
        MaximumReleaseDate = masterGameStats.MasterGame.MaximumReleaseDate?.ToDateTimeUnspecified();
        EarlyAccessReleaseDate = masterGameStats.MasterGame.EarlyAccessReleaseDate?.ToDateTimeUnspecified();
        InternationalReleaseDate = masterGameStats.MasterGame.InternationalReleaseDate?.ToDateTimeUnspecified();
        AnnouncementDate = masterGameStats.MasterGame.AnnouncementDate?.ToDateTimeUnspecified();
        ReleaseDate = masterGameStats.MasterGame.ReleaseDate?.ToDateTimeUnspecified();
        OpenCriticID = masterGameStats.MasterGame.OpenCriticID;
        GGToken = masterGameStats.MasterGame.GGToken;
        CriticScore = masterGameStats.MasterGame.CriticScore;
        OpenCriticSlug = masterGameStats.MasterGame.OpenCriticSlug;
        Notes = masterGameStats.MasterGame.Notes;
        BoxartFileName = masterGameStats.MasterGame.BoxartFileName;
        GGCoverArtFileName = masterGameStats.MasterGame.GGCoverArtFileName;
        UseSimpleEligibility = masterGameStats.MasterGame.UseSimpleEligibility;
        DelayContention = masterGameStats.MasterGame.DelayContention;
        ShowNote = masterGameStats.MasterGame.ShowNote;
        FirstCriticScoreTimestamp = masterGameStats.MasterGame.FirstCriticScoreTimestamp?.ToDateTimeUtc();

        PercentStandardGame = masterGameStats.PercentStandardGame;
        PercentCounterPick = masterGameStats.PercentCounterPick;
        EligiblePercentStandardGame = masterGameStats.EligiblePercentStandardGame;
        AdjustedPercentCounterPick = masterGameStats.AdjustedPercentCounterPick;
        NumberOfBids = masterGameStats.NumberOfBids;
        TotalBidAmount = masterGameStats.TotalBidAmount;
        BidPercentile = masterGameStats.BidPercentile;
        AverageDraftPosition = masterGameStats.AverageDraftPosition;
        AverageWinningBid = masterGameStats.AverageWinningBid;
        HypeFactor = masterGameStats.HypeFactor;
        DateAdjustedHypeFactor = masterGameStats.DateAdjustedHypeFactor;
        PeakHypeFactor = masterGameStats.PeakHypeFactor;
        LinearRegressionHypeFactor = masterGameStats.LinearRegressionHypeFactor;
        AddedTimestamp = masterGameStats.MasterGame.AddedTimestamp.ToDateTimeUtc();
        AddedByUserID = masterGameStats.MasterGame.AddedByUser.Id;
    }

    public Guid MasterGameID { get; set; }
    public int Year { get; set; }
    public string GameName { get; set; } = null!;
    public string EstimatedReleaseDate { get; set; } = null!;
    public DateTime MinimumReleaseDate { get; set; }
    public DateTime? MaximumReleaseDate { get; set; }
    public DateTime? EarlyAccessReleaseDate { get; set; }
    public DateTime? InternationalReleaseDate { get; set; }
    public DateTime? AnnouncementDate { get; set; }
    public DateTime? ReleaseDate { get; set; }
    public int? OpenCriticID { get; set; }
    public string? GGToken { get; set; }
    public decimal? CriticScore { get; set; }
    public bool HasAnyReviews { get; set; }
    public string? OpenCriticSlug { get; set; }
    public string? Notes { get; set; }
    public string? BoxartFileName { get; set; }
    public string? GGCoverArtFileName { get; set; }
    public bool UseSimpleEligibility { get; set; }
    public bool DelayContention { get; set; }
    public bool ShowNote { get; set; }
    public DateTimeOffset? FirstCriticScoreTimestamp { get; set; }
    public double PercentStandardGame { get; set; }
    public double PercentCounterPick { get; set; }
    public double EligiblePercentStandardGame { get; set; }
    public double? AdjustedPercentCounterPick { get; set; }
    public int NumberOfBids { get; set; }
    public int TotalBidAmount { get; set; }
    public double BidPercentile { get; set; }
    public double? AverageDraftPosition { get; set; }
    public double? AverageWinningBid { get; set; }
    public double HypeFactor { get; set; }
    public double DateAdjustedHypeFactor { get; set; }
    public double PeakHypeFactor { get; set; }
    public double LinearRegressionHypeFactor { get; set; }
    public DateTime AddedTimestamp { get; set; }
    public Guid AddedByUserID { get; set; }

    public MasterGameYear ToDomain(IEnumerable<MasterSubGame> subGames, IEnumerable<MasterGameTag> tags, FantasyCriticUser addedByUser)
    {
        LocalDate? releaseDate = null;
        if (ReleaseDate.HasValue)
        {
            releaseDate = LocalDate.FromDateTime(ReleaseDate.Value);
        }

        LocalDate? maximumReleaseDate = null;
        if (MaximumReleaseDate.HasValue)
        {
            maximumReleaseDate = LocalDate.FromDateTime(MaximumReleaseDate.Value);
        }

        LocalDate? earlyAccessReleaseDate = null;
        if (EarlyAccessReleaseDate.HasValue)
        {
            earlyAccessReleaseDate = LocalDate.FromDateTime(EarlyAccessReleaseDate.Value);
        }

        LocalDate? internationalReleaseDate = null;
        if (InternationalReleaseDate.HasValue)
        {
            internationalReleaseDate = LocalDate.FromDateTime(InternationalReleaseDate.Value);
        }

        LocalDate? announcementDate = null;
        if (AnnouncementDate.HasValue)
        {
            announcementDate = LocalDate.FromDateTime(AnnouncementDate.Value);
        }

        Instant? firstCriticScoreTimestamp = null;
        if (FirstCriticScoreTimestamp.HasValue)
        {
            firstCriticScoreTimestamp = Instant.FromDateTimeOffset(FirstCriticScoreTimestamp.Value);
        }

        var addedTimestamp = LocalDateTime.FromDateTime(AddedTimestamp).InZoneStrictly(DateTimeZone.Utc).ToInstant();

        var masterGame = new MasterGame(MasterGameID, GameName, EstimatedReleaseDate, LocalDate.FromDateTime(MinimumReleaseDate),
            maximumReleaseDate, earlyAccessReleaseDate, internationalReleaseDate, announcementDate,
            releaseDate, OpenCriticID, GGToken, CriticScore, HasAnyReviews, OpenCriticSlug, Notes, BoxartFileName, GGCoverArtFileName,
            firstCriticScoreTimestamp, false, false, UseSimpleEligibility, DelayContention, ShowNote, addedTimestamp, addedByUser, subGames, tags);

        return new MasterGameYear(masterGame, Year, PercentStandardGame, PercentCounterPick, EligiblePercentStandardGame, AdjustedPercentCounterPick,
            NumberOfBids, TotalBidAmount, BidPercentile, AverageDraftPosition, AverageWinningBid, HypeFactor, DateAdjustedHypeFactor, PeakHypeFactor, LinearRegressionHypeFactor);
    }
}
