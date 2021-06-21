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
            MinimumReleaseDate = masterGameStats.MasterGame.MinimumReleaseDate;
            MaximumReleaseDate = masterGameStats.MasterGame.MaximumReleaseDate;
            EarlyAccessReleaseDate = masterGameStats.MasterGame.EarlyAccessReleaseDate;
            InternationalReleaseDate = masterGameStats.MasterGame.InternationalReleaseDate;
            ReleaseDate = masterGameStats.MasterGame.ReleaseDate;
            OpenCriticID = masterGameStats.MasterGame.OpenCriticID;
            CriticScore = masterGameStats.MasterGame.CriticScore;
            Notes = masterGameStats.MasterGame.Notes;
            BoxartFileName = masterGameStats.MasterGame.BoxartFileName;
            EligibilityChanged = masterGameStats.MasterGame.EligibilityChanged;
            FirstCriticScoreTimestamp = masterGameStats.MasterGame.FirstCriticScoreTimestamp;

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
            LinearRegressionHypeFactor = masterGameStats.LinearRegressionHypeFactor;
            AddedTimestamp = masterGameStats.MasterGame.AddedTimestamp;
        }

        public Guid MasterGameID { get; set; }
        public int Year { get; set; }
        public string GameName { get; set; }
        public string EstimatedReleaseDate { get; set; }
        public LocalDate MinimumReleaseDate { get; set; }
        public LocalDate? MaximumReleaseDate { get; set; }
        public LocalDate? EarlyAccessReleaseDate { get; set; }
        public LocalDate? InternationalReleaseDate { get; set; }
        public LocalDate? ReleaseDate { get; set; }
        public int? OpenCriticID { get; set; }
        public decimal? CriticScore { get; set; }
        public string Notes { get; set; }
        public string BoxartFileName { get; set; }
        public bool EligibilityChanged { get; set; }
        public Instant? FirstCriticScoreTimestamp { get; set; }
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
        public double LinearRegressionHypeFactor { get; set; }
        public Instant AddedTimestamp { get; set; }

        public MasterGameYear ToDomain(IEnumerable<MasterSubGame> subGames, int year, IEnumerable<MasterGameTag> tags)
        {
            var masterGame = new MasterGame(MasterGameID, GameName, EstimatedReleaseDate, MinimumReleaseDate, MaximumReleaseDate, EarlyAccessReleaseDate, InternationalReleaseDate,
                ReleaseDate, OpenCriticID, CriticScore, Notes, BoxartFileName, FirstCriticScoreTimestamp, false, false, EligibilityChanged, AddedTimestamp, subGames, tags);

            return new MasterGameYear(masterGame, year, PercentStandardGame, PercentCounterPick, EligiblePercentStandardGame, AdjustedPercentCounterPick, 
                NumberOfBids, TotalBidAmount, BidPercentile, AverageDraftPosition, AverageWinningBid, HypeFactor, DateAdjustedHypeFactor, LinearRegressionHypeFactor);
        }
    }
}
