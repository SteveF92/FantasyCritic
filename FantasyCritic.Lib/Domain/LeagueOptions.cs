using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FantasyCritic.Lib.Domain.ScoringSystems;
using FantasyCritic.Lib.Enums;

namespace FantasyCritic.Lib.Domain
{
    public class LeagueOptions
    {
        public LeagueOptions(int draftGames, int waiverGames, int counterPicks, decimal estimatedCriticScore, EligibilityLevel maximumEligibilityLevel, DraftSystem draftSystem, WaiverSystem waiverSystem, ScoringSystem scoringSystem)
        {
            DraftGames = draftGames;
            WaiverGames = waiverGames;
            CounterPicks = counterPicks;
            EstimatedCriticScore = estimatedCriticScore;
            MaximumEligibilityLevel = maximumEligibilityLevel;
            DraftSystem = draftSystem;
            WaiverSystem = waiverSystem;
            ScoringSystem = scoringSystem;
        }

        public LeagueOptions(LeagueCreationParameters parameters)
        {
            DraftGames = parameters.DraftGames;
            WaiverGames = parameters.WaiverGames;
            CounterPicks = parameters.CounterPicks;
            EstimatedCriticScore = parameters.EstimatedCriticScore;
            MaximumEligibilityLevel = parameters.MaximumEligibilityLevel;
            DraftSystem = parameters.DraftSystem;
            WaiverSystem = parameters.WaiverSystem;
            ScoringSystem = parameters.ScoringSystem;
        }

        public int DraftGames { get; }
        public int WaiverGames { get; }
        public int CounterPicks { get; }
        public decimal EstimatedCriticScore { get; }
        public EligibilityLevel MaximumEligibilityLevel { get; }
        public DraftSystem DraftSystem { get; }
        public WaiverSystem WaiverSystem { get; }
        public ScoringSystem ScoringSystem { get; }
    }
}
