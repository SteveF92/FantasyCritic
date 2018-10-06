using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FantasyCritic.Web.Models.Requests
{
    public class ManualPublisherGameScoreRequest
    {
        [Required]
        public Guid PublisherID { get; set; }
        [Required]
        public Guid PublisherGameID { get; set; }
        [Required]
        public decimal? ManualCriticScore { get; set; }
    }
}
