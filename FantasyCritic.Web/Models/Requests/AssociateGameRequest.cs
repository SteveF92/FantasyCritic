using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FantasyCritic.Web.Models.Requests
{
    public class AssociateGameRequest
    {
        public Guid PublisherID { get; set; }
        public Guid PublisherGameID { get; set; }
        public Guid MasterGameID { get; set; }
        public bool ManagerOverride { get; set; }
    }
}
