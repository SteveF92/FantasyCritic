using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FantasyCritic.Lib.Domain;

namespace FantasyCritic.Web.Models.Responses
{
    public class InviteViewModel
    {
        public InviteViewModel(League league)
        {
            LeagueID = league.LeagueID;
            LeagueName = league.LeagueName;
            ActiveYear = league.Years.Max();
        }

        public Guid LeagueID { get; }
        public string LeagueName { get; }
        public int ActiveYear { get; }
    }
}
