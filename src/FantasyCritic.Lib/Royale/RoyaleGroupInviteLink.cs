namespace FantasyCritic.Lib.Royale;

public class RoyaleGroupInviteLink
{
    public RoyaleGroupInviteLink(Guid inviteID, RoyaleGroup group, Guid inviteCode, bool active)
    {
        InviteID = inviteID;
        Group = group;
        InviteCode = inviteCode;
        Active = active;
    }

    public Guid InviteID { get; }
    public RoyaleGroup Group { get; }
    public Guid InviteCode { get; }
    public bool Active { get; }
}
