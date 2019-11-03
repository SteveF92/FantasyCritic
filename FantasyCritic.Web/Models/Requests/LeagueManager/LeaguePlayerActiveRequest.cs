using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FantasyCritic.Web.Models.Requests.LeagueManager
{
    public class LeaguePlayerActiveRequest
    {
        public Guid LeagueID { get; set; }
        public int Year { get; set; }
        public Guid UserID { get; set; }
    }
}
