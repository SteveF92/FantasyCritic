using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FantasyCritic.MySQL.Entities
{
    internal class LeagueInviteEntity
    {
        public Guid? InviteID { get; set; }
        public Guid LeagueID { get; set; }
        public string EmailAddress { get; set; }

    }
}
