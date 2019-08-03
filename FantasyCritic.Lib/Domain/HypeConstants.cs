using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FantasyCritic.Lib.Domain
{
    public class HypeConstants
    {
        public HypeConstants(double baseScore, double standardGameConstant, double counterPickConstant, double averageDraftPositionConstant, 
            double averageBidAmountConstant, double numberOfBidsConstant)
        {
            BaseScore = baseScore;
            StandardGameConstant = standardGameConstant;
            CounterPickConstant = counterPickConstant;
            AverageDraftPositionConstant = averageDraftPositionConstant;
            AverageWinningBidConstant = averageBidAmountConstant;
            NumberOfBidsConstant = numberOfBidsConstant;
        }

        public double BaseScore { get; }
        public double StandardGameConstant { get; }
        public double CounterPickConstant { get; }
        public double AverageDraftPositionConstant { get; }
        public double AverageWinningBidConstant { get; }
        public double NumberOfBidsConstant { get; }
    }
}
