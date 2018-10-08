using FantasyCritic.Lib.Domain.ScoringSystems;
using FantasyCritic.Lib.Enums;

namespace FantasyCritic.Lib.Domain.Requests
{
    public class LeagueCreationParameters
    {
        public LeagueCreationParameters(FantasyCriticUser manager, string leagueName, int draftGames, int acquisitionGames, int counterPicks, 
            decimal estimatedCriticScore, int initialYear, EligibilityLevel maximumEligibilityLevel, bool allowYearlyInstallments, 
            bool allowEarlyAccess, DraftSystem draftSystem, AcquisitionSystem acquisitionSystem, ScoringSystem scoringSystem)
        {
            Manager = manager;
            LeagueName = leagueName;
            DraftGames = draftGames;
            AcquisitionGames = acquisitionGames;
            CounterPicks = counterPicks;
            EstimatedCriticScore = estimatedCriticScore;
            InitialYear = initialYear;
            AllowYearlyInstallments = allowYearlyInstallments;
            AllowEarlyAccess = allowEarlyAccess;
            MaximumEligibilityLevel = maximumEligibilityLevel;
            DraftSystem = draftSystem;
            AcquisitionSystem = acquisitionSystem;
            ScoringSystem = scoringSystem;
        }

        public FantasyCriticUser Manager { get; }
        public string LeagueName { get; }
        public int DraftGames { get; }
        public int AcquisitionGames { get; }
        public int CounterPicks { get; }
        public decimal EstimatedCriticScore { get; }
        public int InitialYear { get; }
        public EligibilityLevel MaximumEligibilityLevel { get; }
        public bool AllowYearlyInstallments { get; set; }
        public bool AllowEarlyAccess { get; set; }
        public DraftSystem DraftSystem { get; }
        public AcquisitionSystem AcquisitionSystem { get; }
        public ScoringSystem ScoringSystem { get; }
    }
}
