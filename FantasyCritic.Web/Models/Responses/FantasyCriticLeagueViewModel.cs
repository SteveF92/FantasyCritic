using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using FantasyCritic.Lib.Domain;

namespace FantasyCritic.Web.Models.Responses
{
    public class FantasyCriticLeagueViewModel
    {
        public FantasyCriticLeagueViewModel(FantasyCriticLeague league, IEnumerable<FantasyCriticUser> players)
        {
            LeagueName = league.LeagueName;
            LeagueManager = new FantasyCriticPlayerViewModel(league.LeagueManager);
            Players = players.Select(x => new FantasyCriticPlayerViewModel(x)).ToList();
        }

        public string LeagueName { get; }
        public FantasyCriticPlayerViewModel LeagueManager { get; }
        public IReadOnlyList<FantasyCriticPlayerViewModel> Players { get; }
    }
}
