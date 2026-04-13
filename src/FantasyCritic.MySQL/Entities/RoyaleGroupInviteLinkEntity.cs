using FantasyCritic.Lib.Royale;

namespace FantasyCritic.MySQL.Entities;

internal class RoyaleGroupInviteLinkEntity
{
    public RoyaleGroupInviteLinkEntity()
    {
    }

    public RoyaleGroupInviteLinkEntity(RoyaleGroupInviteLink domain)
    {
        InviteID = domain.InviteID;
        GroupID = domain.Group.GroupID;
        InviteCode = domain.InviteCode;
        Active = domain.Active;
    }

    public Guid InviteID { get; set; }
    public Guid GroupID { get; set; }
    public Guid InviteCode { get; set; }
    public bool Active { get; set; }

    public RoyaleGroupInviteLink ToDomain(RoyaleGroup group)
    {
        return new RoyaleGroupInviteLink(InviteID, group, InviteCode, Active);
    }
}
