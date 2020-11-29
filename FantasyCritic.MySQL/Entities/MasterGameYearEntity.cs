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
            MasterGameID = masterGameStats.MasterGameYear.MasterGame.MasterGameID;
            Year = masterGameStats.MasterGameYear.Year;
            GameName = masterGameStats.MasterGameYear.MasterGame.GameName;
            EstimatedReleaseDate = masterGameStats.MasterGameYear.MasterGame.EstimatedReleaseDate;
            SortableEstimatedReleaseDate = masterGameStats.MasterGameYear.MasterGame.SortableEstimatedReleaseDate?.ToDateTimeUnspecified();
            ReleaseDate = masterGameStats.MasterGameYear.MasterGame.ReleaseDate?.ToDateTimeUnspecified();
            OpenCriticID = masterGameStats.MasterGameYear.MasterGame.OpenCriticID;
            CriticScore = masterGameStats.MasterGameYear.MasterGame.CriticScore;
            MinimumReleaseDate = masterGameStats.MasterGameYear.MasterGame.MinimumReleaseDate.ToDateTimeUnspecified();
            EligibilityLevel = masterGameStats.MasterGameYear.MasterGame.EligibilitySettings.EligibilityLevel.Level;
            YearlyInstallment = masterGameStats.MasterGameYear.MasterGame.EligibilitySettings.YearlyInstallment;
            EarlyAccess = masterGameStats.MasterGameYear.MasterGame.EligibilitySettings.EarlyAccess;
            FreeToPlay = masterGameStats.MasterGameYear.MasterGame.EligibilitySettings.FreeToPlay;
            ReleasedInternationally = masterGameStats.MasterGameYear.MasterGame.EligibilitySettings.ReleasedInternationally;
            ExpansionPack = masterGameStats.MasterGameYear.MasterGame.EligibilitySettings.ExpansionPack;
            UnannouncedGame = masterGameStats.MasterGameYear.MasterGame.EligibilitySettings.UnannouncedGame;
            Notes = masterGameStats.MasterGameYear.MasterGame.Notes;
            BoxartFileName = masterGameStats.MasterGameYear.MasterGame.BoxartFileName;
            EligibilityChanged = masterGameStats.MasterGameYear.MasterGame.EligibilityChanged;
            FirstCriticScoreTimestamp = masterGameStats.MasterGameYear.MasterGame.FirstCriticScoreTimestamp?.ToDateTimeUtc();
            PercentStandardGame = masterGameStats.MasterGameYear.PercentStandardGame;
            PercentCounterPick = masterGameStats.MasterGameYear.PercentCounterPick;
            EligiblePercentStandardGame = masterGameStats.MasterGameYear.EligiblePercentStandardGame;
            EligiblePercentCounterPick = masterGameStats.MasterGameYear.EligiblePercentCounterPick;
            NumberOfBids = masterGameStats.MasterGameYear.NumberOfBids;
            TotalBidAmount = masterGameStats.MasterGameYear.TotalBidAmount;
            BidPercentile = masterGameStats.MasterGameYear.BidPercentile;
            AverageDraftPosition = masterGameStats.MasterGameYear.AverageDraftPosition;
            AverageWinningBid = masterGameStats.MasterGameYear.AverageWinningBid;
            HypeFactor = masterGameStats.HypeFactor;
            DateAdjustedHypeFactor = masterGameStats.DateAdjustedHypeFactor;
            LinearRegressionHypeFactor = masterGameStats.LinearRegressionHypeFactor;
            AddedTimestamp = masterGameStats.MasterGameYear.MasterGame.AddedTimestamp.ToDateTimeUtc();
        }

        public Guid MasterGameID { get; set; }
        public int Year { get; set; }
        public string GameName { get; set; }
        public string EstimatedReleaseDate { get; set; }
        public DateTime? SortableEstimatedReleaseDate { get; set; }
        public DateTime? ReleaseDate { get; set; }
        public int? OpenCriticID { get; set; }
        public decimal? CriticScore { get; set; }
        public DateTime MinimumReleaseDate { get; set; }
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

            LocalDate? sortableEstimatedReleaseDate = null;
            if (SortableEstimatedReleaseDate.HasValue)
            {
                sortableEstimatedReleaseDate = LocalDate.FromDateTime(SortableEstimatedReleaseDate.Value);
            }

            Instant? firstCriticScoreTimestamp = null;
            if (FirstCriticScoreTimestamp.HasValue)
            {
                firstCriticScoreTimestamp = Instant.FromDateTimeUtc(FirstCriticScoreTimestamp.Value);
            }

            var addedTimestamp = LocalDateTime.FromDateTime(AddedTimestamp).InZoneStrictly(DateTimeZone.Utc).ToInstant();
            var eligibilitySettings = new EligibilitySettings(eligibilityLevel, YearlyInstallment, EarlyAccess, FreeToPlay, ReleasedInternationally, ExpansionPack, UnannouncedGame);

            var masterGame =  new MasterGame(MasterGameID, GameName, EstimatedReleaseDate, sortableEstimatedReleaseDate, releaseDate, 
                OpenCriticID, CriticScore, LocalDate.FromDateTime(MinimumReleaseDate), eligibilitySettings, Notes, subGames.ToList(), BoxartFileName, firstCriticScoreTimestamp, 
                false, false, EligibilityChanged, addedTimestamp);

            return new MasterGameYear(masterGame, year, PercentStandardGame, PercentCounterPick, EligiblePercentStandardGame, EligiblePercentCounterPick, 
                NumberOfBids, TotalBidAmount, BidPercentile, AverageDraftPosition, AverageWinningBid, HypeFactor, DateAdjustedHypeFactor, LinearRegressionHypeFactor);
        }
    }
}
