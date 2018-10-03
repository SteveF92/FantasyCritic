using System;
using System.Collections.Generic;
using System.Text;
using FantasyCritic.Lib.Domain.ScoringSystems;
using FantasyCritic.Lib.Enums;

namespace FantasyCritic.Lib.Domain
{
    public class EditLeagueYearParameters
    {
        public EditLeagueYearParameters(FantasyCriticUser manager, Guid leagueID, int year, int draftGames, int waiverGames, int counterPicks, 
            decimal estimatedCriticScore, EligibilityLevel maximumEligibilityLevel, bool allowYearlyInstallments, 
            bool allowEarlyAccess, DraftSystem draftSystem, WaiverSystem waiverSystem, ScoringSystem scoringSystem)
        {
            Manager = manager;
            LeagueID = leagueID;
            Year = year;
            DraftGames = draftGames;
            WaiverGames = waiverGames;
            CounterPicks = counterPicks;
            EstimatedCriticScore = estimatedCriticScore;
            AllowYearlyInstallments = allowYearlyInstallments;
            AllowEarlyAccess = allowEarlyAccess;
            MaximumEligibilityLevel = maximumEligibilityLevel;
            DraftSystem = draftSystem;
            WaiverSystem = waiverSystem;
            ScoringSystem = scoringSystem;
        }

        public FantasyCriticUser Manager { get; }
        public Guid LeagueID { get; }
        public int Year { get; }
        public int DraftGames { get; }
        public int WaiverGames { get; }
        public int CounterPicks { get; }
        public decimal EstimatedCriticScore { get; }
        public EligibilityLevel MaximumEligibilityLevel { get; }
        public bool AllowYearlyInstallments { get; set; }
        public bool AllowEarlyAccess { get; set; }
        public DraftSystem DraftSystem { get; }
        public WaiverSystem WaiverSystem { get; }
        public ScoringSystem ScoringSystem { get; }
    }
}
