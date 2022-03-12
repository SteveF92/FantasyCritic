using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FantasyCritic.Web.Models.Responses.Royale;
using FantasyCritic.Web.Models.RoundTrip;

namespace FantasyCritic.Web.Models.Requests.Royale
{
    public class CreateRoyalePublisherRequest
    {
        public int Year { get; set; }
        public int Quarter { get; set; }
        public string PublisherName { get; set; }
    }
}
