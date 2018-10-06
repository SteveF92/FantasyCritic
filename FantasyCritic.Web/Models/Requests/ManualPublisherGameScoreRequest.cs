using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FantasyCritic.Web.Models.Requests
{
    public class ManualPublisherGameScoreRequest
    {
        public Guid PublisherGameID { get; set; }
        public decimal? ManualCriticScore { get; set; }
    }
}
