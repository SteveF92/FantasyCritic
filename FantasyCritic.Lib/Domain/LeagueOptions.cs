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
        public LeagueOptions(int standardGames, int gamesToDraft, int counterPicks, int freeDroppableGames, int willNotReleaseDroppableGames, int willReleaseDroppableGames,
            bool dropOnlyDraftGames, EligibilitySettings eligibilitySettings, DraftSystem draftSystem, PickupSystem pickupSystem, ScoringSystem scoringSystem, bool publicLeague)
        {
            StandardGames = standardGames;
            GamesToDraft = gamesToDraft;
            CounterPicks = counterPicks;
            FreeDroppableGames = freeDroppableGames;
            WillNotReleaseDroppableGames = willNotReleaseDroppableGames;
            WillReleaseDroppableGames = willReleaseDroppableGames;
            DropOnlyDraftGames = dropOnlyDraftGames;
            AllowedEligibilitySettings = eligibilitySettings;
            DraftSystem = draftSystem;
            PickupSystem = pickupSystem;
            ScoringSystem = scoringSystem;
            PublicLeague = publicLeague;
        }

        public LeagueOptions(LeagueCreationParameters parameters)
        {
            StandardGames = parameters.StandardGames;
            GamesToDraft = parameters.GamesToDraft;
            CounterPicks = parameters.CounterPicks;
            FreeDroppableGames = parameters.FreeDroppableGames;
            WillNotReleaseDroppableGames = parameters.WillNotReleaseDroppableGames;
            WillReleaseDroppableGames = parameters.WillReleaseDroppableGames;
            DropOnlyDraftGames = parameters.DropOnlyDraftGames;
            AllowedEligibilitySettings = parameters.AllowedEligibilitySettings;
            DraftSystem = parameters.DraftSystem;
            PickupSystem = parameters.PickupSystem;
            ScoringSystem = parameters.ScoringSystem;
            PublicLeague = parameters.PublicLeague;
        }

        public LeagueOptions(EditLeagueYearParameters parameters)
        {
            StandardGames = parameters.StandardGames;
            GamesToDraft = parameters.GamesToDraft;
            CounterPicks = parameters.CounterPicks;
            FreeDroppableGames = parameters.FreeDroppableGames;
            WillNotReleaseDroppableGames = parameters.WillNotReleaseDroppableGames;
            WillReleaseDroppableGames = parameters.WillReleaseDroppableGames;
            DropOnlyDraftGames = parameters.DropOnlyDraftGames;
            AllowedEligibilitySettings = parameters.AllowedEligibilitySettings;
            DraftSystem = parameters.DraftSystem;
            PickupSystem = parameters.PickupSystem;
            ScoringSystem = parameters.ScoringSystem;
            PublicLeague = parameters.PublicLeague;
        }

        public int StandardGames { get; }
        public int GamesToDraft { get; }
        public int CounterPicks { get; }
        public int FreeDroppableGames { get; }
        public int WillNotReleaseDroppableGames { get; }
        public int WillReleaseDroppableGames { get; }
        public bool DropOnlyDraftGames { get; }
        public EligibilitySettings AllowedEligibilitySettings { get; }
        public DraftSystem DraftSystem { get; }
        public PickupSystem PickupSystem { get; }
        public ScoringSystem ScoringSystem { get; }
        public bool PublicLeague { get; }

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
