using System.ComponentModel.DataAnnotations;

namespace FantasyCritic.Web.Models.Requests.Shared;

public class DeleteInviteRequest
{
    [Required]
    public Guid LeagueID { get; set; }
    [Required]
    public Guid InviteID { get; set; }
}
