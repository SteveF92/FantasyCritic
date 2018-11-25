using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FantasyCritic.Lib.Domain;

namespace FantasyCritic.MySQL.Entities
{
    public class LeagueWideValuesEntity
    {
        public decimal AverageStandardGamePoints { get; set; }
        public decimal AverageCounterPickPoints { get; set; }

        public LeagueWideValues ToDomain()
        {
            return new LeagueWideValues(AverageStandardGamePoints, AverageCounterPickPoints);
        }
    }
}
