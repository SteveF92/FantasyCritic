using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FantasyCritic.Lib.Domain;
using NodaTime;

namespace FantasyCritic.Web.Models.Responses
{
    public class LeagueInviteLinkViewModel
    {
        public LeagueInviteLinkViewModel(LeagueInviteLink domain, int currentYear, string baseURL)
        {
            InviteID = domain.InviteID;
            LeagueID = domain.League.LeagueID;
            InviteCode = domain.InviteCode;
            FullInviteLink = baseURL + $"/league/{LeagueID}/{currentYear}?inviteCode={InviteCode}";
        }

        public Guid InviteID { get; }
        public Guid LeagueID { get; }
        public Guid InviteCode { get; }
        public string FullInviteLink { get; }
    }
}
