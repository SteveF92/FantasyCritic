using System.ComponentModel.DataAnnotations;

namespace FantasyCritic.Web.Models.Requests.League
{
    public class PickupBidDeleteRequest
    {
        [Required]
        public Guid BidID { get; set; }
    }
}
