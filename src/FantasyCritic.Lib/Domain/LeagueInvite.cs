using FantasyCritic.Lib.Identity;

namespace FantasyCritic.Lib.Domain;

public class LeagueInvite
{
    public LeagueInvite(Guid inviteID, League league, string emailAddress)
    {
        InviteID = inviteID;
        League = league;
        EmailAddress = emailAddress;
        User = null;
    }

    public LeagueInvite(Guid inviteID, League league, FantasyCriticUser user)
    {
        InviteID = inviteID;
        League = league;
        EmailAddress = user.GetEmail();
        User = user;
    }

    public Guid InviteID { get; }
    public League League { get; }
    public string EmailAddress { get; }
    public FantasyCriticUser? User { get; }
}
