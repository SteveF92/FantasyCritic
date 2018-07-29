using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FantasyCritic.Lib.Domain
{
    public class LeagueYear
    {
        public LeagueYear(League league, int year, LeagueOptions options)
        {
            League = league;
            Year = year;
            Options = options;
        }

        public League League { get; }
        public int Year { get; }
        public LeagueOptions Options { get; }
    }
}
