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
        public MasterGameWithEligibilityFactors(Maybe<MasterGame> masterGame, LeagueOptions options, bool? overridenEligibility, IReadOnlyList<MasterGameTag> tagOverrides)
        {
            MasterGame = masterGame;
            Options = options;
            OverridenEligibility = overridenEligibility;
            TagOverrides = tagOverrides;
        }

        public Maybe<MasterGame> MasterGame { get; }
        public LeagueOptions Options { get; }
        public bool? OverridenEligibility { get; }
        public IReadOnlyList<MasterGameTag> TagOverrides { get; }
    }
}