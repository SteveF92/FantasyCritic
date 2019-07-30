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
        public decimal BaseScore { get; set; }
        public decimal StandardGameConstant { get; set; }
        public decimal CounterPickConstant { get; set; }
        public decimal AverageDraftPositionConstant { get; set; }
        public decimal AverageWinningBidConstant { get; set; }

        public HypeConstants ToDomain()
        {
            return new HypeConstants(BaseScore, StandardGameConstant, CounterPickConstant, AverageDraftPositionConstant, AverageWinningBidConstant);
        }
    }
}
