using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FantasyCritic.Web.Models.Requests.League
{
    public class ReorderPublisherGamesRequest
    {
        public Guid PublisherID { get; set; }
        public Dictionary<int, Guid?> SlotStates { get; set; }
    }
}
