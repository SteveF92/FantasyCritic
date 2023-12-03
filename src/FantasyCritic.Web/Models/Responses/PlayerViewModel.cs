using FantasyCritic.Lib.Domain.Conferences;
using FantasyCritic.Lib.Identity;

namespace FantasyCritic.Web.Models.Responses;

public class PlayerViewModel
{
    public PlayerViewModel(League league, FantasyCriticUser user, bool removable)
    {
        LeagueID = league.LeagueID;
        LeagueName = league.LeagueName;
        UserID = user.Id;
        DisplayName = user.UserName;
        Removable = removable;
    }

    public PlayerViewModel(ConferenceLeague league, FantasyCriticUser user, bool removable)
    {
        LeagueID = league.LeagueID;
        LeagueName = league.LeagueName;
        UserID = user.Id;
        DisplayName = user.UserName;
        Removable = removable;
    }

    public Guid LeagueID { get; }
    public string LeagueName { get; }
    public Guid UserID { get; }
    public string DisplayName { get; }
    public bool Removable { get; }
}
