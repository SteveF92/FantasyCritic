using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FantasyCritic.Lib.Domain;

namespace FantasyCritic.MySQL.Entities
{
    internal class LeagueYearEntity
    {
        public LeagueYearEntity()
        {

        }

        public LeagueYearEntity(FantasyCriticLeague league, int year)
        {
            LeagueID = league.LeagueID;
            Year = year;
        }

        public Guid LeagueID { get; set; }
        public int Year { get; set; }
    }
}
