using FantasyCritic.Lib.Domain.ScoringSystems;
using FantasyCritic.Lib.Enums;

namespace FantasyCritic.Lib.Domain.Requests
{
    public class LeagueCreationParameters
    {
        public LeagueCreationParameters(FantasyCriticUser manager, string leagueName, int standardGames, int gamesToDraft, int counterPicks, int initialYear, 
            EligibilityLevel maximumEligibilityLevel, bool allowYearlyInstallments,  bool allowEarlyAccess, bool allowFreeToPlay, bool allowReleasedInternationally,
            bool allowExpansions, DraftSystem draftSystem, PickupSystem pickupSystem, ScoringSystem scoringSystem, bool publicLeague, bool testLeague)
        {
            Manager = manager;
            LeagueName = leagueName;
            StandardGames = standardGames;
            GamesToDraft = gamesToDraft;
            CounterPicks = counterPicks;
            InitialYear = initialYear;
            AllowedEligibilitySettings = new EligibilitySettings(maximumEligibilityLevel, allowYearlyInstallments, allowEarlyAccess, 
                allowFreeToPlay, allowReleasedInternationally, allowExpansions);
            DraftSystem = draftSystem;
            PickupSystem = pickupSystem;
            ScoringSystem = scoringSystem;
            PublicLeague = publicLeague;
            TestLeague = testLeague;
        }

        public FantasyCriticUser Manager { get; }
        public string LeagueName { get; }
        public int StandardGames { get; }
        public int GamesToDraft { get; }
        public int CounterPicks { get; }
        public int InitialYear { get; }
        public EligibilitySettings AllowedEligibilitySettings { get; }
        public DraftSystem DraftSystem { get; }
        public PickupSystem PickupSystem { get; }
        public ScoringSystem ScoringSystem { get; }
        public bool PublicLeague { get; }
        public bool TestLeague { get; }
    }
}
