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

        public LeagueViewModel(League league, bool isManager, IEnumerable<FantasyCriticUser> players,
            IEnumerable<FantasyCriticUser> invitedPlayers, bool outstandingInvite)
        {
            LeagueID = league.LeagueID;
            LeagueName = league.LeagueName;
            LeagueManager = new PlayerViewModel(league, league.LeagueManager);
            IsManager = isManager;
            Years = league.Years;
            ActiveYear = Years.Max();
            OutstandingInvite = outstandingInvite;
            Players = players.Select(x => new PlayerViewModel(league, x)).ToList();
            InvitedPlayers = invitedPlayers.Select(x => new PlayerViewModel(league, x)).ToList();
        }

        public Guid LeagueID { get; }
        public string LeagueName { get; }
        public PlayerViewModel LeagueManager { get; }
        public bool IsManager { get; }
        public IReadOnlyList<PlayerViewModel> Players { get; }
        public IReadOnlyList<PlayerViewModel> InvitedPlayers { get; }
        public bool OutstandingInvite { get; }
        public IReadOnlyList<int> Years { get; }
        public int ActiveYear { get; }
    }
}
