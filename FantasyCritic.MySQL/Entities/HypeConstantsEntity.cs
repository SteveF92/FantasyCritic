using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FantasyCritic.Lib.Domain;

namespace FantasyCritic.MySQL.Entities
{
    public class HypeConstantsEntity
    {
        public double BaseScore { get; set; }
        public double StandardGameConstant { get; set; }
        public double CounterPickConstant { get; set; }
        public double AverageDraftPositionConstant { get; set; }
        public double AverageWinningBidConstant { get; set; }
        public double NumberOfBidsConstant { get; set; }

        public HypeConstants ToDomain()
        {
            return new HypeConstants(BaseScore, StandardGameConstant, CounterPickConstant, AverageDraftPositionConstant, AverageWinningBidConstant, NumberOfBidsConstant);
        }
    }
}
