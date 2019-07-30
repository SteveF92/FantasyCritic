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

        public MasterGameYear(MasterGame masterGame, int year, decimal percentStandardGame, decimal percentCounterPick, decimal eligiblePercentStandardGame, 
            decimal eligiblePercentCounterPick, decimal? averageDraftPosition, decimal? averageBidAmount,
            decimal? hypeFactor, decimal? dateAdjustedHypeFactor, decimal? bidAdjustedHypeFactor, decimal? linearRegressionHypeFactor)
        {
            MasterGame = masterGame;
            Year = year;
            PercentStandardGame = percentStandardGame;
            PercentCounterPick = percentCounterPick;
            EligiblePercentStandardGame = eligiblePercentStandardGame;
            EligiblePercentCounterPick = eligiblePercentCounterPick;
            AverageDraftPosition = averageDraftPosition;
            AverageWinningBid = averageBidAmount;
            if (AverageDraftPosition == 0m)
            {
                AverageDraftPosition = null;
            }

            HypeFactor = hypeFactor;
            DateAdjustedHypeFactor = dateAdjustedHypeFactor;
            BidAdjustedHypeFactor = bidAdjustedHypeFactor;
            LinearRegressionHypeFactor = linearRegressionHypeFactor;
        }

        public MasterGame MasterGame { get; }
        public int Year { get; }
        public decimal PercentStandardGame { get; }
        public decimal PercentCounterPick { get; }
        public decimal EligiblePercentStandardGame { get; }
        public decimal EligiblePercentCounterPick { get; }
        public decimal? AverageDraftPosition { get; }
        public decimal? AverageWinningBid { get; }
        public decimal? HypeFactor { get; }
        public decimal? DateAdjustedHypeFactor { get; }
        public decimal? BidAdjustedHypeFactor { get; }
        public decimal? LinearRegressionHypeFactor { get; }
        

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
