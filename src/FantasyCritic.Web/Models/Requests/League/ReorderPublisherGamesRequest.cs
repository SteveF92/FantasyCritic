using System.ComponentModel.DataAnnotations;

namespace FantasyCritic.Web.Models.Requests.League;

public class ReorderPublisherGamesRequest
{
    [Required]
    public Guid PublisherID { get; set; }
    [Required]
    public Dictionary<int, Guid?> SlotStates { get; set; }
}
