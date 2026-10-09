using FantasyCritic.Lib.Identity;

namespace FantasyCritic.Web.Models.Responses;

public class PlayerViewModel
{
    public PlayerViewModel(Guid leagueID, string leagueName, IVeryMinimalFantasyCriticUser user)
    {
        LeagueID = leagueID;
        LeagueName = leagueName;
        UserID = user.UserID;
        DisplayName = user.DisplayName;
    }

    public Guid LeagueID { get; }
    public string LeagueName { get; }
    public Guid UserID { get; }
    public string DisplayName { get; }
}
