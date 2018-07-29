using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FantasyCritic.Lib.Domain;

namespace FantasyCritic.MySQL.Entities
{
    internal class LeagueYearEntity
    {
        public LeagueYearEntity()
        {

        }

        public LeagueYearEntity(League league, int year, LeagueOptions options)
        {
            LeagueID = league.LeagueID;
            Year = year;

            DraftGames = options.DraftGames;
            WaiverGames = options.WaiverGames;
            AntiPicks = options.AntiPicks;
            EstimatedGameScore = options.EstimatedGameScore;

            EligibilitySystem = options.EligibilitySystem.Value;
            DraftSystem = options.DraftSystem.Value;
            WaiverSystem = options.WaiverSystem.Value;
            ScoringSystem = options.ScoringSystem.Value;
        }

        public Guid LeagueID { get; set; }
        public int Year { get; set; }
        public int DraftGames { get; set; }
        public int WaiverGames { get; set; }
        public int AntiPicks { get; set; }
        public decimal EstimatedGameScore { get; set; }
        public string EligibilitySystem { get; set; }
        public string DraftSystem { get; set; }
        public string WaiverSystem { get; set; }
        public string ScoringSystem { get; set; }

    }
}
