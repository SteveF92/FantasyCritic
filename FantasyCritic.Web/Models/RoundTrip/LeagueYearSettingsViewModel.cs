using System;
using System.ComponentModel.DataAnnotations;
using FantasyCritic.Lib.Domain;
using FantasyCritic.Lib.Domain.Requests;
using FantasyCritic.Lib.Domain.ScoringSystems;
using FantasyCritic.Lib.Enums;

namespace FantasyCritic.Web.Models.Requests
{
    public class LeagueYearSettingsViewModel
    {
        public LeagueYearSettingsViewModel()
        {

        }

        public LeagueYearSettingsViewModel(League league, LeagueYear leagueYear)
        {
            LeagueID = league.LeagueID;
            Year = leagueYear.Year;
            LeagueName = league.LeagueName;
            StandardGames = leagueYear.Options.StandardGames;
            DraftGames = leagueYear.Options.DraftGames;
            CounterPicks = leagueYear.Options.CounterPicks;
            EstimatedCriticScore = leagueYear.Options.EstimatedCriticScore;
            MaximumEligibilityLevel = leagueYear.Options.MaximumEligibilityLevel.Level;
            AllowYearlyInstallments = leagueYear.Options.AllowYearlyInstallments;
            AllowEarlyAccess = leagueYear.Options.AllowEarlyAccess;
            DraftSystem = leagueYear.Options.DraftSystem.Value;
            PickupSystem = leagueYear.Options.PickupSystem.Value;
            ScoringSystem = leagueYear.Options.ScoringSystem.Name;
        }

        [Required]
        public Guid LeagueID { get; set; }
        [Required]
        public int Year { get; set; }
        [Required]
        public string LeagueName { get; set; }
        [Required]
        public int StandardGames { get; set; }
        [Required]
        public int DraftGames { get; set; }
        [Required]
        public int PickupGames { get; set; }
        [Required]
        public int CounterPicks { get; set; }
        [Required]
        public decimal EstimatedCriticScore { get; set; }
        [Required]
        public int MaximumEligibilityLevel { get; set; }
        [Required]
        public bool AllowYearlyInstallments { get; set; }
        [Required]
        public bool AllowEarlyAccess { get; set; }
        [Required]
        public string DraftSystem { get; set; }
        [Required]
        public string PickupSystem { get; set; }
        [Required]
        public string ScoringSystem { get; set; }

        public EditLeagueYearParameters ToDomain(FantasyCriticUser manager, EligibilityLevel maximumEligibilityLevel)
        {
            DraftSystem draftSystem = Lib.Enums.DraftSystem.FromValue(DraftSystem);
            PickupSystem pickupSystem = Lib.Enums.PickupSystem.FromValue(PickupSystem);
            ScoringSystem scoringSystem = Lib.Domain.ScoringSystems.ScoringSystem.GetScoringSystem(ScoringSystem);

            EditLeagueYearParameters parameters = new EditLeagueYearParameters(manager, LeagueID, Year, DraftGames, PickupGames, CounterPicks,
                EstimatedCriticScore, maximumEligibilityLevel, AllowYearlyInstallments, AllowEarlyAccess, draftSystem, pickupSystem, scoringSystem);
            return parameters;
        }
    }
}
