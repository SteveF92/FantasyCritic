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

            MaximumEligibilityLevel = options.AllowedEligibilitySettings.EligibilityLevel.Level;
            AllowYearlyInstallments = options.AllowedEligibilitySettings.YearlyInstallment;
            AllowEarlyAccess = options.AllowedEligibilitySettings.EarlyAccess;
            AllowFreeToPlay = options.AllowedEligibilitySettings.FreeToPlay;
            AllowReleasedInternationally = options.AllowedEligibilitySettings.ReleasedInternationally;
            AllowExpansions = options.AllowedEligibilitySettings.ExpansionPack;
            AllowUnannouncedGames = options.AllowedEligibilitySettings.UnannouncedGame;

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
        public int MaximumEligibilityLevel { get; set; }
        public bool DropOnlyDraftGames { get; set; }
        public bool AllowYearlyInstallments { get; set; }
        public bool AllowEarlyAccess { get; set; }
        public bool AllowFreeToPlay { get; set; }
        public bool AllowReleasedInternationally { get; set; }
        public bool AllowExpansions { get; set; }
        public bool AllowUnannouncedGames { get; set; }
        public string DraftSystem { get; set; }
        public string PickupSystem { get; set; }
        public string ScoringSystem { get; set; }
        public string PlayStatus { get; set; }
        public DateTime Timestamp { get; set; }
        public DateTime? DraftStartedTimestamp { get; set; }

        public LeagueYear ToDomain(League league, EligibilityLevel maximumEligibilityLevel, IEnumerable<EligibilityOverride> eligibilityOverrides, 
            IEnumerable<LeagueTagStatus> leagueTags)
        {
            DraftSystem draftSystem = Lib.Enums.DraftSystem.FromValue(DraftSystem);
            PickupSystem pickupSystem = Lib.Enums.PickupSystem.FromValue(PickupSystem);
            ScoringSystem scoringSystem = Lib.Domain.ScoringSystems.ScoringSystem.GetScoringSystem(ScoringSystem);
            var eligibilitySettings = new EligibilitySettings(maximumEligibilityLevel, AllowYearlyInstallments, AllowEarlyAccess, 
                AllowFreeToPlay, AllowReleasedInternationally, AllowExpansions, AllowUnannouncedGames);

            LeagueOptions options = new LeagueOptions(StandardGames, GamesToDraft, CounterPicks, FreeDroppableGames, WillNotReleaseDroppableGames, WillReleaseDroppableGames,
                DropOnlyDraftGames, eligibilitySettings, leagueTags, draftSystem, pickupSystem, scoringSystem, league.PublicLeague);

            Instant? draftStartedTimestamp = null;
            if (DraftStartedTimestamp.HasValue)
            {
                draftStartedTimestamp = Instant.FromDateTimeUtc(DraftStartedTimestamp.Value);
            }

            return new LeagueYear(league, Year, options, Lib.Enums.PlayStatus.FromValue(PlayStatus), eligibilityOverrides, draftStartedTimestamp);
        }
    }
}
