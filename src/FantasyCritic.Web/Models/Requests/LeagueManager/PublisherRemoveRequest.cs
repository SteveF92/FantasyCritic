using System.ComponentModel.DataAnnotations;

namespace FantasyCritic.Web.Models.Requests.LeagueManager
{
    public class PublisherRemoveRequest
    {
        [Required]
        public Guid PublisherID { get; set; }
        [Required]
        public Guid LeagueID { get; set; }
    }
}
