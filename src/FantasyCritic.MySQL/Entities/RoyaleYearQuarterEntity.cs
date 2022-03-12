using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FantasyCritic.Lib.Domain;
using FantasyCritic.Lib.Royale;

namespace FantasyCritic.MySQL.Entities
{
    internal class RoyaleYearQuarterEntity
    {
        public int Year { get; set; }
        public int Quarter { get; set; }
        public bool OpenForPlay { get; set; }
        public bool Finished { get; set; }

        public RoyaleYearQuarter ToDomain(SupportedYear supportedYear)
        {
            return new RoyaleYearQuarter(supportedYear, new YearQuarter(Year, Quarter), OpenForPlay, Finished);
        }
    }
}
