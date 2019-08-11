using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FantasyCritic.Lib.Domain
{
    public class HypeConstants
    {
        public HypeConstants(double baseScore, double standardGameConstant, double counterPickConstant, double hypeFactorConstant, double averageDraftPositionConstant,
            double totalBidAmountConstant, double bidPercentileConstant)
        {
            BaseScore = baseScore;
            StandardGameConstant = standardGameConstant;
            CounterPickConstant = counterPickConstant;
            HypeFactorConstant = hypeFactorConstant;
            AverageDraftPositionConstant = averageDraftPositionConstant;
            TotalBidAmountConstant = totalBidAmountConstant;
            BidPercentileConstant = bidPercentileConstant;
        }

        public double BaseScore { get; }
        public double StandardGameConstant { get; }
        public double CounterPickConstant { get; }
        public double HypeFactorConstant { get; }
        public double AverageDraftPositionConstant { get; }
        public double TotalBidAmountConstant { get; }
        public double BidPercentileConstant { get; }

        public override string ToString()
        {
            return
                $"BaseScore= {BaseScore}  StandardGameConstant= {StandardGameConstant}  CounterPickConstant= {CounterPickConstant}  " +
                $"HypeFactorConstant= {HypeFactorConstant}  AverageDraftPositionConstant= {AverageDraftPositionConstant}  " +
                $"TotalBidAmountConstant= {TotalBidAmountConstant} BidPercentileConstant = {BidPercentileConstant}";
        }
    }
}
