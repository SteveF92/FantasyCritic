using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FantasyCritic.Lib.Domain
{
    public class LeagueYear
    {
        public LeagueYear(League league, int year, LeagueOptions options, bool playStarted)
        {
            League = league;
            Year = year;
            Options = options;
            PlayStarted = playStarted;
        }

        public League League { get; }
        public int Year { get; }
        public LeagueOptions Options { get; }
        public bool PlayStarted { get; }
    }
}
