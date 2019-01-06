using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FantasyCritic.Lib.Domain;
using FantasyCritic.Lib.Enums;

namespace FantasyCritic.MySQL.Entities
{
    internal class LeagueEntity
    {
        public LeagueEntity()
        {

        }

        public LeagueEntity(League league)
        {
            LeagueID = league.LeagueID;
            LeagueName = league.LeagueName;
            LeagueManager = league.LeagueManager.UserID;
            PublicLeague = league.PublicLeague;
        }

        public Guid LeagueID { get; set; }
        public string LeagueName { get; set; }
        public Guid LeagueManager { get; set; }
        public bool PublicLeague { get; set; }

        public League ToDomain(FantasyCriticUser manager, IEnumerable<int> years)
        {
            League parameters = new League(LeagueID, LeagueName, manager, years, PublicLeague);
            return parameters;
        }
    }
}
