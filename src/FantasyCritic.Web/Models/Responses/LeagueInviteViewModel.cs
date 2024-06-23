namespace FantasyCritic.Web.Models.Responses;

public class LeagueInviteViewModel
{
    public LeagueInviteViewModel(LeagueInvite invite)
    {
        InviteID = invite.InviteID;
        LeagueID = invite.LeagueID;
    }

    public Guid InviteID { get; }
    public Guid LeagueID { get; }
}

public class CompleteLeagueInviteViewModel
{
    public CompleteLeagueInviteViewModel(CompleteLeagueInvite invite)
    {
        InviteID = invite.InviteID;
        LeagueID = invite.LeagueID;
        LeagueName = invite.LeagueName;
        ActiveYear = invite.ActiveYear;
        InviteName = invite.InviteName;
        LeagueManager = new PlayerViewModel(invite.LeagueID, invite.LeagueName, invite.LeagueManager, false);
    }

    public Guid InviteID { get; }
    public Guid LeagueID { get; }
    public string LeagueName { get; }
    public int ActiveYear { get; }
    public string InviteName { get; }
    public PlayerViewModel LeagueManager { get; }
}
