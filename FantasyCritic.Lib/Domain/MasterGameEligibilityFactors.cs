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
        public MasterGameEligibilityFactors(LeagueOptions options, Maybe<MasterGame> masterGame, bool? overridenEligibility, IReadOnlyList<MasterGameTag> tagOverrides)
        {
            Options = options;
            MasterGame = masterGame;
            OverridenEligibility = overridenEligibility;
            TagOverrides = tagOverrides;
        }

        public LeagueOptions Options { get; }
        public Maybe<MasterGame> MasterGame { get; }
        public bool? OverridenEligibility { get; }
        public IReadOnlyList<MasterGameTag> TagOverrides { get; }
    }
}