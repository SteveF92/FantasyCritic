using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FantasyCritic.Lib.Domain;
using NodaTime;

namespace FantasyCritic.MySQL.Entities
{
    internal class MasterGameYearEntity
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
            InternationalReleaseDate = masterGameStats.MasterGame.InternationalReleaseDate?.ToDateTimeUnspecified();
            EarlyAccessReleaseDate = masterGameStats.MasterGame.EarlyAccessReleaseDate?.ToDateTimeUnspecified();
            ReleaseDate = masterGameStats.MasterGame.ReleaseDate?.ToDateTimeUnspecified();
            OpenCriticID = masterGameStats.MasterGame.OpenCriticID;
            CriticScore = masterGameStats.MasterGame.CriticScore;
            EligibilityLevel = masterGameStats.MasterGame.EligibilitySettings.EligibilityLevel.Level;
            YearlyInstallment = masterGameStats.MasterGame.EligibilitySettings.YearlyInstallment;
            EarlyAccess = masterGameStats.MasterGame.EligibilitySettings.EarlyAccess;
            FreeToPlay = masterGameStats.MasterGame.EligibilitySettings.FreeToPlay;
            ReleasedInternationally = masterGameStats.MasterGame.EligibilitySettings.ReleasedInternationally;
            ExpansionPack = masterGameStats.MasterGame.EligibilitySettings.ExpansionPack;
            UnannouncedGame = masterGameStats.MasterGame.EligibilitySettings.UnannouncedGame;
            Notes = masterGameStats.MasterGame.Notes;
            BoxartFileName = masterGameStats.MasterGame.BoxartFileName;
            EligibilityChanged = masterGameStats.MasterGame.EligibilityChanged;
            FirstCriticScoreTimestamp = masterGameStats.MasterGame.FirstCriticScoreTimestamp?.ToDateTimeUtc();

            PercentStandardGame = masterGameStats.PercentStandardGame;
            PercentCounterPick = masterGameStats.PercentCounterPick;
            EligiblePercentStandardGame = masterGameStats.EligiblePercentStandardGame;
            EligiblePercentCounterPick = masterGameStats.EligiblePercentCounterPick;
            NumberOfBids = masterGameStats.NumberOfBids;
            TotalBidAmount = masterGameStats.TotalBidAmount;
            BidPercentile = masterGameStats.BidPercentile;
            AverageDraftPosition = masterGameStats.AverageDraftPosition;
            AverageWinningBid = masterGameStats.AverageWinningBid;
            HypeFactor = masterGameStats.HypeFactor;
            DateAdjustedHypeFactor = masterGameStats.DateAdjustedHypeFactor;
            LinearRegressionHypeFactor = masterGameStats.LinearRegressionHypeFactor;
            AddedTimestamp = masterGameStats.MasterGame.AddedTimestamp.ToDateTimeUtc();
        }

        public Guid MasterGameID { get; set; }
        public int Year { get; set; }
        public string GameName { get; set; }
        public string EstimatedReleaseDate { get; set; }
        public DateTime MinimumReleaseDate { get; set; }
        public DateTime? MaximumReleaseDate { get; set; }
        public DateTime? InternationalReleaseDate { get; set; }
        public DateTime? EarlyAccessReleaseDate { get; set; }
        public DateTime? ReleaseDate { get; set; }
        public int? OpenCriticID { get; set; }
        public decimal? CriticScore { get; set; }
        public int EligibilityLevel { get; set; }
        public bool YearlyInstallment { get; set; }
        public bool EarlyAccess { get; set; }
        public bool FreeToPlay { get; set; }
        public bool ReleasedInternationally { get; set; }
        public bool ExpansionPack { get; set; }
        public bool UnannouncedGame { get; set; }
        public string Notes { get; set; }
        public string BoxartFileName { get; set; }
        public bool EligibilityChanged { get; set; }
        public DateTime? FirstCriticScoreTimestamp { get; set; }
        public double PercentStandardGame { get; set; }
        public double PercentCounterPick { get; set; }
        public double EligiblePercentStandardGame { get; set; }
        public double EligiblePercentCounterPick { get; set; }
        public int NumberOfBids { get; set; }
        public int TotalBidAmount { get; set; }
        public double BidPercentile { get; set; }
        public double? AverageDraftPosition { get; set; }
        public double? AverageWinningBid { get; set; }
        public double HypeFactor { get; set; }
        public double DateAdjustedHypeFactor { get; set; }
        public double LinearRegressionHypeFactor { get; set; }
        public DateTime AddedTimestamp { get; set; }

        public MasterGameYear ToDomain(IEnumerable<MasterSubGame> subGames, EligibilityLevel eligibilityLevel, int year)
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

            LocalDate? internationalReleaseDate = null;
            if (InternationalReleaseDate.HasValue)
            {
                internationalReleaseDate = LocalDate.FromDateTime(InternationalReleaseDate.Value);
            }

            LocalDate? earlyAccessReleaseDate = null;
            if (EarlyAccessReleaseDate.HasValue)
            {
                earlyAccessReleaseDate = LocalDate.FromDateTime(EarlyAccessReleaseDate.Value);
            }

            Instant? firstCriticScoreTimestamp = null;
            if (FirstCriticScoreTimestamp.HasValue)
            {
                firstCriticScoreTimestamp = Instant.FromDateTimeUtc(FirstCriticScoreTimestamp.Value);
            }

            var addedTimestamp = LocalDateTime.FromDateTime(AddedTimestamp).InZoneStrictly(DateTimeZone.Utc).ToInstant();
            var eligibilitySettings = new EligibilitySettings(eligibilityLevel, YearlyInstallment, EarlyAccess, FreeToPlay, ReleasedInternationally, ExpansionPack, UnannouncedGame);

            var masterGame = new MasterGame(MasterGameID, GameName, EstimatedReleaseDate, LocalDate.FromDateTime(MinimumReleaseDate), maximumReleaseDate, internationalReleaseDate, earlyAccessReleaseDate,
                releaseDate, OpenCriticID, CriticScore, eligibilitySettings, Notes, BoxartFileName, firstCriticScoreTimestamp, false, false, EligibilityChanged, addedTimestamp);

            return new MasterGameYear(masterGame, year, PercentStandardGame, PercentCounterPick, EligiblePercentStandardGame, EligiblePercentCounterPick, 
                NumberOfBids, TotalBidAmount, BidPercentile, AverageDraftPosition, AverageWinningBid, HypeFactor, DateAdjustedHypeFactor, LinearRegressionHypeFactor);
        }
    }
}
