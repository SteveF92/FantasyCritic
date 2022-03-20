using System.ComponentModel.DataAnnotations;

namespace FantasyCritic.Web.Models.Requests.LeagueManager;

public class DeleteInviteLinkRequest
{
    [Required]
    public Guid LeagueID { get; set; }
    [Required]
    public Guid InviteID { get; set; }
}
