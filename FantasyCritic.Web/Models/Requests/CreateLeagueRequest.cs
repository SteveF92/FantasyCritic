using FantasyCritic.Lib.Domain;
using FantasyCritic.Lib.Enums;

namespace FantasyCritic.Web.Models.Requests
{
    public class CreateLeagueRequest
    {
        public string LeagueName { get; set; }
        public int RosterSize { get; set; }
        public decimal EstimatedGameScore { get; set; }
        public int InitialYear { get; set; }
        public string EligibilitySystem { get; set; }
        public string DraftSystem { get; set; }
        public string WaiverSystem { get; set; }
        public string ScoringSystem { get; set; }

        public LeagueCreationParameters ToDomain(FantasyCriticUser manager)
        {
            EligibilitySystem eligibilitySystem = Lib.Enums.EligibilitySystem.FromValue(EligibilitySystem);
            DraftSystem draftSystem = Lib.Enums.DraftSystem.FromValue(DraftSystem);
            WaiverSystem waiverSystem = Lib.Enums.WaiverSystem.FromValue(WaiverSystem);
            ScoringSystem scoringSystem = Lib.Enums.ScoringSystem.FromValue(ScoringSystem);

            LeagueCreationParameters parameters = new LeagueCreationParameters(manager, LeagueName, RosterSize,
                EstimatedGameScore, InitialYear, eligibilitySystem, draftSystem, waiverSystem, scoringSystem);
            return parameters;
        }
    }
}
