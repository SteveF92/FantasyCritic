using FantasyCritic.Lib.Domain.ScoringSystems;
using FantasyCritic.Lib.Enums;

namespace FantasyCritic.Lib.Domain.Requests
{
    public class LeagueCreationParameters
    {
        public LeagueCreationParameters(FantasyCriticUser manager, string leagueName, int draftGames, int pickupGames, int counterPicks, 
            decimal estimatedCriticScore, int initialYear, EligibilityLevel maximumEligibilityLevel, bool allowYearlyInstallments, 
            bool allowEarlyAccess, DraftSystem draftSystem, PickupSystem pickupSystem, ScoringSystem scoringSystem)
        {
            Manager = manager;
            LeagueName = leagueName;
            DraftGames = draftGames;
            PickupGames = pickupGames;
            CounterPicks = counterPicks;
            EstimatedCriticScore = estimatedCriticScore;
            InitialYear = initialYear;
            AllowYearlyInstallments = allowYearlyInstallments;
            AllowEarlyAccess = allowEarlyAccess;
            MaximumEligibilityLevel = maximumEligibilityLevel;
            DraftSystem = draftSystem;
            PickupSystem = pickupSystem;
            ScoringSystem = scoringSystem;
        }

        public FantasyCriticUser Manager { get; }
        public string LeagueName { get; }
        public int DraftGames { get; }
        public int PickupGames { get; }
        public int CounterPicks { get; }
        public decimal EstimatedCriticScore { get; }
        public int InitialYear { get; }
        public EligibilityLevel MaximumEligibilityLevel { get; }
        public bool AllowYearlyInstallments { get; set; }
        public bool AllowEarlyAccess { get; set; }
        public DraftSystem DraftSystem { get; }
        public PickupSystem PickupSystem { get; }
        public ScoringSystem ScoringSystem { get; }
    }
}
