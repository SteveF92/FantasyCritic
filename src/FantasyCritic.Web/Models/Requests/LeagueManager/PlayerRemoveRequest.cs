using System.ComponentModel.DataAnnotations;

namespace FantasyCritic.Web.Models.Requests.LeagueManager
{
    public class PlayerRemoveRequest
    {
        [Required]
        public Guid UserID { get; set; }
        [Required]
        public Guid LeagueID { get; set; }
    }
}
