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
        public decimal CounterpickConstant { get; set; }
        public decimal AverageDraftPositionConstant { get; set; }
        public decimal AverageBidAmountConstant { get; set; }

        public HypeConstants ToDomain()
        {
            return new HypeConstants(BaseScore, StandardGameConstant, CounterpickConstant, AverageDraftPositionConstant, AverageBidAmountConstant);
        }
    }
}
