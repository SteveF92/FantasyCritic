using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FantasyCritic.Lib.Domain
{
    public class FantasyCriticLeague
    {
        public FantasyCriticLeague(Guid leagueID, string leagueName, FantasyCriticUser leagueManager, IEnumerable<int> leagueYears, LeagueOptions leagueOptions)
        {
            LeagueID = leagueID;
            LeagueName = leagueName;
            LeagueManager = leagueManager;
            LeagueYears = leagueYears.ToList();
            LeagueOptions = leagueOptions;
        }

        public Guid LeagueID { get; }
        public string LeagueName { get; }
        public FantasyCriticUser LeagueManager { get; }
        public IReadOnlyList<int> LeagueYears { get; }
        public LeagueOptions LeagueOptions { get; }
    }
}
