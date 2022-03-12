using System.ComponentModel.DataAnnotations;
using FantasyCritic.Lib.Domain.Requests;

namespace FantasyCritic.Web.Models.Requests.LeagueManager
{
    public class PublisherEditRequest
    {
        [Required]
        public Guid PublisherID { get; set; }
        [Required]
        public Guid LeagueID { get; set; }
        [Required]
        public string PublisherName { get; set; }
        [Required]
        public int Budget { get; set; }
        [Required]
        public int FreeGamesDropped { get; set; }
        [Required]
        public int WillNotReleaseGamesDropped { get; set; }
        [Required]
        public int WillReleaseGamesDropped { get; set; }

        public EditPublisherRequest ToDomain(Publisher publisher)
        {
            return new EditPublisherRequest(publisher, PublisherName, Budget, FreeGamesDropped, WillNotReleaseGamesDropped, WillReleaseGamesDropped);
        }
    }
}
