using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FantasyCritic.Lib.Domain;

namespace FantasyCritic.Web.Models.Responses
{
    public class LeagueInviteViewModel
    {
        private LeagueInviteViewModel(League league, string emailAddress, string displayName)
        {
            LeagueID = league.LeagueID;
            LeagueName = league.LeagueName;
            ActiveYear = league.Years.Max();
            EmailAddress = emailAddress;
            DisplayName = displayName;
        }

        public Guid LeagueID { get; }
        public string LeagueName { get; }
        public int ActiveYear { get; }
        public string EmailAddress { get; }
        public string DisplayName { get; }

        public static LeagueInviteViewModel CreateWithEmailAddress(LeagueInvite invite)
        {
            return new LeagueInviteViewModel(invite.League, invite.EmailAddress, null);
        }

        public static LeagueInviteViewModel CreateWithDisplayName(LeagueInvite invite)
        {
            if (invite.User.HasNoValue)
            {
                throw new Exception("Invite does not have a user attached.");
            }

            return new LeagueInviteViewModel(invite.League, null, invite.User.Value.DisplayName);
        }
    }
}
