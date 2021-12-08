using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using FantasyCritic.Lib.Domain.Results;
using FantasyCritic.Lib.Enums;
using FantasyCritic.Lib.Services;
using FantasyCritic.Lib.Utilities;
using NodaTime;

namespace FantasyCritic.Lib.Domain
{
    public class LeagueYear : IEquatable<LeagueYear>
    {
        private readonly IReadOnlyDictionary<MasterGame, EligibilityOverride> _eligibilityOverridesDictionary;
        private readonly IReadOnlyDictionary<MasterGame, TagOverride> _tagOverridesDictionary;

        public LeagueYear(League league, SupportedYear year, LeagueOptions options, PlayStatus playStatus, 
            IEnumerable<EligibilityOverride> eligibilityOverrides, IEnumerable<TagOverride> tagOverrides, 
            Instant? draftStartedTimestamp)
        {
            League = league;
            SupportedYear = year;
            Options = options;
            PlayStatus = playStatus;
            EligibilityOverrides = eligibilityOverrides.ToList();
            _eligibilityOverridesDictionary = EligibilityOverrides.ToDictionary(x => x.MasterGame);
            TagOverrides = tagOverrides.ToList();
            _tagOverridesDictionary = TagOverrides.ToDictionary(x => x.MasterGame);
            DraftStartedTimestamp = draftStartedTimestamp;
        }

        public League League { get; }
        public SupportedYear SupportedYear { get; }
        public int Year => SupportedYear.Year;
        public LeagueOptions Options { get; }
        public PlayStatus PlayStatus { get; }
        public IReadOnlyList<EligibilityOverride> EligibilityOverrides { get; }
        public IReadOnlyList<TagOverride> TagOverrides { get; }
        public Instant? DraftStartedTimestamp { get; }

        public LeagueYearKey Key => new LeagueYearKey(League.LeagueID, Year);

        public string GetGroupName => $"{League.LeagueID}|{Year}";

        public MasterGameWithEligibilityFactors GetEligibilityFactorsForMasterGame(MasterGame masterGame)
        {
            bool? eligibilityOverride = GetOverriddenEligibility(masterGame);
            IReadOnlyList<MasterGameTag> tagOverrides = GetOverriddenTags(masterGame);
            return new MasterGameWithEligibilityFactors(masterGame, Options, eligibilityOverride, tagOverrides);
        }

        public Maybe<MasterGameWithEligibilityFactors> GetEligibilityFactorsForSlot(PublisherSlot publisherSlot)
        {
            if (publisherSlot.PublisherGame.HasNoValue || publisherSlot.PublisherGame.Value.MasterGame.HasNoValue)
            {
                return Maybe<MasterGameWithEligibilityFactors>.None;
            }

            var masterGame = publisherSlot.PublisherGame.Value.MasterGame.Value.MasterGame;
            bool? eligibilityOverride = GetOverriddenEligibility(masterGame);
            IReadOnlyList<MasterGameTag> tagOverrides = GetOverriddenTags(masterGame);
            return new MasterGameWithEligibilityFactors(publisherSlot.PublisherGame.Value.MasterGame.Value.MasterGame, Options, eligibilityOverride, tagOverrides);
        }

        public bool GameIsEligibleInAnySlot(MasterGame masterGame)
        {
            var eligibilityFactors = GetEligibilityFactorsForMasterGame(masterGame);
            return SlotEligibilityService.GameIsEligibleInLeagueYear(eligibilityFactors);
        }

        private bool? GetOverriddenEligibility(MasterGame masterGame)
        {
            bool found = _eligibilityOverridesDictionary.TryGetValue(masterGame, out var eligibilityOverride);
            if (!found)
            {
                return null;
            }

            return eligibilityOverride.Eligible;
        }

        private IReadOnlyList<MasterGameTag> GetOverriddenTags(MasterGame masterGame)
        {
            bool found = _tagOverridesDictionary.TryGetValue(masterGame, out var tagOverride);
            if (!found)
            {
                return new List<MasterGameTag>();
            }

            return tagOverride.Tags;
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
