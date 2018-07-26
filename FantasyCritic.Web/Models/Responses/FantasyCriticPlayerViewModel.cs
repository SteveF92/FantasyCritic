using System.Collections.Generic;
using System.Linq;
using FantasyCritic.Lib.Domain;

namespace FantasyCritic.Web.Models.Responses
{
    public class FantasyCriticPlayerViewModel
    {
        public FantasyCriticPlayerViewModel(FantasyCriticUser user)
        {
            UserID = user.UserID.ToString();
            UserName = user.UserName;
        }

        public FantasyCriticPlayerViewModel(FantasyCriticLeague league, FantasyCriticUser user, IEnumerable<PlayerGameViewModel> games)
        {
            LeagueID = league.LeagueID.ToString();
            LeagueName = league.LeagueName;
            UserID = user.UserID.ToString();
            UserName = user.UserName;
            Games = games.ToList();
        }

        public string LeagueID { get; }
        public string LeagueName { get; }
        public string UserID { get; }
        public string UserName { get; }
        public IReadOnlyList<PlayerGameViewModel> Games { get; }
    }
}
