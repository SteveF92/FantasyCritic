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
        private readonly IReadOnlyDictionary<MasterGame, EligibilityOverride> _eligibilityOverridesDictionary;

        public LeagueYear(League league, int year, LeagueOptions options, PlayStatus playStatus, IEnumerable<EligibilityOverride> eligibilityOverrides)
        {
            League = league;
            Year = year;
            Options = options;
            PlayStatus = playStatus;
            EligibilityOverrides = eligibilityOverrides.ToList();
            _eligibilityOverridesDictionary = EligibilityOverrides.ToDictionary(x => x.MasterGame);
        }

        public League League { get; }
        public int Year { get; }
        public LeagueOptions Options { get; }
        public PlayStatus PlayStatus { get; }
        public IReadOnlyList<EligibilityOverride> EligibilityOverrides { get; }

        public LeagueYearKey Key => new LeagueYearKey(League.LeagueID, Year);

        public string GetGroupName => $"{League.LeagueID}|{Year}";

        public bool? GetOverriddenEligibility(MasterGame masterGame)
        {
            bool found = _eligibilityOverridesDictionary.TryGetValue(masterGame, out var eligibilityOverride);
            if (!found)
            {
                return null;
            }

            return eligibilityOverride.Eligible;
        }

        public bool GameIsEligible(MasterGame masterGame)
        {
            bool found = _eligibilityOverridesDictionary.TryGetValue(masterGame, out var eligibilityOverride);
            if (found)
            {
                return eligibilityOverride.Eligible;
            }

            var claimErrors = Options.AllowedEligibilitySettings.GameIsEligible(masterGame);
            return !claimErrors.Any();
        }

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
