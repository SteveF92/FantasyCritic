using System;
using System.ComponentModel.DataAnnotations;
using FantasyCritic.Lib.Domain;
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
            DraftGames = leagueYear.Options.DraftGames;
            AcquisitionGames = leagueYear.Options.AcquisitionGames;
            CounterPicks = leagueYear.Options.CounterPicks;
            EstimatedCriticScore = leagueYear.Options.EstimatedCriticScore;
            MaximumEligibilityLevel = leagueYear.Options.MaximumEligibilityLevel.Level;
            AllowYearlyInstallments = leagueYear.Options.AllowYearlyInstallments;
            AllowEarlyAccess = leagueYear.Options.AllowEarlyAccess;
            DraftSystem = leagueYear.Options.DraftSystem.Value;
            AcquisitionSystem = leagueYear.Options.AcquisitionSystem.Value;
            ScoringSystem = leagueYear.Options.ScoringSystem.Name;
        }

        [Required]
        public Guid LeagueID { get; set; }
        [Required]
        public int Year { get; set; }
        [Required]
        public string LeagueName { get; set; }
        [Required]
        public int DraftGames { get; set; }
        [Required]
        public int AcquisitionGames { get; set; }
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
        public string AcquisitionSystem { get; set; }
        [Required]
        public string ScoringSystem { get; set; }

        public EditLeagueYearParameters ToDomain(FantasyCriticUser manager, EligibilityLevel maximumEligibilityLevel)
        {
            DraftSystem draftSystem = Lib.Enums.DraftSystem.FromValue(DraftSystem);
            AcquisitionSystem acquisitionSystem = Lib.Enums.AcquisitionSystem.FromValue(AcquisitionSystem);
            ScoringSystem scoringSystem = Lib.Domain.ScoringSystems.ScoringSystem.GetScoringSystem(ScoringSystem);

            EditLeagueYearParameters parameters = new EditLeagueYearParameters(manager, LeagueID, Year, DraftGames, AcquisitionGames, CounterPicks,
                EstimatedCriticScore, maximumEligibilityLevel, AllowYearlyInstallments, AllowEarlyAccess, draftSystem, acquisitionSystem, scoringSystem);
            return parameters;
        }
    }
}
