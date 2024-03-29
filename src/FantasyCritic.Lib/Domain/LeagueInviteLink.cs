namespace FantasyCritic.Lib.Domain;

public class LeagueInviteLink
{
    public LeagueInviteLink(Guid inviteID, League league, Guid inviteCode, bool active)
    {
        InviteID = inviteID;
        League = league;
        InviteCode = inviteCode;
        Active = active;
    }

    public Guid InviteID { get; }
    public League League { get; }
    public Guid InviteCode { get; }
    public bool Active { get; }
}
