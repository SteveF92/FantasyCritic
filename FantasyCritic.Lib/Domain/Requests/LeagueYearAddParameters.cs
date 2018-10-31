using FantasyCritic.Lib.Domain.ScoringSystems;
using FantasyCritic.Lib.Enums;

namespace FantasyCritic.Lib.Domain.Requests
{
    public class LeagueYearAddParameters
    {
        public LeagueYearAddParameters(League league, int draftGames, int pickupGames, int counterPicks, decimal estimatedCriticScore, int year,
            int eligibilityLevel, DraftSystem draftSystem, PickupSystem pickupSystem, ScoringSystem scoringSystem)
        {
            League = league;
            DraftGames = draftGames;
            PickupGames = pickupGames;
            CounterPicks = counterPicks;
            EstimatedCriticScore = estimatedCriticScore;
            Year = year;
            EligibilityLevel = eligibilityLevel;
            DraftSystem = draftSystem;
            PickupSystem = pickupSystem;
            ScoringSystem = scoringSystem;
        }

        public League League { get; }
        public int DraftGames { get; }
        public int PickupGames { get; }
        public int CounterPicks { get; }
        public decimal EstimatedCriticScore { get; }
        public int Year { get; }
        public int EligibilityLevel { get; }
        public DraftSystem DraftSystem { get; }
        public PickupSystem PickupSystem { get; }
        public ScoringSystem ScoringSystem { get; }
    }
}
