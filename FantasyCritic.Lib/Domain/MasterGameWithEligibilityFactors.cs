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
    public class MasterGameWithEligibilityFactors
    {
        public MasterGameWithEligibilityFactors(MasterGame masterGame, LeagueOptions options, bool? overridenEligibility, IReadOnlyList<MasterGameTag> tagOverrides)
        {
            MasterGame = masterGame;
            Options = options;
            OverridenEligibility = overridenEligibility;
            TagOverrides = tagOverrides;
        }

        public MasterGame MasterGame { get; }
        public LeagueOptions Options { get; }
        public bool? OverridenEligibility { get; }
        public IReadOnlyList<MasterGameTag> TagOverrides { get; }

        public bool GameIsSpecificallyAllowed => OverridenEligibility.HasValue && OverridenEligibility.Value;
        public bool GameIsSpecificallyBanned => OverridenEligibility.HasValue && !OverridenEligibility.Value;

        public IReadOnlyList<ClaimError> CheckGameAgainstTags(IEnumerable<LeagueTagStatus> leagueTags)
        {
            var tagsToUse = TagOverrides.Any() ? TagOverrides : MasterGame.Tags;
            return leagueTags.GameHasValidTags(tagsToUse);
        }
    }
}