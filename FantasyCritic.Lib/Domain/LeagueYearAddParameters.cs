using System;
using System.Collections.Generic;
using System.Text;
using FantasyCritic.Lib.Enums;

namespace FantasyCritic.Lib.Domain
{
    public class LeagueYearAddParameters
    {
        public LeagueYearAddParameters(League league, int draftGames, int waiverGames, int antiPicks, decimal estimatedGameScore, int year,
            EligibilitySystem eligibilitySystem, DraftSystem draftSystem, WaiverSystem waiverSystem, ScoringSystem scoringSystem)
        {
            League = league;
            DraftGames = draftGames;
            WaiverGames = waiverGames;
            AntiPicks = antiPicks;
            EstimatedGameScore = estimatedGameScore;
            Year = year;
            EligibilitySystem = eligibilitySystem;
            DraftSystem = draftSystem;
            WaiverSystem = waiverSystem;
            ScoringSystem = scoringSystem;
        }

        public League League { get; }
        public int DraftGames { get; }
        public int WaiverGames { get; }
        public int AntiPicks { get; }
        public decimal EstimatedGameScore { get; }
        public int Year { get; }
        public EligibilitySystem EligibilitySystem { get; }
        public DraftSystem DraftSystem { get; }
        public WaiverSystem WaiverSystem { get; }
        public ScoringSystem ScoringSystem { get; }
    }
}
