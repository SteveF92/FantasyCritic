using System.ComponentModel.DataAnnotations;

namespace FantasyCritic.Web.Models.Requests.LeagueManager;

public class GameRemoveRequest
{
    [Required]
    public Guid PublisherGameID { get; set; }
    [Required]
    public Guid PublisherID { get; set; }
}
