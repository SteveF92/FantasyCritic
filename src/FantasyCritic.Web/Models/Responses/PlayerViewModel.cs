using FantasyCritic.Lib.Identity;

namespace FantasyCritic.Web.Models.Responses
{
    public class PlayerViewModel
    {

        public PlayerViewModel(League league, FantasyCriticUser user, bool removable)
        {
            Removable = removable;
            LeagueID = league.LeagueID.ToString();
            LeagueName = league.LeagueName;
            UserID = user.Id.ToString();
            DisplayName = user.UserName;
        }

        public string LeagueID { get; }
        public string LeagueName { get; }
        public string UserID { get; }
        public string DisplayName { get; }
        public bool Removable { get; }
    }
}
