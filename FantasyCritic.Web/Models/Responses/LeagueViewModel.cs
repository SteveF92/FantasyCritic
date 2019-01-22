using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using FantasyCritic.Lib.Domain;

namespace FantasyCritic.Web.Models.Responses
{
    public class LeagueViewModel
    {
        public LeagueViewModel(League league, bool isManager, bool userIsInLeague, bool userIsFollowingLeague)
        {
            LeagueID = league.LeagueID;
            LeagueName = league.LeagueName;
            LeagueManager = new PlayerViewModel(league, league.LeagueManager);
            IsManager = isManager;
            Years = league.Years;
            ActiveYear = Years.Max();
            PublicLeague = league.PublicLeague;
            TestLeague = league.TestLeague;
            UserIsInLeague = userIsInLeague;
            UserIsFollowingLeague = userIsFollowingLeague;
            NumberOfFollowers = league.NumberOfFollowers;
        }

        public LeagueViewModel(League league, bool isManager, IEnumerable<FantasyCriticUser> players, bool outstandingInvite, bool neverStarted, bool userIsInLeague, bool userIsFollowingLeague)
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
            PublicLeague = league.PublicLeague;
            TestLeague = league.TestLeague;
            UserIsInLeague = userIsInLeague;
            UserIsFollowingLeague = userIsFollowingLeague;
            NumberOfFollowers = league.NumberOfFollowers;
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
        public bool PublicLeague { get; }
        public bool TestLeague { get; }
        public bool UserIsInLeague { get; }
        public bool UserIsFollowingLeague { get; }
        public int NumberOfFollowers{ get; }
    }
}
