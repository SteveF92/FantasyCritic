using System;
using FantasyCritic.Lib.Domain.ScoringSystems;
using FantasyCritic.Lib.Enums;

namespace FantasyCritic.Lib.Domain.Requests
{
    public class EditLeagueYearParameters
    {
        public EditLeagueYearParameters(FantasyCriticUser manager, Guid leagueID, int year, int draftGames, int acquisitionGames, int counterPicks, 
            decimal estimatedCriticScore, EligibilityLevel maximumEligibilityLevel, bool allowYearlyInstallments, 
            bool allowEarlyAccess, DraftSystem draftSystem, AcquisitionSystem acquisitionSystem, ScoringSystem scoringSystem)
        {
            Manager = manager;
            LeagueID = leagueID;
            Year = year;
            DraftGames = draftGames;
            AcquisitionGames = acquisitionGames;
            CounterPicks = counterPicks;
            EstimatedCriticScore = estimatedCriticScore;
            AllowYearlyInstallments = allowYearlyInstallments;
            AllowEarlyAccess = allowEarlyAccess;
            MaximumEligibilityLevel = maximumEligibilityLevel;
            DraftSystem = draftSystem;
            AcquisitionSystem = acquisitionSystem;
            ScoringSystem = scoringSystem;
        }

        public FantasyCriticUser Manager { get; }
        public Guid LeagueID { get; }
        public int Year { get; }
        public int DraftGames { get; }
        public int AcquisitionGames { get; }
        public int CounterPicks { get; }
        public decimal EstimatedCriticScore { get; }
        public EligibilityLevel MaximumEligibilityLevel { get; }
        public bool AllowYearlyInstallments { get; set; }
        public bool AllowEarlyAccess { get; set; }
        public DraftSystem DraftSystem { get; }
        public AcquisitionSystem AcquisitionSystem { get; }
        public ScoringSystem ScoringSystem { get; }
    }
}
