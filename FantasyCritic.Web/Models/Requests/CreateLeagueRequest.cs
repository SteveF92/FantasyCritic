using System.ComponentModel.DataAnnotations;
using FantasyCritic.Lib.Domain;
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
        public int WaiverGames { get; set; }
        [Required]
        public int AntiPicks { get; set; }
        [Required]
        public decimal EstimatedGameScore { get; set; }
        [Required]
        public int InitialYear { get; set; }
        [Required]
        public string EligibilitySystem { get; set; }
        [Required]
        public string DraftSystem { get; set; }
        [Required]
        public string WaiverSystem { get; set; }
        [Required]
        public string ScoringSystem { get; set; }

        public LeagueCreationParameters ToDomain(FantasyCriticUser manager)
        {
            EligibilitySystem eligibilitySystem = Lib.Enums.EligibilitySystem.FromValue(EligibilitySystem);
            DraftSystem draftSystem = Lib.Enums.DraftSystem.FromValue(DraftSystem);
            WaiverSystem waiverSystem = Lib.Enums.WaiverSystem.FromValue(WaiverSystem);
            ScoringSystem scoringSystem = Lib.Enums.ScoringSystem.FromValue(ScoringSystem);

            LeagueCreationParameters parameters = new LeagueCreationParameters(manager, LeagueName, DraftGames, WaiverGames, AntiPicks,
                EstimatedGameScore, InitialYear, eligibilitySystem, draftSystem, waiverSystem, scoringSystem);
            return parameters;
        }
    }
}
