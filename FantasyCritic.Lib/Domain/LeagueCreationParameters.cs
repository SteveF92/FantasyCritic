using System;
using System.Collections.Generic;
using System.Text;
using FantasyCritic.Lib.Domain.ScoringSystems;
using FantasyCritic.Lib.Enums;

namespace FantasyCritic.Lib.Domain
{
    public class LeagueCreationParameters
    {
        public LeagueCreationParameters(FantasyCriticUser manager, string leagueName, int draftGames, int waiverGames, int counterPicks, 
            decimal estimatedCriticScore, int initialYear, EligibilityLevel maximumEligibilityLevel, bool allowYearlyInstallments, 
            bool allowEarlyAccess, DraftSystem draftSystem, WaiverSystem waiverSystem, ScoringSystem scoringSystem)
        {
            Manager = manager;
            LeagueName = leagueName;
            DraftGames = draftGames;
            WaiverGames = waiverGames;
            CounterPicks = counterPicks;
            EstimatedCriticScore = estimatedCriticScore;
            InitialYear = initialYear;
            AllowYearlyInstallments = allowYearlyInstallments;
            AllowEarlyAccess = allowEarlyAccess;
            MaximumEligibilityLevel = maximumEligibilityLevel;
            DraftSystem = draftSystem;
            WaiverSystem = waiverSystem;
            ScoringSystem = scoringSystem;
        }

        public FantasyCriticUser Manager { get; }
        public string LeagueName { get; }
        public int DraftGames { get; }
        public int WaiverGames { get; }
        public int CounterPicks { get; }
        public decimal EstimatedCriticScore { get; }
        public int InitialYear { get; }
        public EligibilityLevel MaximumEligibilityLevel { get; }
        public bool AllowYearlyInstallments { get; set; }
        public bool AllowEarlyAccess { get; set; }
        public DraftSystem DraftSystem { get; }
        public WaiverSystem WaiverSystem { get; }
        public ScoringSystem ScoringSystem { get; }
    }
}
