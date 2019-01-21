using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using FantasyCritic.Lib.Domain.Results;
using FantasyCritic.Lib.Enums;

namespace FantasyCritic.Lib.Domain
{
    public class LeagueYear
    {
        public LeagueYear(League league, int year, LeagueOptions options, PlayStatus playStatus)
        {
            League = league;
            Year = year;
            Options = options;
            PlayStatus = playStatus;
        }

        public League League { get; }
        public int Year { get; }
        public LeagueOptions Options { get; }
        public PlayStatus PlayStatus { get; }

        public LeagueYearKey Key => new LeagueYearKey(League.LeagueID, Year);

        public string GetGroupName => $"{League.LeagueID}|{Year}";
    }
}
