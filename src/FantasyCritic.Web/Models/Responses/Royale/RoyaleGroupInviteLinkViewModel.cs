using FantasyCritic.Lib.Royale;

namespace FantasyCritic.Web.Models.Responses.Royale;

public class RoyaleGroupInviteLinkViewModel
{
    public RoyaleGroupInviteLinkViewModel(RoyaleGroupInviteLink link)
    {
        InviteID = link.InviteID;
        InviteCode = link.InviteCode;
        Active = link.Active;
    }

    public Guid InviteID { get; }
    public Guid InviteCode { get; }
    public bool Active { get; }
}
