using System;
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
            LeagueID = league.LeagueID;
            LeagueName = league.LeagueName;
            LeagueManager = new FantasyCriticPlayerViewModel(league.LeagueManager);
            IsManager = isManager;
        }

        public FantasyCriticLeagueViewModel(FantasyCriticLeague league, bool isManager, IEnumerable<FantasyCriticUser> players, bool outstandingInvite)
        {
            LeagueID = league.LeagueID;
            LeagueName = league.LeagueName;
            LeagueManager = new FantasyCriticPlayerViewModel(league.LeagueManager);
            IsManager = isManager;
            OutstandingInvite = outstandingInvite;
            Players = players.Select(x => new FantasyCriticPlayerViewModel(x)).ToList();
        }

        public Guid LeagueID { get; }
        public string LeagueName { get; }
        public FantasyCriticPlayerViewModel LeagueManager { get; }
        public bool IsManager { get; }
        public IReadOnlyList<FantasyCriticPlayerViewModel> Players { get; }
        public bool OutstandingInvite { get; }
    }
}
