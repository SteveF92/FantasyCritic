using System.ComponentModel.DataAnnotations;

namespace FantasyCritic.Web.Models.Requests.League
{
    public class SetArchiveStatusRequest
    {
        [Required]
        public Guid LeagueID { get; set; }
        [Required]
        public bool Archive { get; set; }
    }
}
