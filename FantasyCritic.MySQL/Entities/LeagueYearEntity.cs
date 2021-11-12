using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FantasyCritic.Lib.Domain;
using FantasyCritic.Lib.Domain.ScoringSystems;
using FantasyCritic.Lib.Enums;
using NodaTime;

namespace FantasyCritic.MySQL.Entities
{
    internal class LeagueYearEntity
    {
        public LeagueYearEntity()
        {

        }

        public LeagueYearEntity(League league, int year, LeagueOptions options, PlayStatus playStatus)
        {
            LeagueID = league.LeagueID;
            Year = year;

            StandardGames = options.StandardGames;
            GamesToDraft = options.GamesToDraft;
            CounterPicks = options.CounterPicks;
            FreeDroppableGames = options.FreeDroppableGames;
            WillNotReleaseDroppableGames = options.WillNotReleaseDroppableGames;
            WillReleaseDroppableGames = options.WillReleaseDroppableGames;
            DropOnlyDraftGames = options.DropOnlyDraftGames;
            CounterPicksBlockDrops = options.CounterPicksBlockDrops;
            MinimumBidAmount = options.MinimumBidAmount;

            DraftSystem = options.DraftSystem.Value;
            PickupSystem = options.PickupSystem.Value;
            ScoringSystem = options.ScoringSystem.Name;
            PlayStatus = playStatus.Value;
        }

        public Guid LeagueID { get; set; }
        public int Year { get; set; }
        public int StandardGames { get; set; }
        public int GamesToDraft { get; set; }
        public int CounterPicks { get; set; }
        public int FreeDroppableGames { get; set; }
        public int WillNotReleaseDroppableGames { get; set; }
        public int WillReleaseDroppableGames { get; set; }
        public bool DropOnlyDraftGames { get; set; }
        public bool CounterPicksBlockDrops { get; set; }
        public int MinimumBidAmount { get; set; }
        public string DraftSystem { get; set; }
        public string PickupSystem { get; set; }
        public string ScoringSystem { get; set; }
        public string PlayStatus { get; set; }
        public Instant Timestamp { get; set; }
        public Instant? DraftStartedTimestamp { get; set; }

        public LeagueYear ToDomain(League league, IEnumerable<EligibilityOverride> eligibilityOverrides, IEnumerable<TagOverride> tagOverrides, IEnumerable<LeagueTagStatus> leagueTags)
        {
            DraftSystem draftSystem = Lib.Enums.DraftSystem.FromValue(DraftSystem);
            PickupSystem pickupSystem = Lib.Enums.PickupSystem.FromValue(PickupSystem);
            ScoringSystem scoringSystem = Lib.Domain.ScoringSystems.ScoringSystem.GetScoringSystem(ScoringSystem);

            LeagueOptions options = new LeagueOptions(StandardGames, GamesToDraft, CounterPicks, FreeDroppableGames, WillNotReleaseDroppableGames, WillReleaseDroppableGames,
                DropOnlyDraftGames, CounterPicksBlockDrops, MinimumBidAmount, leagueTags, draftSystem, pickupSystem, scoringSystem, league.PublicLeague);

            return new LeagueYear(league, Year, options, Lib.Enums.PlayStatus.FromValue(PlayStatus), eligibilityOverrides, tagOverrides, DraftStartedTimestamp);
        }
    }
}
