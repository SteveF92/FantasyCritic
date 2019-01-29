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
    public class LeagueYear : IEquatable<LeagueYear>
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

        public bool Equals(LeagueYear other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(League, other.League) && Year == other.Year;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((LeagueYear) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((League != null ? League.GetHashCode() : 0) * 397) ^ Year;
            }
        }
    }
}
