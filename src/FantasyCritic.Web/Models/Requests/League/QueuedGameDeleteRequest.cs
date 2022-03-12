using System;
using System.ComponentModel.DataAnnotations;

namespace FantasyCritic.Web.Models.Requests.League
{
    public class QueuedGameDeleteRequest
    {
        [Required]
        public Guid PublisherID { get; set; }
        [Required]
        public Guid MasterGameID { get; set; }
    }
}
