using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using FantasyCritic.Lib.Domain.Results;

namespace FantasyCritic.Lib.Domain
{
    public class MasterGameEligibilityFactors
    {
        public MasterGameEligibilityFactors(LeagueOptions options, Maybe<PublisherSlot> publisherSlot, Maybe<MasterGame> masterGame, bool? overridenEligibility, IReadOnlyList<MasterGameTag> tagOverrides)
        {
            Options = options;
            PublisherSlot = publisherSlot;
            MasterGame = masterGame;
            OverridenEligibility = overridenEligibility;
            TagOverrides = tagOverrides;
        }

        public LeagueOptions Options { get; }
        public Maybe<PublisherSlot> PublisherSlot { get; }
        public Maybe<MasterGame> MasterGame { get; }
        public bool? OverridenEligibility { get; }
        public IReadOnlyList<MasterGameTag> TagOverrides { get; }

        public IReadOnlyList<ClaimError> GetErrors()
        {
            throw new NotImplementedException();
            //if (OverridenEligibility.HasValue && OverridenEligibility.Value)
            //{
            //    return true;
            //}

            //var claimErrors = Options.LeagueTags.GameHasValidTags(masterGame, overriddenTags);
            //return !claimErrors.Any();
        }

        public bool IsEligible() => !GetErrors().Any();
    }
}