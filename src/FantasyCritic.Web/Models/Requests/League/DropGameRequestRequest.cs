using System.ComponentModel.DataAnnotations;

namespace FantasyCritic.Web.Models.Requests.League
{
    public class DropGameRequestRequest
    {
        [Required]
        public Guid PublisherID { get; set; }
        [Required]
        public Guid PublisherGameID { get; set; }
    }
}
