using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FantasyCritic.Lib.Domain;
using FantasyCritic.Lib.Domain.ScoringSystems;
using NodaTime;

namespace FantasyCritic.Web.Models.Responses
{
    public class PlayerWithPublisherViewModel
    {
        public PlayerWithPublisherViewModel(Guid inviteID, string inviteName)
        {
            InviteID = inviteID;
            InviteName = inviteName;
        }

        public PlayerWithPublisherViewModel(LeagueYear leagueYear, FantasyCriticUser user, bool removable)
        {
            User = new PlayerViewModel(leagueYear.League, user, removable);
        }

        public PlayerWithPublisherViewModel(LeagueYear leagueYear, FantasyCriticUser user, Publisher publisher, IClock clock,
            LeagueOptions options, SystemWideValues systemWideValues, bool userIsInLeague, bool userIsInvitedToLeague,
            SupportedYear supportedYear, bool removable, bool previousYearWinner)
        {
            User = new PlayerViewModel(leagueYear.League, user, removable);
            Publisher = new PublisherViewModel(publisher, clock, userIsInLeague, userIsInvitedToLeague, systemWideValues, supportedYear.Finished);
            TotalFantasyPoints = publisher.TotalFantasyPoints;
            SimpleProjectedFantasyPoints = publisher.GetProjectedFantasyPoints(options, systemWideValues, supportedYear.Finished, true, clock);
            AdvancedProjectedFantasyPoints = publisher.GetProjectedFantasyPoints(options, systemWideValues, supportedYear.Finished, false, clock);
            PreviousYearWinner = previousYearWinner;
        }

        public Guid InviteID { get; }
        public string InviteName { get; }
        public PlayerViewModel User { get; }
        public PublisherViewModel Publisher { get; }
        public decimal TotalFantasyPoints { get; }
        public decimal SimpleProjectedFantasyPoints { get; }
        public decimal AdvancedProjectedFantasyPoints { get; }
        public bool PreviousYearWinner { get; }
    }
}
