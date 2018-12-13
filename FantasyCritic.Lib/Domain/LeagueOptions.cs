using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using FantasyCritic.Lib.Domain.Requests;
using FantasyCritic.Lib.Domain.ScoringSystems;
using FantasyCritic.Lib.Enums;

namespace FantasyCritic.Lib.Domain
{
    public class LeagueOptions
    {
        public LeagueOptions(int standardGames, int gamesToDraft, int counterPicks,
            EligibilityLevel maximumEligibilityLevel, bool allowYearlyInstallments, bool allowEarlyAccess,
            DraftSystem draftSystem, PickupSystem pickupSystem, ScoringSystem scoringSystem)
        {
            StandardGames = standardGames;
            GamesToDraft = gamesToDraft;
            CounterPicks = counterPicks;
            MaximumEligibilityLevel = maximumEligibilityLevel;
            AllowYearlyInstallments = allowYearlyInstallments;
            AllowEarlyAccess = allowEarlyAccess;
            DraftSystem = draftSystem;
            PickupSystem = pickupSystem;
            ScoringSystem = scoringSystem;
        }

        public LeagueOptions(LeagueCreationParameters parameters)
        {
            StandardGames = parameters.StandardGames;
            GamesToDraft = parameters.GamesToDraft;
            CounterPicks = parameters.CounterPicks;
            MaximumEligibilityLevel = parameters.MaximumEligibilityLevel;
            AllowYearlyInstallments = parameters.AllowYearlyInstallments;
            AllowEarlyAccess = parameters.AllowEarlyAccess;
            DraftSystem = parameters.DraftSystem;
            PickupSystem = parameters.PickupSystem;
            ScoringSystem = parameters.ScoringSystem;
        }

        public LeagueOptions(EditLeagueYearParameters parameters)
        {
            StandardGames = parameters.StandardGames;
            GamesToDraft = parameters.GamesToDraft;
            CounterPicks = parameters.CounterPicks;
            MaximumEligibilityLevel = parameters.MaximumEligibilityLevel;
            AllowYearlyInstallments = parameters.AllowYearlyInstallments;
            AllowEarlyAccess = parameters.AllowEarlyAccess;
            DraftSystem = parameters.DraftSystem;
            PickupSystem = parameters.PickupSystem;
            ScoringSystem = parameters.ScoringSystem;
        }

        public int StandardGames { get; }
        public int GamesToDraft { get; }
        public int CounterPicks { get; }
        public bool AllowYearlyInstallments { get; }
        public bool AllowEarlyAccess { get; }
        public EligibilityLevel MaximumEligibilityLevel { get; }
        public DraftSystem DraftSystem { get; }
        public PickupSystem PickupSystem { get; }
        public ScoringSystem ScoringSystem { get; }

        public Result Validate()
        {
            if (GamesToDraft > StandardGames)
            {
                return Result.Fail("Can't draft more than the total number of standard games.");
            }

            if (CounterPicks > GamesToDraft)
            {
                return Result.Fail("Can't have more counter picks than drafted games.");
            }

            return Result.Ok();
        }
    }
}
