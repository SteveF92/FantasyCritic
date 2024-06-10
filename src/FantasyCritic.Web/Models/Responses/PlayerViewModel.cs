using FantasyCritic.Lib.Domain.Conferences;
using FantasyCritic.Lib.Identity;

namespace FantasyCritic.Web.Models.Responses;

public class PlayerViewModel
{
    public PlayerViewModel(League league, MinimalFantasyCriticUser user, bool removable)
    {
        LeagueID = league.LeagueID;
        LeagueName = league.LeagueName;
        UserID = user.UserID;
        DisplayName = user.DisplayName;
        Removable = removable;
    }

    public PlayerViewModel(ConferenceLeague league, MinimalFantasyCriticUser user, bool removable)
    {
        LeagueID = league.LeagueID;
        LeagueName = league.LeagueName;
        UserID = user.UserID;
        DisplayName = user.DisplayName;
        Removable = removable;
    }

    public Guid LeagueID { get; }
    public string LeagueName { get; }
    public Guid UserID { get; }
    public string DisplayName { get; }
    public bool Removable { get; }
}
