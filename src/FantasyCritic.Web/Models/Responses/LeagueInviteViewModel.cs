using FantasyCritic.Lib.Identity;

namespace FantasyCritic.Web.Models.Responses;

public class LeagueInviteViewModel
{
    private LeagueInviteViewModel(Guid inviteID, League league, string inviteName)
    {
        InviteID = inviteID;
        LeagueID = league.LeagueID;
        LeagueName = league.LeagueName;
        ActiveYear = league.Years.Max();
        InviteName = inviteName;
        LeagueManager = new PlayerViewModel(league, league.LeagueManager, true);
    }

    public Guid InviteID { get; }
    public Guid LeagueID { get; }
    public string LeagueName { get; }
    public int ActiveYear { get; }
    public string InviteName { get; }
    public PlayerViewModel LeagueManager { get; }

    public static LeagueInviteViewModel CreateWithEmailAddress(LeagueInvite invite)
    {
        return new LeagueInviteViewModel(invite.InviteID, invite.League, invite.EmailAddress);
    }

    public static LeagueInviteViewModel CreateWithDisplayName(LeagueInvite invite, FantasyCriticUser user)
    {
        return new LeagueInviteViewModel(invite.InviteID, invite.League, user.UserName);
    }
}