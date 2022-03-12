using System.ComponentModel.DataAnnotations;

namespace FantasyCritic.Web.Models.Requests.LeagueManager
{
    public class TagOverrideRequest
    {
        [Required]
        public Guid LeagueID { get; set; }
        [Required]
        public int Year { get; set; }
        [Required]
        public Guid MasterGameID { get; set; }
        [Required]
        public List<string> Tags { get; set; }
    }
}
