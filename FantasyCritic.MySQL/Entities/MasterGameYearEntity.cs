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
        public DateTime? ReleaseDate { get; set; }
        public int? OpenCriticID { get; set; }
        public decimal? CriticScore { get; set; }
        public int MinimumReleaseYear { get; set; }
        public int EligibilityLevel { get; set; }
        public bool YearlyInstallment { get; set; }
        public bool EarlyAccess { get; set; }
        public string BoxartFileName { get; set; }
        public DateTime? FirstCriticScoreTimestamp { get; set; }
        public bool DoNotRefresh { get; set; }
        public decimal PercentStandardGame { get; set; }
        public decimal PercentCounterPick { get; set; }
        public decimal EligiblePercentStandardGame { get; set; }
        public decimal EligiblePercentCounterPick { get; set; }
        public decimal AverageDraftPosition { get; set; }
        public decimal? HypeFactor { get; set; }
        public decimal? DateAdjustedHypeFactor { get; set; }
        public DateTime AddedTimestamp { get; set; }

        public MasterGameYear ToDomain(IEnumerable<MasterSubGame> subGames, EligibilityLevel eligibilityLevel, int year)
        {
            LocalDate? releaseDate = null;
            if (ReleaseDate.HasValue)
            {
                releaseDate = LocalDate.FromDateTime(ReleaseDate.Value);
            }

            Instant? firstCriticScoreTimestamp = null;
            if (FirstCriticScoreTimestamp.HasValue)
            {
                firstCriticScoreTimestamp = Instant.FromDateTimeUtc(FirstCriticScoreTimestamp.Value);
            }

            var addedTimestamp = LocalDateTime.FromDateTime(AddedTimestamp).InZoneStrictly(DateTimeZone.Utc).ToInstant();

            var masterGame =  new MasterGame(MasterGameID, GameName, EstimatedReleaseDate, releaseDate, OpenCriticID, CriticScore, MinimumReleaseYear, eligibilityLevel, 
                YearlyInstallment, EarlyAccess, subGames.ToList(), BoxartFileName, firstCriticScoreTimestamp, DoNotRefresh, addedTimestamp);

            return new MasterGameYear(masterGame, year, PercentStandardGame, PercentCounterPick, EligiblePercentStandardGame, EligiblePercentCounterPick, AverageDraftPosition, HypeFactor, DateAdjustedHypeFactor);
        }
    }
}
