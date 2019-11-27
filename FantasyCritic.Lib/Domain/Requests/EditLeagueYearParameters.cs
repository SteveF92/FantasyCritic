using System;
using FantasyCritic.Lib.Domain.ScoringSystems;
using FantasyCritic.Lib.Enums;

namespace FantasyCritic.Lib.Domain.Requests
{
    public class EditLeagueYearParameters
    {
        public EditLeagueYearParameters(FantasyCriticUser manager, Guid leagueID, int year, int standardGames, int gamesToDraft, int counterPicks,
            int freeDroppableGames, int willNotReleaseDroppableGames, EligibilityLevel maximumEligibilityLevel, bool allowYearlyInstallments, 
            bool allowEarlyAccess, bool allowFreeToPlay, bool allowReleasedInternationally, bool allowExpansions, DraftSystem draftSystem, 
            PickupSystem pickupSystem, ScoringSystem scoringSystem, bool publicLeague)
        {
            Manager = manager;
            LeagueID = leagueID;
            Year = year;
            StandardGames = standardGames;
            GamesToDraft = gamesToDraft;
            CounterPicks = counterPicks;
            FreeDroppableGames = freeDroppableGames;
            WillNotReleaseDroppableGames = willNotReleaseDroppableGames;
            AllowedEligibilitySettings = new EligibilitySettings(maximumEligibilityLevel, allowYearlyInstallments, allowEarlyAccess, allowFreeToPlay,
                allowReleasedInternationally, allowExpansions);
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
        public EligibilitySettings AllowedEligibilitySettings { get; }
        public DraftSystem DraftSystem { get; }
        public PickupSystem PickupSystem { get; }
        public ScoringSystem ScoringSystem { get; }
        public bool PublicLeague { get; }
    }
}
