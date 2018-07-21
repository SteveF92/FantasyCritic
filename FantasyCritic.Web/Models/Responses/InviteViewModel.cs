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
        public InviteViewModel(FantasyCriticLeague league)
        {
            LeagueID = league.LeagueID;
            LeagueName = league.LeagueName;
        }

        public Guid LeagueID { get; }
        public string LeagueName { get; }
    }
}
