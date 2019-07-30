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
        public Guid MasterGameID { get; set; }
        public string GameName { get; set; }
        public string EstimatedReleaseDate { get; set; }
        public DateTime? SortableEstimatedReleaseDate { get; set; }
        public DateTime? ReleaseDate { get; set; }
        public int? OpenCriticID { get; set; }
        public decimal? CriticScore { get; set; }
        public int MinimumReleaseYear { get; set; }
        public int EligibilityLevel { get; set; }
        public bool YearlyInstallment { get; set; }
        public bool EarlyAccess { get; set; }
        public bool FreeToPlay { get; set; }
        public bool ReleasedInternationally { get; set; }
        public bool ExpansionPack { get; set; }
        public string BoxartFileName { get; set; }
        public DateTime? FirstCriticScoreTimestamp { get; set; }
        public bool DoNotRefreshDate { get; set; }
        public bool DoNotRefreshAnything { get; set; }
        public decimal PercentStandardGame { get; set; }
        public decimal PercentCounterPick { get; set; }
        public decimal EligiblePercentStandardGame { get; set; }
        public decimal EligiblePercentCounterPick { get; set; }
        public decimal AverageDraftPosition { get; set; }
        public decimal? AverageBidAmount { get; set; }
        public decimal? HypeFactor { get; set; }
        public decimal? DateAdjustedHypeFactor { get; set; }
        public decimal? BidAdjustedHypeFactor { get; set; }
        public decimal? LinearRegressionHypeFactor { get; set; }
        public DateTime AddedTimestamp { get; set; }

        public MasterGameYear ToDomain(IEnumerable<MasterSubGame> subGames, EligibilityLevel eligibilityLevel, int year)
        {
            LocalDate? releaseDate = null;
            if (ReleaseDate.HasValue)
            {
                releaseDate = LocalDate.FromDateTime(ReleaseDate.Value);
            }

            LocalDate sortableEstimatedReleaseDate = LocalDate.MaxIsoValue;
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
            var eligibilitySettings = new EligibilitySettings(eligibilityLevel, YearlyInstallment, EarlyAccess, FreeToPlay, ReleasedInternationally, ExpansionPack);

            var masterGame =  new MasterGame(MasterGameID, GameName, EstimatedReleaseDate, sortableEstimatedReleaseDate, releaseDate, 
                OpenCriticID, CriticScore, MinimumReleaseYear, eligibilitySettings, subGames.ToList(), BoxartFileName, firstCriticScoreTimestamp, 
                DoNotRefreshDate, DoNotRefreshAnything, addedTimestamp);

            return new MasterGameYear(masterGame, year, PercentStandardGame, PercentCounterPick, EligiblePercentStandardGame, EligiblePercentCounterPick, 
                AverageDraftPosition, AverageBidAmount, HypeFactor, DateAdjustedHypeFactor, BidAdjustedHypeFactor, LinearRegressionHypeFactor);
        }
    }
}
