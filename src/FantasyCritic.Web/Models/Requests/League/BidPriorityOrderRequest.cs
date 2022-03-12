using System.ComponentModel.DataAnnotations;

namespace FantasyCritic.Web.Models.Requests.League;

public class BidPriorityOrderRequest
{
    [Required]
    public Guid PublisherID { get; set; }
    [Required]
    public List<Guid> BidPriorities { get; set; }
}
