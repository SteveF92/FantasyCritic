using System.ComponentModel.DataAnnotations;
using FantasyCritic.Lib.Domain;
using FantasyCritic.Lib.Domain.ScoringSystems;
using FantasyCritic.Lib.Enums;

namespace FantasyCritic.Web.Models.Requests
{
    public class CreateLeagueRequest
    {
        [Required]
        public string LeagueName { get; set; }
        [Required]
        public int DraftGames { get; set; }
        [Required]
        public int AcquisitionGames { get; set; }
        [Required]
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
        public string AcquisitionSystem { get; set; }
        [Required]
        public string ScoringSystem { get; set; }

        public LeagueCreationParameters ToDomain(FantasyCriticUser manager, EligibilityLevel maximumEligibilityLevel)
        {
            DraftSystem draftSystem = Lib.Enums.DraftSystem.FromValue(DraftSystem);
            AcquisitionSystem acquisitionSystem = Lib.Enums.AcquisitionSystem.FromValue(AcquisitionSystem);
            ScoringSystem scoringSystem = Lib.Domain.ScoringSystems.ScoringSystem.GetScoringSystem(ScoringSystem);

            LeagueCreationParameters parameters = new LeagueCreationParameters(manager, LeagueName, DraftGames, AcquisitionGames, CounterPicks,
                EstimatedCriticScore, InitialYear, maximumEligibilityLevel, AllowYearlyInstallments, AllowEarlyAccess, draftSystem, acquisitionSystem, scoringSystem);
            return parameters;
        }
    }
}
