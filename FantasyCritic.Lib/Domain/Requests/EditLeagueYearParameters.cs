using System;
using System.Collections.Generic;
using System.Linq;
using FantasyCritic.Lib.Domain.ScoringSystems;
using FantasyCritic.Lib.Enums;

namespace FantasyCritic.Lib.Domain.Requests
{
    public class EditLeagueYearParameters
    {
        public EditLeagueYearParameters(FantasyCriticUser manager, Guid leagueID, int year, int standardGames, int gamesToDraft, int counterPicks,
            int freeDroppableGames, int willNotReleaseDroppableGames, int willReleaseDroppableGames, bool dropOnlyDraftGames, EligibilityLevel maximumEligibilityLevel, bool allowYearlyInstallments, 
            bool allowEarlyAccess, bool allowFreeToPlay, bool allowReleasedInternationally, bool allowExpansions, bool allowUnannouncedGames, IEnumerable<LeagueTagStatus> leagueTags,
            DraftSystem draftSystem, PickupSystem pickupSystem, ScoringSystem scoringSystem, bool publicLeague)
        {
            Manager = manager;
            LeagueID = leagueID;
            Year = year;
            StandardGames = standardGames;
            GamesToDraft = gamesToDraft;
            CounterPicks = counterPicks;
            FreeDroppableGames = freeDroppableGames;
            WillNotReleaseDroppableGames = willNotReleaseDroppableGames;
            WillReleaseDroppableGames = willReleaseDroppableGames;
            DropOnlyDraftGames = dropOnlyDraftGames;
            AllowedEligibilitySettings = new EligibilitySettings(maximumEligibilityLevel, allowYearlyInstallments, allowEarlyAccess, allowFreeToPlay,
                allowReleasedInternationally, allowExpansions, allowUnannouncedGames);
            LeagueTags = leagueTags.ToList();
            DraftSystem = draftSystem;
            PickupSystem = pickupSystem;
            ScoringSystem = scoringSystem;
            PublicLeague = publicLeague;
        }

        public FantasyCriticUser Manager { get; }
        public Guid LeagueID { get; }
        public int Year { get; }
        public int StandardGames { get; }
        public int GamesToDraft { get; }
        public int CounterPicks { get; }
        public int FreeDroppableGames { get; }
        public int WillNotReleaseDroppableGames { get; }
        public int WillReleaseDroppableGames { get; }
        public bool DropOnlyDraftGames { get; }
        public EligibilitySettings AllowedEligibilitySettings { get; }
        public IReadOnlyList<LeagueTagStatus> LeagueTags { get; }
        public DraftSystem DraftSystem { get; }
        public PickupSystem PickupSystem { get; }
        public ScoringSystem ScoringSystem { get; }
        public bool PublicLeague { get; }
    }
}
