using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FantasyCritic.Lib.Domain
{
    public class LeaguePlayer
    {
        public LeaguePlayer(League league, FantasyCriticUser player)
        {
            League = league;
            Player = player;
        }

        public League League { get; }
        public FantasyCriticUser Player { get; }
    }
}
