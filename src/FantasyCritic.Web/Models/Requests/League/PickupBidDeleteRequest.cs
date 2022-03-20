using System.ComponentModel.DataAnnotations;

namespace FantasyCritic.Web.Models.Requests.League;

public class PickupBidDeleteRequest
{
    [Required]
    public Guid LeagueID { get; set; }
    [Required]
    public int Year { get; set; }
    [Required]
    public Guid PublisherID { get; set; }
    [Required]
    public Guid BidID { get; set; }
}
