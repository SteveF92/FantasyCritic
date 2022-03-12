using System.ComponentModel.DataAnnotations;

namespace FantasyCritic.Web.Models.Requests.LeagueManager
{
    public class ManagerSetAutoDraftRequest
    {
        [Required]
        public Guid LeagueID { get; set; }
        [Required]
        public int Year { get; set; }
        [Required]
        public Dictionary<Guid, bool> PublisherAutoDraft { get; set; }
    }
}
