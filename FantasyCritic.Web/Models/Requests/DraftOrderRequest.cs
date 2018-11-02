using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FantasyCritic.Web.Models.Requests
{
    public class DraftOrderRequest
    {
        public Guid LeagueID { get; set; }
        public int Year { get; set; }
        public List<Guid> PublisherDraftPositions { get; set; }
    }
}
