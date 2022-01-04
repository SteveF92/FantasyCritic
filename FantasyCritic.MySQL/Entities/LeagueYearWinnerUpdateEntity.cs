using FantasyCritic.Lib.Domain;
using FantasyCritic.Lib.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FantasyCritic.MySQL.Entities
{
    public class LeagueYearWinnerUpdateEntity
    {
        public LeagueYearWinnerUpdateEntity(KeyValuePair<LeagueYearKey, FantasyCriticUser> keyValuePair)
        {
            LeagueID = keyValuePair.Key.LeagueID;
            Year = keyValuePair.Key.Year;
            WinningUserID = keyValuePair.Value.Id;
        }

        public Guid LeagueID { get; set; }
        public int Year { get; set; }
        public Guid WinningUserID { get; set; }
    }
}
