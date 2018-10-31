using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FantasyCritic.Lib.Domain;
using FantasyCritic.Lib.Domain.ScoringSystems;
using FantasyCritic.Lib.Enums;

namespace FantasyCritic.MySQL.Entities
{
    internal class LeagueYearEntity
    {
        public LeagueYearEntity()
        {

        }

        public LeagueYearEntity(League league, int year, LeagueOptions options, bool playStarted)
        {
            LeagueID = league.LeagueID;
            Year = year;

            DraftGames = options.DraftGames;
            PickupGames = options.PickupGames;
            CounterPicks = options.CounterPicks;
            EstimatedCriticScore = options.EstimatedCriticScore;

            MaximumEligibilityLevel = options.MaximumEligibilityLevel.Level;
            AllowYearlyInstallments = options.AllowYearlyInstallments;
            AllowEarlyAccess = options.AllowEarlyAccess;
            DraftSystem = options.DraftSystem.Value;
            PickupSystem = options.PickupSystem.Value;
            ScoringSystem = options.ScoringSystem.Name;
            PlayStarted = playStarted;
        }

        public Guid LeagueID { get; set; }
        public int Year { get; set; }
        public int DraftGames { get; set; }
        public int PickupGames { get; set; }
        public int CounterPicks { get; set; }
        public decimal EstimatedCriticScore { get; set; }
        public int MaximumEligibilityLevel { get; set; }
        public bool AllowYearlyInstallments { get; set; }
        public bool AllowEarlyAccess { get; set; }
        public string DraftSystem { get; set; }
        public string PickupSystem { get; set; }
        public string ScoringSystem { get; set; }
        public bool PlayStarted { get; set; }

        public LeagueYear ToDomain(League league, EligibilityLevel maximumEligibilityLevel)
        {
            DraftSystem draftSystem = Lib.Enums.DraftSystem.FromValue(DraftSystem);
            PickupSystem pickupSystem = Lib.Enums.PickupSystem.FromValue(PickupSystem);
            ScoringSystem scoringSystem = Lib.Domain.ScoringSystems.ScoringSystem.GetScoringSystem(ScoringSystem);

            LeagueOptions options = new LeagueOptions(DraftGames, PickupGames, CounterPicks, EstimatedCriticScore,
                maximumEligibilityLevel, AllowYearlyInstallments, AllowEarlyAccess, draftSystem, pickupSystem, scoringSystem);

            return new LeagueYear(league, Year, options, PlayStarted);
        }
    }
}
