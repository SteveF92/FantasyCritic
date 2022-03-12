using System.ComponentModel.DataAnnotations;

namespace FantasyCritic.Web.Models.Requests.LeagueManager;

public class PromoteNewLeagueManagerRequest
{
    [Required]
    public Guid LeagueID { get; set; }
    [Required]
    public Guid NewManagerUserID { get; set; }
}
