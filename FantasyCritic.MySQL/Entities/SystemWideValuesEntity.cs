using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FantasyCritic.Lib.Domain;

namespace FantasyCritic.MySQL.Entities
{
    public class SystemWideValuesEntity
    {
        public decimal AverageStandardGamePoints { get; set; }
        public decimal AverageCounterPickPoints { get; set; }

        public SystemWideValues ToDomain()
        {
            return new SystemWideValues(AverageStandardGamePoints, AverageCounterPickPoints);
        }
    }
}
