using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FantasyCritic.Lib.Domain;

namespace FantasyCritic.MySQL.Entities
{
    internal class LeagueInviteLinkEntity
    {
        public LeagueInviteLinkEntity()
        {

        }

        public LeagueInviteLinkEntity(LeagueInviteLink domain)
        {
            InviteID = domain.InviteID;
            LeagueID = domain.League.LeagueID;
            InviteCode = domain.InviteCode;
            Active = domain.Active;
        }

        public Guid InviteID { get; set; }
        public Guid LeagueID { get; set; }
        public Guid InviteCode { get; set; }
        public bool Active { get; set; }

        public LeagueInviteLink ToDomain(League league)
        {
            return new LeagueInviteLink(InviteID, league, InviteCode, Active);
        }
    }
}
