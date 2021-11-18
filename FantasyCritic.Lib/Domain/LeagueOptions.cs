using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using FantasyCritic.Lib.Domain.Requests;
using FantasyCritic.Lib.Domain.ScoringSystems;
using FantasyCritic.Lib.Enums;

namespace FantasyCritic.Lib.Domain
{
    public class LeagueOptions
    {
        public LeagueOptions(int standardGames, int gamesToDraft, int counterPicks, int counterPicksToDraft, int freeDroppableGames, int willNotReleaseDroppableGames, int willReleaseDroppableGames,
            bool dropOnlyDraftGames, bool counterPicksBlockDrops, int minimumBidAmount, IEnumerable<LeagueTagStatus> leagueTags, DraftSystem draftSystem, PickupSystem pickupSystem, ScoringSystem scoringSystem, bool publicLeague)
        {
            StandardGames = standardGames;
            GamesToDraft = gamesToDraft;
            CounterPicks = counterPicks;
            CounterPicksToDraft = counterPicksToDraft;
            FreeDroppableGames = freeDroppableGames;
            WillNotReleaseDroppableGames = willNotReleaseDroppableGames;
            WillReleaseDroppableGames = willReleaseDroppableGames;
            DropOnlyDraftGames = dropOnlyDraftGames;
            CounterPicksBlockDrops = counterPicksBlockDrops;
            MinimumBidAmount = minimumBidAmount;
            LeagueTags = leagueTags.ToList();
            DraftSystem = draftSystem;
            PickupSystem = pickupSystem;
            ScoringSystem = scoringSystem;
            PublicLeague = publicLeague;
        }

        public LeagueOptions(LeagueCreationParameters parameters)
        {
            StandardGames = parameters.StandardGames;
            GamesToDraft = parameters.GamesToDraft;
            CounterPicks = parameters.CounterPicks;
            CounterPicksToDraft = parameters.CounterPicksToDraft;
            FreeDroppableGames = parameters.FreeDroppableGames;
            WillNotReleaseDroppableGames = parameters.WillNotReleaseDroppableGames;
            WillReleaseDroppableGames = parameters.WillReleaseDroppableGames;
            DropOnlyDraftGames = parameters.DropOnlyDraftGames;
            CounterPicksBlockDrops = parameters.CounterPicksBlockDrops;
            MinimumBidAmount = parameters.MinimumBidAmount;
            LeagueTags = parameters.LeagueTags;
            DraftSystem = parameters.DraftSystem;
            PickupSystem = parameters.PickupSystem;
            ScoringSystem = parameters.ScoringSystem;
            PublicLeague = parameters.PublicLeague;
        }

        public LeagueOptions(EditLeagueYearParameters parameters)
        {
            StandardGames = parameters.StandardGames;
            GamesToDraft = parameters.GamesToDraft;
            CounterPicks = parameters.CounterPicks;
            CounterPicksToDraft = parameters.CounterPicksToDraft;
            FreeDroppableGames = parameters.FreeDroppableGames;
            WillNotReleaseDroppableGames = parameters.WillNotReleaseDroppableGames;
            WillReleaseDroppableGames = parameters.WillReleaseDroppableGames;
            DropOnlyDraftGames = parameters.DropOnlyDraftGames;
            CounterPicksBlockDrops = parameters.CounterPicksBlockDrops;
            MinimumBidAmount = parameters.MinimumBidAmount;
            LeagueTags = parameters.LeagueTags;
            DraftSystem = parameters.DraftSystem;
            PickupSystem = parameters.PickupSystem;
            ScoringSystem = parameters.ScoringSystem;
            PublicLeague = parameters.PublicLeague;
        }

        public int StandardGames { get; }
        public int GamesToDraft { get; }
        public int CounterPicks { get; }
        public int CounterPicksToDraft { get; }
        public int FreeDroppableGames { get; }
        public int WillNotReleaseDroppableGames { get; }
        public int WillReleaseDroppableGames { get; }
        public bool DropOnlyDraftGames { get; }
        public bool CounterPicksBlockDrops { get; }
        public int MinimumBidAmount { get; }
        public IReadOnlyList<LeagueTagStatus> LeagueTags { get; }
        public DraftSystem DraftSystem { get; }
        public PickupSystem PickupSystem { get; }
        public ScoringSystem ScoringSystem { get; }
        public bool PublicLeague { get; }

        public Result Validate()
        {
            if (GamesToDraft > StandardGames)
            {
                return Result.Failure("Can't draft more than the total number of standard games.");
            }

            if (CounterPicks > GamesToDraft)
            {
                return Result.Failure("Can't have more counter picks than drafted games.");
            }

            if (CounterPicksToDraft > CounterPicks)
            {
                return Result.Failure("Can't draft more counter picks than the total number of counter picks.");
            }

            var bannedTagNames = LeagueTags.Where(x => x.Status.Equals(TagStatus.Banned)).Select(x => x.Tag.Name).ToHashSet();
            if (bannedTagNames.Contains("PlannedForEarlyAccess") && !bannedTagNames.Contains("CurrentlyInEarlyAccess"))
            {
                return Result.Failure("If you ban 'Planned for Early Access' you must also ban 'Currently in Early Access'. See the FAQ page for an explanation.");
            }
            if (bannedTagNames.Contains("WillReleaseInternationallyFirst") && !bannedTagNames.Contains("ReleasedInternationally"))
            {
                return Result.Failure("If you ban 'Will Release Internationally First' you must also ban 'Released Internationally'. See the FAQ page for an explanation.");
            }

            return Result.Success();
        }
    }
}
