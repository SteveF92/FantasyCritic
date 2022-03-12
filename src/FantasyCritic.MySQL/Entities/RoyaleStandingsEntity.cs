using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FantasyCritic.MySQL.Entities
{
    public class RoyaleStandingsEntity
    {
        public Guid PublisherID { get; set; }
        public int Year { get; set; }
        public int Quarter { get; set; }
        public decimal TotalFantasyPoints { get; set; }
    }
}
