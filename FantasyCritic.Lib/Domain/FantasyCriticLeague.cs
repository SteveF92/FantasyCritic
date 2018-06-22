using System;
using System.Collections.Generic;
using System.Text;

namespace FantasyCritic.Lib.Domain
{
    public class FantasyCriticLeague
    {
        public FantasyCriticLeague(Guid leagueID, string leagueName)
        {
            LeagueID = leagueID;
            LeagueName = leagueName;
        }

        public Guid LeagueID { get; }
        public string LeagueName { get; }
    }
}
