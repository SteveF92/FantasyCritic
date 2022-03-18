using System.ComponentModel.DataAnnotations;

namespace FantasyCritic.Web.Models.Requests.LeagueManager;

public class GameRemoveRequest
{
    [Required]
    public Guid PublisherGameID { get; set; }
    [Required]
    public Guid PublisherID { get; set; }
    [Required]
    public Guid LeagueID { get; set; }
    [Required]
    public int Year { get; set; }
}
