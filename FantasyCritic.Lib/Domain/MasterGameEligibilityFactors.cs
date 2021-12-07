using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using FantasyCritic.Lib.Domain.Results;
using FantasyCritic.Lib.Enums;

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

        public IReadOnlyList<ClaimError> GetErrors(bool counterPicking = false, bool dropping = false)
        {
            List<ClaimError> claimErrors = new List<ClaimError>();
            if (counterPicking || dropping)
            {
                return claimErrors;
            }

            if (MasterGame.HasNoValue)
            {
                return claimErrors;
            }

            if (OverridenEligibility.HasValue && !OverridenEligibility.Value)
            {
                claimErrors.Add(new ClaimError("That game has been specifically banned by your league.", false));
                return claimErrors;
            }

            var masterGameTagsToUse = MasterGame.Value.Tags;
            if (TagOverrides.Any())
            {
                masterGameTagsToUse = TagOverrides;
            }

            var slotTagsToUse = Options.LeagueTags;
            if (PublisherSlot.HasValue && PublisherSlot.Value.SpecialGameSlot.HasValue)
            {
                slotTagsToUse = PublisherSlot.Value.SpecialGameSlot.Value.Tags.Select(x => new LeagueTagStatus(x, TagStatus.Required)).ToList();
            }

            var tagErrors = slotTagsToUse.GameHasValidTags(masterGameTagsToUse);
            claimErrors.AddRange(tagErrors);
            return claimErrors;
        }

        public bool IsEligible() => !GetErrors().Any();
    }
}