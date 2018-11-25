using System.ComponentModel.DataAnnotations;
using FantasyCritic.Lib.Domain;
using FantasyCritic.Lib.Domain.Requests;
using FantasyCritic.Lib.Domain.ScoringSystems;
using FantasyCritic.Lib.Enums;

namespace FantasyCritic.Web.Models.Requests
{
    public class CreateLeagueRequest
    {
        [Required]
        public string LeagueName { get; set; }

        [Required]
        [Range(1,20)]
        public int StandardGames { get; set; }
        [Required]
        [Range(1,20)]
        public int GamesToDraft { get; set; }
        [Required]
        [Range(1,20)]
        public int CounterPicks { get; set; }

        [Required]
        public decimal EstimatedCriticScore { get; set; }
        [Required]
        public int InitialYear { get; set; }
        [Required]
        public int MaximumEligibilityLevel { get; set; }
        [Required]
        public bool AllowYearlyInstallments { get; set; }
        [Required]
        public bool AllowEarlyAccess { get; set; }
        [Required]
        public string DraftSystem { get; set; }
        [Required]
        public string PickupSystem { get; set; }
        [Required]
        public string ScoringSystem { get; set; }

        public LeagueCreationParameters ToDomain(FantasyCriticUser manager, EligibilityLevel maximumEligibilityLevel)
        {
            DraftSystem draftSystem = Lib.Enums.DraftSystem.FromValue(DraftSystem);
            PickupSystem pickupSystem = Lib.Enums.PickupSystem.FromValue(PickupSystem);
            ScoringSystem scoringSystem = Lib.Domain.ScoringSystems.ScoringSystem.GetScoringSystem(ScoringSystem);

            LeagueCreationParameters parameters = new LeagueCreationParameters(manager, LeagueName, StandardGames, GamesToDraft, CounterPicks,
                InitialYear, maximumEligibilityLevel, AllowYearlyInstallments, AllowEarlyAccess, draftSystem, pickupSystem, scoringSystem);
            return parameters;
        }
    }
}
