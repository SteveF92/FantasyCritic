using System.ComponentModel.DataAnnotations;
using FantasyCritic.Lib.Domain;
using FantasyCritic.Lib.Domain.Requests;
using FantasyCritic.Lib.Domain.ScoringSystems;
using FantasyCritic.Lib.Enums;

namespace FantasyCritic.Web.Models.Requests.LeagueManager
{
    public class CreateLeagueRequest
    {
        [Required]
        public string LeagueName { get; set; }

        [Required]
        [Range(1,30)]
        public int StandardGames { get; set; }
        [Required]
        [Range(1,30)]
        public int GamesToDraft { get; set; }
        [Required]
        [Range(0,20)]
        public int CounterPicks { get; set; }

        [Required]
        [Range(0, 30)]
        public int FreeDroppableGames { get; set; }
        [Required]
        [Range(0, 30)]
        public int WillNotReleaseDroppableGames { get; set; }
        [Required]
        public bool UnlimitedFreeDroppableGames { get; set; }
        [Required]
        public bool UnlimitedWillNotReleaseDroppableGames { get; set; }

        [Required]
        public int InitialYear { get; set; }
        [Required]
        public int MaximumEligibilityLevel { get; set; }
        [Required]
        public bool AllowYearlyInstallments { get; set; }
        [Required]
        public bool AllowEarlyAccess { get; set; }
        [Required]
        public bool AllowFreeToPlay { get; set; }
        [Required]
        public bool AllowReleasedInternationally { get; set; }
        [Required]
        public bool AllowExpansions { get; set; }

        [Required]
        public string DraftSystem { get; set; }
        [Required]
        public string PickupSystem { get; set; }
        [Required]
        public string ScoringSystem { get; set; }
        [Required]
        public bool PublicLeague { get; set; }
        [Required]
        public bool TestLeague { get; set; }

        //Don't need this once 2019 is no longer create-able.
        public bool ValidForOldYears()
        {
            if (InitialYear > 2019)
            {
                return true;
            }

            if (FreeDroppableGames != 0 || UnlimitedFreeDroppableGames)
            {
                return false;
            }

            if (WillNotReleaseDroppableGames != 0 || UnlimitedWillNotReleaseDroppableGames)
            {
                return false;
            }

            return true;
        }

        public bool IsValid()
        {
            if (UnlimitedFreeDroppableGames && UnlimitedFreeDroppableGames)
            {
                return true;
            }

            int freeDroppableGamesCompare = FreeDroppableGames;
            if (UnlimitedFreeDroppableGames)
            {
                freeDroppableGamesCompare = int.MaxValue;
            }

            int willNotReleaseDroppableGames = WillNotReleaseDroppableGames;
            if (UnlimitedWillNotReleaseDroppableGames)
            {
                willNotReleaseDroppableGames = int.MaxValue;
            }

            return freeDroppableGamesCompare <= willNotReleaseDroppableGames;
        }

        public LeagueCreationParameters ToDomain(FantasyCriticUser manager, EligibilityLevel maximumEligibilityLevel)
        {
            DraftSystem draftSystem = Lib.Enums.DraftSystem.FromValue(DraftSystem);
            PickupSystem pickupSystem = Lib.Enums.PickupSystem.FromValue(PickupSystem);
            ScoringSystem scoringSystem = Lib.Domain.ScoringSystems.ScoringSystem.GetScoringSystem(ScoringSystem);

            int freeDroppableGames = FreeDroppableGames;
            if (UnlimitedFreeDroppableGames)
            {
                freeDroppableGames = -1;
            }
            int willNotReleaseDroppableGames = WillNotReleaseDroppableGames;
            if (UnlimitedWillNotReleaseDroppableGames)
            {
                willNotReleaseDroppableGames = -1;
            }

            LeagueCreationParameters parameters = new LeagueCreationParameters(manager, LeagueName, StandardGames, GamesToDraft, CounterPicks,
                freeDroppableGames, willNotReleaseDroppableGames, InitialYear, maximumEligibilityLevel, AllowYearlyInstallments, AllowEarlyAccess,
                AllowFreeToPlay, AllowReleasedInternationally, AllowExpansions, draftSystem, pickupSystem, scoringSystem, PublicLeague, TestLeague);
            return parameters;
        }
    }
}
