using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FantasyCritic.Web.Models.Requests.LeagueManager
{
    public class DeleteInviteLinkRequest
    {
        public Guid LeagueID { get; set; }
        public Guid InviteID { get; set; }
    }
}
