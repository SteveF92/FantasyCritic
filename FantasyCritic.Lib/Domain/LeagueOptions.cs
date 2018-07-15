using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FantasyCritic.Lib.Enums;

namespace FantasyCritic.Lib.Domain
{
    public class LeagueOptions
    {
        public LeagueOptions(int draftGames, int waiverGames, int antiPicks, decimal estimatedGameScore, EligibilitySystem eligibilitySystem, DraftSystem draftSystem, WaiverSystem waiverSystem, ScoringSystem scoringSystem)
        {
            DraftGames = draftGames;
            WaiverGames = waiverGames;
            AntiPicks = antiPicks;
            EstimatedGameScore = estimatedGameScore;
            EligibilitySystem = eligibilitySystem;
            DraftSystem = draftSystem;
            WaiverSystem = waiverSystem;
            ScoringSystem = scoringSystem;
        }

        public LeagueOptions(LeagueCreationParameters parameters)
        {
            DraftGames = parameters.DraftGames;
            WaiverGames = parameters.WaiverGames;
            AntiPicks = parameters.AntiPicks;
            EstimatedGameScore = parameters.EstimatedGameScore;
            EligibilitySystem = parameters.EligibilitySystem;
            DraftSystem = parameters.DraftSystem;
            WaiverSystem = parameters.WaiverSystem;
            ScoringSystem = parameters.ScoringSystem;
        }

        public int DraftGames { get; }
        public int WaiverGames { get; }
        public int AntiPicks { get; }
        public decimal EstimatedGameScore { get; }
        public EligibilitySystem EligibilitySystem { get; }
        public DraftSystem DraftSystem { get; }
        public WaiverSystem WaiverSystem { get; }
        public ScoringSystem ScoringSystem { get; }
    }
}
