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
            Years = league.LeagueYears;
            ActiveYear = league.LeagueYears.Max();
        }

        public FantasyCriticLeagueViewModel(FantasyCriticLeague league, bool isManager, IEnumerable<LeaguePlayer> players,
            IEnumerable<FantasyCriticUser> invitedPlayers, bool outstandingInvite)
        {
            LeagueID = league.LeagueID;
            LeagueName = league.LeagueName;
            LeagueManager = new FantasyCriticPlayerViewModel(league.LeagueManager);
            IsManager = isManager;
            OutstandingInvite = outstandingInvite;
            Players = players.Select(x => new FantasyCriticPlayerViewModel(league, x.Player, x.Games)).ToList();
            InvitedPlayers = invitedPlayers.Select(x => new FantasyCriticPlayerViewModel(x)).ToList();
            Years = league.LeagueYears;
            ActiveYear = league.LeagueYears.Max();
        }

        public Guid LeagueID { get; }
        public string LeagueName { get; }
        public FantasyCriticPlayerViewModel LeagueManager { get; }
        public bool IsManager { get; }
        public IReadOnlyList<FantasyCriticPlayerViewModel> Players { get; }
        public IReadOnlyList<FantasyCriticPlayerViewModel> InvitedPlayers { get; }
        public bool OutstandingInvite { get; }
        public IReadOnlyList<int> Years { get; }
        public int ActiveYear { get; }
    }
}
