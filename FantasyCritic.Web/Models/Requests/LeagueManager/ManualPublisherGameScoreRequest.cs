using System;
using System.ComponentModel.DataAnnotations;

namespace FantasyCritic.Web.Models.Requests.LeagueManager
{
    public class ManualPublisherGameScoreRequest
    {
        [Required]
        public Guid PublisherID { get; set; }
        [Required]
        public Guid PublisherGameID { get; set; }
        [Required]
        public decimal ManualCriticScore { get; set; }
    }
}
