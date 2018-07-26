using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FantasyCritic.Lib.Domain
{
    public class LeaguePlayer
    {
        public LeaguePlayer(FantasyCriticLeague league, FantasyCriticUser player, IEnumerable<PlayerGame> games)
        {
            League = league;
            Player = player;
            Games = games.ToList();
        }

        public FantasyCriticLeague League { get; }
        public FantasyCriticUser Player { get; }
        public IReadOnlyList<PlayerGame> Games { get; }
    }
}
