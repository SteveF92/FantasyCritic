using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FantasyCritic.Lib.Domain;

namespace FantasyCritic.Web.Models
{
    public class FantasyCriticLeagueViewModel
    {
        public FantasyCriticLeagueViewModel(FantasyCriticLeague league)
        {
            LeagueName = league.LeagueName;
            LeagueManager = new FantasyCriticPlayerViewModel(league.LeagueManager);
        }

        public string LeagueName { get; }
        public FantasyCriticPlayerViewModel LeagueManager { get; }
    }
}
