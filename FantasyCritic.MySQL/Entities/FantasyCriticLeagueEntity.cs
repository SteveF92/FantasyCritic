using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FantasyCritic.Lib.Domain;

namespace FantasyCritic.MySQL.Entities
{
    internal class FantasyCriticLeagueEntity
    {
        public FantasyCriticLeagueEntity()
        {

        }

        public FantasyCriticLeagueEntity(FantasyCriticLeague league)
        {
            LeagueID = league.LeagueID;
            LeagueName = league.LeagueName;
            LeagueManager = league.LeagueManager.UserID;

            DraftGames = league.LeagueOptions.DraftGames;
            WaiverGames = league.LeagueOptions.WaiverGames;
            AntiPicks = league.LeagueOptions.AntiPicks;
            EstimatedGameScore = league.LeagueOptions.EstimatedGameScore;

            EligibilitySystem = league.LeagueOptions.EligibilitySystem.Value;
            DraftSystem = league.LeagueOptions.DraftSystem.Value;
            WaiverSystem = league.LeagueOptions.WaiverSystem.Value;
            ScoringSystem = league.LeagueOptions.ScoringSystem.Value;
        }

        public Guid LeagueID { get; set; }
        public string LeagueName { get; set; }
        public Guid LeagueManager { get; set; }
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
