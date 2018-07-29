using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FantasyCritic.Lib.Domain
{
    public class League
    {
        public League(Guid leagueID, string leagueName, FantasyCriticUser leagueManager, IEnumerable<int> years)
        {
            LeagueID = leagueID;
            LeagueName = leagueName;
            LeagueManager = leagueManager;
            Years = years.ToList();
        }

        public Guid LeagueID { get; }
        public string LeagueName { get; }
        public FantasyCriticUser LeagueManager { get; }
        public IReadOnlyList<int> Years { get; }
    }
}
