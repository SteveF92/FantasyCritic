using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FantasyCritic.MySQL.Entities
{
    public class LeagueTagOverride
    {
        public Guid LeagueID { get; set; }
        public int Year { get; set; }
        public Guid MasterGameID { get; set; }
        public string TagName { get; set; }
    }
}
