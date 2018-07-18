using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using FantasyCritic.Lib.Domain;

namespace FantasyCritic.Web.Models.Responses
{
    public class FantasyCriticLeagueViewModel
    {
        public FantasyCriticLeagueViewModel(FantasyCriticLeague league, bool isManager)
        {
            LeagueName = league.LeagueName;
            LeagueManager = new FantasyCriticPlayerViewModel(league.LeagueManager);
            IsManager = isManager;
        }

        public FantasyCriticLeagueViewModel(FantasyCriticLeague league, bool isManager, IEnumerable<FantasyCriticUser> players)
        {
            LeagueName = league.LeagueName;
            LeagueManager = new FantasyCriticPlayerViewModel(league.LeagueManager);
            IsManager = isManager;
            Players = players.Select(x => new FantasyCriticPlayerViewModel(x)).ToList();
        }

        public string LeagueName { get; }
        public FantasyCriticPlayerViewModel LeagueManager { get; }
        public bool IsManager { get; }
        public IReadOnlyList<FantasyCriticPlayerViewModel> Players { get; }
    }
}
