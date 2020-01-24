using System;
using System.ComponentModel.DataAnnotations;
using FantasyCritic.Lib.Domain;
using FantasyCritic.Lib.Domain.Requests;
using FantasyCritic.Lib.Domain.ScoringSystems;
using FantasyCritic.Lib.Enums;

namespace FantasyCritic.Web.Models.RoundTrip
{
    public class LeagueYearSettingsViewModel
    {
        public LeagueYearSettingsViewModel()
        {

        }

        public LeagueYearSettingsViewModel(LeagueYear leagueYear)
        {
            LeagueID = leagueYear.League.LeagueID;
            Year = leagueYear.Year;
            LeagueName = leagueYear.League.LeagueName;
            StandardGames = leagueYear.Options.StandardGames;
            GamesToDraft = leagueYear.Options.GamesToDraft;
            CounterPicks = leagueYear.Options.CounterPicks;

            FreeDroppableGames = leagueYear.Options.FreeDroppableGames;
            if (leagueYear.Options.FreeDroppableGames == -1)
            {
                FreeDroppableGames = 0;
                UnlimitedFreeDroppableGames = true;
            }
            WillNotReleaseDroppableGames = leagueYear.Options.WillNotReleaseDroppableGames;
            if (leagueYear.Options.WillNotReleaseDroppableGames == -1)
            {
                WillNotReleaseDroppableGames = 0;
                UnlimitedWillNotReleaseDroppableGames = true;
            }
            WillReleaseDroppableGames = leagueYear.Options.WillReleaseDroppableGames;
            if (leagueYear.Options.WillReleaseDroppableGames == -1)
            {
                WillReleaseDroppableGames = 0;
                UnlimitedWillReleaseDroppableGames = true;
            }
            DropOnlyDraftGames = leagueYear.Options.DropOnlyDraftGames;

            MaximumEligibilityLevel = leagueYear.Options.AllowedEligibilitySettings.EligibilityLevel.Level;
            AllowYearlyInstallments = leagueYear.Options.AllowedEligibilitySettings.YearlyInstallment;
            AllowEarlyAccess = leagueYear.Options.AllowedEligibilitySettings.EarlyAccess;
            AllowFreeToPlay = leagueYear.Options.AllowedEligibilitySettings.FreeToPlay;
            AllowReleasedInternationally = leagueYear.Options.AllowedEligibilitySettings.ReleasedInternationally;
            AllowExpansions = leagueYear.Options.AllowedEligibilitySettings.ExpansionPack;
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
        [Range(1, 50)]
        public int StandardGames { get; set; }
        [Required]
        [Range(1, 50)]
        public int GamesToDraft { get; set; }
        [Required]
        [Range(0, 20)]
        public int CounterPicks { get; set; }

        [Required]
        [Range(0, 100)]
        public int FreeDroppableGames { get; set; }
        [Required]
        [Range(0, 100)]
        public int WillNotReleaseDroppableGames { get; set; }
        [Required]
        [Range(0, 100)]
        public int WillReleaseDroppableGames { get; set; }
        [Required]
        public bool UnlimitedFreeDroppableGames { get; set; }
        [Required]
        public bool UnlimitedWillNotReleaseDroppableGames { get; set; }
        [Required]
        public bool UnlimitedWillReleaseDroppableGames { get; set; }
        [Required]
        public bool DropOnlyDraftGames { get; set; }
        [Required]
        public int MaximumEligibilityLevel { get; set; }
        [Required]
        public bool AllowYearlyInstallments { get; set; }
        [Required]
        public bool AllowEarlyAccess { get; set; }
        [Required]
        public bool AllowFreeToPlay { get; set; }
        [Required]
        public bool AllowReleasedInternationally { get; set; }
        [Required]
        public bool AllowExpansions { get; set; }

        [Required]
        public string DraftSystem { get; set; }
        [Required]
        public string PickupSystem { get; set; }
        [Required]
        public string ScoringSystem { get; set; }
        [Required]
        public bool PublicLeague { get; set; }

        //Don't need this once 2019 is no longer editable.
        public bool ValidForOldYears()
        {
            if (Year > 2019)
            {
                return true;
            }

            if (FreeDroppableGames != 0 || UnlimitedFreeDroppableGames)
            {
                return false;
            }

            if (WillNotReleaseDroppableGames != 0 || UnlimitedWillNotReleaseDroppableGames)
            {
                return false;
            }

            if (WillReleaseDroppableGames != 0 || UnlimitedWillReleaseDroppableGames)
            {
                return false;
            }

            return true;
        }

        public EditLeagueYearParameters ToDomain(FantasyCriticUser manager, EligibilityLevel maximumEligibilityLevel)
        {
            DraftSystem draftSystem = Lib.Enums.DraftSystem.FromValue(DraftSystem);
            PickupSystem pickupSystem = Lib.Enums.PickupSystem.FromValue(PickupSystem);
            ScoringSystem scoringSystem = Lib.Domain.ScoringSystems.ScoringSystem.GetScoringSystem(ScoringSystem);

            int freeDroppableGames = FreeDroppableGames;
            if (UnlimitedFreeDroppableGames)
            {
                freeDroppableGames = -1;
            }
            int willNotReleaseDroppableGames = WillNotReleaseDroppableGames;
            if (UnlimitedWillNotReleaseDroppableGames)
            {
                willNotReleaseDroppableGames = -1;
            }
            int willReleaseDroppableGames = WillReleaseDroppableGames;
            if (UnlimitedWillReleaseDroppableGames)
            {
                willReleaseDroppableGames = -1;
            }

            EditLeagueYearParameters parameters = new EditLeagueYearParameters(manager, LeagueID, Year, StandardGames, GamesToDraft, CounterPicks,
                freeDroppableGames, willNotReleaseDroppableGames, willReleaseDroppableGames, DropOnlyDraftGames, maximumEligibilityLevel, AllowYearlyInstallments,
                AllowEarlyAccess, AllowFreeToPlay, AllowReleasedInternationally, AllowExpansions, draftSystem, pickupSystem, scoringSystem, PublicLeague);
            return parameters;
        }
    }
}
