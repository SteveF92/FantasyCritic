using FantasyCritic.Lib.Domain.ScoringSystems;
using FantasyCritic.Lib.Enums;

namespace FantasyCritic.Lib.Domain.Requests
{
    public class LeagueYearAddParameters
    {
        public LeagueYearAddParameters(League league, int draftGames, int acquisitionGames, int counterPicks, decimal estimatedCriticScore, int year,
            int eligibilityLevel, DraftSystem draftSystem, AcquisitionSystem acquisitionSystem, ScoringSystem scoringSystem)
        {
            League = league;
            DraftGames = draftGames;
            AcquisitionGames = acquisitionGames;
            CounterPicks = counterPicks;
            EstimatedCriticScore = estimatedCriticScore;
            Year = year;
            EligibilityLevel = eligibilityLevel;
            DraftSystem = draftSystem;
            AcquisitionSystem = acquisitionSystem;
            ScoringSystem = scoringSystem;
        }

        public League League { get; }
        public int DraftGames { get; }
        public int AcquisitionGames { get; }
        public int CounterPicks { get; }
        public decimal EstimatedCriticScore { get; }
        public int Year { get; }
        public int EligibilityLevel { get; }
        public DraftSystem DraftSystem { get; }
        public AcquisitionSystem AcquisitionSystem { get; }
        public ScoringSystem ScoringSystem { get; }
    }
}
