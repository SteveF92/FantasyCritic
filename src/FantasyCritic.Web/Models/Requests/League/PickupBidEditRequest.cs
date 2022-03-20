using System.ComponentModel.DataAnnotations;

namespace FantasyCritic.Web.Models.Requests.League;

public class PickupBidEditRequest
{
    [Required]
    public Guid PublisherID { get; set; }
    [Required]
    public Guid BidID { get; set; }
    public Guid? ConditionalDropPublisherGameID { get; set; }
    [Required]
    public uint BidAmount { get; set; }
}
