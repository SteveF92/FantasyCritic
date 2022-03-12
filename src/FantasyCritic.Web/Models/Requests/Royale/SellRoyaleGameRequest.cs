using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FantasyCritic.Lib.Royale;

namespace FantasyCritic.Web.Models.Requests.Royale
{
    public class SellRoyaleGameRequest
    {
        public Guid PublisherID { get; set; }
        public Guid MasterGameID { get; set; }
    }
}
