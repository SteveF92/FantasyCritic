using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using FantasyCritic.Lib.Identity;

namespace FantasyCritic.Lib.Domain
{
    public class LeagueInvite
    {
        public LeagueInvite(Guid inviteID, League league, string emailAddress)
        {
            InviteID = inviteID;
            League = league;
            EmailAddress = emailAddress;
            User = Maybe<FantasyCriticUser>.None; 
        }

        public LeagueInvite(Guid inviteID, League league, FantasyCriticUser user)
        {
            InviteID = inviteID;
            League = league;
            EmailAddress = user.EmailAddress;
            User = user;
        }

        public Guid InviteID { get; }
        public League League { get; }
        public string EmailAddress { get; }
        public Maybe<FantasyCriticUser> User { get; }
    }
}