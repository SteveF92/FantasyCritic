using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Amazon.RDS.Model;
using FantasyCritic.Lib.Domain;
using FantasyCritic.Lib.Domain.Requests;
using FantasyCritic.Lib.Domain.ScoringSystems;
using FantasyCritic.Lib.Enums;
using FantasyCritic.Lib.Identity;

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
            CounterPicksToDraft = leagueYear.Options.CounterPicksToDraft;

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
            CounterPicksBlockDrops = leagueYear.Options.CounterPicksBlockDrops;
            MinimumBidAmount = leagueYear.Options.MinimumBidAmount;

            DraftSystem = leagueYear.Options.DraftSystem.Value;
            PickupSystem = leagueYear.Options.PickupSystem.Value;
            ScoringSystem = leagueYear.Options.ScoringSystem.Name;

            var bannedTags = leagueYear.Options.LeagueTags
                .Where(x => x.Status == TagStatus.Banned)
                .Select(x => x.Tag.Name)
                .ToList();

            var requiredTags = leagueYear.Options.LeagueTags
                .Where(x => x.Status == TagStatus.Required)
                .Select(x => x.Tag.Name)
                .ToList();

            Tags = new LeagueTagOptionsViewModel()
            {
                Banned = bannedTags,
                Required = requiredTags
            };

            SpecialGameSlots = leagueYear.Options.SpecialGameSlots.Select(x => new SpecialGameSlotViewModel(x)).ToList();
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
        [Range(0, 5)]
        public int CounterPicks { get; set; }
        [Required]
        [Range(0, 5)]
        public int CounterPicksToDraft { get; set; }

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
        public bool CounterPicksBlockDrops { get; set; }
        [Required]
        public int MinimumBidAmount { get; set; }

        [Required]
        public string DraftSystem { get; set; }
        [Required]
        public string PickupSystem { get; set; }
        [Required]
        public string ScoringSystem { get; set; }
        [Required]
        public bool PublicLeague { get; set; }

        [Required]
        public LeagueTagOptionsViewModel Tags { get; set; }
        [Required]
        public List<SpecialGameSlotViewModel> SpecialGameSlots { get; set; }

        public EditLeagueYearParameters ToDomain(FantasyCriticUser manager, IReadOnlyDictionary<string, MasterGameTag> tagDictionary)
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

            var leagueTags = Tags.ToDomain(tagDictionary);
            var specialGameSlots = SpecialGameSlots.Select(x => x.ToDomain(tagDictionary));

            EditLeagueYearParameters parameters = new EditLeagueYearParameters(manager, LeagueID, Year, StandardGames, GamesToDraft, CounterPicks, CounterPicksToDraft,
                freeDroppableGames, willNotReleaseDroppableGames, willReleaseDroppableGames, DropOnlyDraftGames, CounterPicksBlockDrops, MinimumBidAmount,
                leagueTags, specialGameSlots, draftSystem, pickupSystem, scoringSystem, PublicLeague);
            return parameters;
        }
    }
}
