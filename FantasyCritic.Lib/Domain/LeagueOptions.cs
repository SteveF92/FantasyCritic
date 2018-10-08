using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FantasyCritic.Lib.Domain.Requests;
using FantasyCritic.Lib.Domain.ScoringSystems;
using FantasyCritic.Lib.Enums;

namespace FantasyCritic.Lib.Domain
{
    public class LeagueOptions
    {
        public LeagueOptions(int draftGames, int acquisitionGames, int counterPicks, decimal estimatedCriticScore, 
            EligibilityLevel maximumEligibilityLevel, bool allowYearlyInstallments, bool allowEarlyAccess,
            DraftSystem draftSystem, AcquisitionSystem acquisitionSystem, ScoringSystem scoringSystem)
        {
            DraftGames = draftGames;
            AcquisitionGames = acquisitionGames;
            CounterPicks = counterPicks;
            EstimatedCriticScore = estimatedCriticScore;
            MaximumEligibilityLevel = maximumEligibilityLevel;
            AllowYearlyInstallments = allowYearlyInstallments;
            AllowEarlyAccess = allowEarlyAccess;
            DraftSystem = draftSystem;
            AcquisitionSystem = acquisitionSystem;
            ScoringSystem = scoringSystem;
        }

        public LeagueOptions(LeagueCreationParameters parameters)
        {
            DraftGames = parameters.DraftGames;
            AcquisitionGames = parameters.AcquisitionGames;
            CounterPicks = parameters.CounterPicks;
            EstimatedCriticScore = parameters.EstimatedCriticScore;
            MaximumEligibilityLevel = parameters.MaximumEligibilityLevel;
            AllowYearlyInstallments = parameters.AllowYearlyInstallments;
            AllowEarlyAccess = parameters.AllowEarlyAccess;
            DraftSystem = parameters.DraftSystem;
            AcquisitionSystem = parameters.AcquisitionSystem;
            ScoringSystem = parameters.ScoringSystem;
        }

        public LeagueOptions(EditLeagueYearParameters parameters)
        {
            DraftGames = parameters.DraftGames;
            AcquisitionGames = parameters.AcquisitionGames;
            CounterPicks = parameters.CounterPicks;
            EstimatedCriticScore = parameters.EstimatedCriticScore;
            MaximumEligibilityLevel = parameters.MaximumEligibilityLevel;
            AllowYearlyInstallments = parameters.AllowYearlyInstallments;
            AllowEarlyAccess = parameters.AllowEarlyAccess;
            DraftSystem = parameters.DraftSystem;
            AcquisitionSystem = parameters.AcquisitionSystem;
            ScoringSystem = parameters.ScoringSystem;
        }

        public int DraftGames { get; }
        public int AcquisitionGames { get; }
        public int CounterPicks { get; }
        public decimal EstimatedCriticScore { get; }
        public bool AllowYearlyInstallments { get; }
        public bool AllowEarlyAccess { get; }
        public EligibilityLevel MaximumEligibilityLevel { get; }
        public DraftSystem DraftSystem { get; }
        public AcquisitionSystem AcquisitionSystem { get; }
        public ScoringSystem ScoringSystem { get; }
    }
}
