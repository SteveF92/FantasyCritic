using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;

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
            EmailAddress = Maybe<string>.None;
            User = user;
        }

        public Guid InviteID { get; }
        public League League { get; }
        public Maybe<string> EmailAddress { get; }
        public Maybe<FantasyCriticUser> User { get; }
    }
}