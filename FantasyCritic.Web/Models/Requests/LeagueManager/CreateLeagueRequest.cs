using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using FantasyCritic.Lib.Domain;
using FantasyCritic.Lib.Domain.Requests;
using FantasyCritic.Lib.Domain.ScoringSystems;
using FantasyCritic.Lib.Enums;
using FantasyCritic.Web.Models.RoundTrip;

namespace FantasyCritic.Web.Models.Requests.LeagueManager
{
    public class CreateLeagueRequest
    {
        [Required]
        public string LeagueName { get; set; }

        [Required]
        [Range(1, 50)]
        public int StandardGames { get; set; }
        [Required]
        [Range(1, 50)]
        public int GamesToDraft { get; set; }
        [Required]
        [Range(0,20)]
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
        public bool CounterPicksBlockDrops { get; set; }
        [Required]
        public int MinimumBidAmount { get; set; }

        [Required]
        public int InitialYear { get; set; }
        [Required]
        public string DraftSystem { get; set; }
        [Required]
        public string PickupSystem { get; set; }
        [Required]
        public string ScoringSystem { get; set; }
        [Required]
        public bool PublicLeague { get; set; }
        [Required]
        public bool TestLeague { get; set; }

        [Required]
        public LeagueTagOptionsViewModel Tags { get; set; }


        //Don't need this once 2019 is no longer create-able.
        public bool ValidForOldYears()
        {
            if (InitialYear > 2019)
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

        public LeagueCreationParameters ToDomain(FantasyCriticUser manager, IReadOnlyDictionary<string, MasterGameTag> tagDictionary)
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

            LeagueCreationParameters parameters = new LeagueCreationParameters(manager, LeagueName, StandardGames, GamesToDraft, CounterPicks,
                freeDroppableGames, willNotReleaseDroppableGames, willReleaseDroppableGames, DropOnlyDraftGames, CounterPicksBlockDrops, MinimumBidAmount,
                InitialYear, leagueTags, draftSystem, pickupSystem, scoringSystem, PublicLeague, TestLeague);
            return parameters;
        }
    }
}
