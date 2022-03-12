namespace FantasyCritic.Web.Models.Requests.LeagueManager;

public class DeleteInviteLinkRequest
{
    public Guid LeagueID { get; set; }
    public Guid InviteID { get; set; }
}
