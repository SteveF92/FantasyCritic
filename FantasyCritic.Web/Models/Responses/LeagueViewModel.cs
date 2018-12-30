using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using FantasyCritic.Lib.Domain;

namespace FantasyCritic.Web.Models.Responses
{
    public class LeagueViewModel
    {
        public LeagueViewModel(League league, bool isManager)
        {
            LeagueID = league.LeagueID;
            LeagueName = league.LeagueName;
            LeagueManager = new PlayerViewModel(league, league.LeagueManager);
            IsManager = isManager;
            Years = league.Years;
            ActiveYear = Years.Max();
        }

        public LeagueViewModel(League league, bool isManager, IEnumerable<FantasyCriticUser> players, bool outstandingInvite, bool neverStarted)
        {
            LeagueID = league.LeagueID;
            LeagueName = league.LeagueName;
            LeagueManager = new PlayerViewModel(league, league.LeagueManager);
            IsManager = isManager;
            Years = league.Years;
            ActiveYear = Years.Max();
            OutstandingInvite = outstandingInvite;
            Players = players.Select(x => new PlayerViewModel(league, x)).ToList();
            NeverStarted = neverStarted;
        }

        public Guid LeagueID { get; }
        public string LeagueName { get; }
        public PlayerViewModel LeagueManager { get; }
        public bool IsManager { get; }
        public IReadOnlyList<PlayerViewModel> Players { get; }
        public bool OutstandingInvite { get; }
        public IReadOnlyList<int> Years { get; }
        public int ActiveYear { get; }
        public bool NeverStarted { get; }
    }
}
