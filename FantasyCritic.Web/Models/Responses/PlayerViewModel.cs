using System.Collections.Generic;
using System.Linq;
using FantasyCritic.Lib.Domain;

namespace FantasyCritic.Web.Models.Responses
{
    public class PlayerViewModel
    {
        public PlayerViewModel(League league, FantasyCriticUser user)
        {
            LeagueID = league.LeagueID.ToString();
            LeagueName = league.LeagueName;
            UserID = user.UserID.ToString();
            DisplayName = user.DisplayName;
        }

        public string LeagueID { get; }
        public string LeagueName { get; }
        public string UserID { get; }
        public string DisplayName { get; }
    }
}
