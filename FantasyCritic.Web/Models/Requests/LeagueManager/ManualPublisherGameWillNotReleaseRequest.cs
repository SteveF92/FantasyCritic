using System;
using System.ComponentModel.DataAnnotations;

namespace FantasyCritic.Web.Models.Requests.LeagueManager
{
    public class ManualPublisherGameWillNotReleaseRequest
    {
        [Required]
        public Guid PublisherID { get; set; }
        [Required]
        public Guid PublisherGameID { get; set; }
        [Required]
        public bool WillNotRelease { get; set; }
    }
}
