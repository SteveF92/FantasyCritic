using System.ComponentModel.DataAnnotations;

namespace FantasyCritic.Web.Models.Requests.LeagueManager;

public class AssociateGameRequest
{
    [Required]
    public Guid PublisherID { get; set; }
    [Required]
    public Guid PublisherGameID { get; set; }
    [Required]
    public Guid MasterGameID { get; set; }
    [Required]
    public bool ManagerOverride { get; set; }
}
