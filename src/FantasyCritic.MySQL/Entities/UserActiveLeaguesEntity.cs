using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FantasyCritic.MySQL.Entities
{
    public class UserActiveLeaguesEntity
    {
        public Guid UserID { get; set; }
        public Guid LeagueID { get; set; }
        public int Year { get; set; }
    }
}
