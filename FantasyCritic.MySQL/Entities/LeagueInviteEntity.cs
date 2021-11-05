using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FantasyCritic.Lib.Domain;
using FantasyCritic.Lib.Identity;

namespace FantasyCritic.MySQL.Entities
{
    internal class LeagueInviteEntity
    {
        public LeagueInviteEntity()
        {

        }

        public LeagueInviteEntity(LeagueInvite domain)
        {
            InviteID = domain.InviteID;
            LeagueID = domain.League.LeagueID;

            EmailAddress = domain.EmailAddress;

            if (domain.User.HasValue)
            {
                UserID = domain.User.Value.Id;
            }
        }

        public Guid InviteID { get; set; }
        public Guid LeagueID { get; set; }
        public string EmailAddress { get; set; }
        public Guid? UserID { get; set; }

        public LeagueInvite ToDomain(League league)
        {
            return new LeagueInvite(InviteID, league, EmailAddress);
        }

        public LeagueInvite ToDomain(League league, FantasyCriticUser user)
        {
            return new LeagueInvite(InviteID, league, user);
        }
    }
}
