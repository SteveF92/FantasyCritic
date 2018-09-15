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
        public LeagueOptions(int draftGames, int waiverGames, int counterPicks, decimal estimatedGameScore, EligibilitySystem eligibilitySystem, DraftSystem draftSystem, WaiverSystem waiverSystem, ScoringSystem scoringSystem)
        {
            DraftGames = draftGames;
            WaiverGames = waiverGames;
            CounterPicks = counterPicks;
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
            CounterPicks = parameters.CounterPicks;
            EstimatedGameScore = parameters.EstimatedGameScore;
            EligibilitySystem = parameters.EligibilitySystem;
            DraftSystem = parameters.DraftSystem;
            WaiverSystem = parameters.WaiverSystem;
            ScoringSystem = parameters.ScoringSystem;
        }

        public int DraftGames { get; }
        public int WaiverGames { get; }
        public int CounterPicks { get; }
        public decimal EstimatedGameScore { get; }
        public EligibilitySystem EligibilitySystem { get; }
        public DraftSystem DraftSystem { get; }
        public WaiverSystem WaiverSystem { get; }
        public ScoringSystem ScoringSystem { get; }
    }
}
