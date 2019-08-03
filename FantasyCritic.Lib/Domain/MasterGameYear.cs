using FantasyCritic.Lib.Domain.ScoringSystems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FantasyCritic.Lib.Domain
{
    public class MasterGameYear : IEquatable<MasterGameYear>
    {
        public MasterGameYear(MasterGame masterGame, int year)
        {
            MasterGame = masterGame;
            Year = year;
        }

        public MasterGameYear(MasterGame masterGame, int year, double percentStandardGame, double percentCounterPick, double eligiblePercentStandardGame,
            double eligiblePercentCounterPick, int numberOfBids, int totalBidAmount, double? averageDraftPosition, double? averageBidAmount,
            double hypeFactor, double dateAdjustedHypeFactor, double linearRegressionHypeFactor)
        {
            MasterGame = masterGame;
            Year = year;
            PercentStandardGame = percentStandardGame;
            PercentCounterPick = percentCounterPick;
            EligiblePercentStandardGame = eligiblePercentStandardGame;
            EligiblePercentCounterPick = eligiblePercentCounterPick;
            NumberOfBids = numberOfBids;
            TotalBidAmount = totalBidAmount;
            AverageDraftPosition = averageDraftPosition;
            AverageWinningBid = averageBidAmount;

            HypeFactor = hypeFactor;
            DateAdjustedHypeFactor = dateAdjustedHypeFactor;
            LinearRegressionHypeFactor = linearRegressionHypeFactor;
        }

        public MasterGame MasterGame { get; }
        public int Year { get; }
        public double PercentStandardGame { get; }
        public double PercentCounterPick { get; }
        public double EligiblePercentStandardGame { get; }
        public double EligiblePercentCounterPick { get; }
        public int NumberOfBids { get; }
        public int TotalBidAmount { get; }
        public double? AverageDraftPosition { get; }
        public double? AverageWinningBid { get; }
        public double HypeFactor { get; }
        public double DateAdjustedHypeFactor { get; }
        public double LinearRegressionHypeFactor { get; }

        public override string ToString() => $"{MasterGame}-{Year}";

        public bool WillRelease()
        {
            if (Year < MasterGame.MinimumReleaseDate.Year)
            {
                return false;
            }

            return true;
        }

        public decimal GetProjectedFantasyPoints(ScoringSystem scoringSystem, bool counterPick)
        {
            decimal? criticScoreToUse = CalculateFantasyPoints(scoringSystem, counterPick);
            if (!criticScoreToUse.HasValue)
            {
                criticScoreToUse = Convert.ToDecimal(LinearRegressionHypeFactor);
            }

            return scoringSystem.GetPointsForScore(criticScoreToUse.Value, counterPick);
        }

        public decimal? CalculateFantasyPoints(ScoringSystem scoringSystem, bool counterPick)
        {
            decimal criticScoreToUse;
            if (MasterGame.CriticScore.HasValue)
            {
                criticScoreToUse = MasterGame.CriticScore.Value;
            }
            else if (!WillRelease())
            {
                return 0m;
            }
            else
            {
                return null;
            }

            return scoringSystem.GetPointsForScore(criticScoreToUse, counterPick);
        }

        public bool Equals(MasterGameYear other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(MasterGame, other.MasterGame) && Year == other.Year;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((MasterGameYear) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((MasterGame != null ? MasterGame.GetHashCode() : 0) * 397) ^ Year;
            }
        }
    }
}
