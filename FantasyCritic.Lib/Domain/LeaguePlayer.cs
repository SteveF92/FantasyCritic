using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FantasyCritic.Lib.Domain
{
    public class LeaguePlayer
    {
        public LeaguePlayer(League league, FantasyCriticUser user, IEnumerable<PlayerGame> playerGames)
        {
            League = league;
            User = user;
            PlayerGames = playerGames.ToList();
        }

        public League League { get; }
        public FantasyCriticUser User { get; }
        public IReadOnlyList<PlayerGame> PlayerGames { get; }
    }
}
