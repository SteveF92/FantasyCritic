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
        public PlayerWithPublisherViewModel(string invitedEmailAddress)
        {
            InvitedEmailAddress = invitedEmailAddress;
        }

        public PlayerWithPublisherViewModel(LeagueYear leagueYear, FantasyCriticUser user)
        {
            User = new PlayerViewModel(leagueYear.League, user);
        }

        public PlayerWithPublisherViewModel(LeagueYear leagueYear, FantasyCriticUser user, Publisher publisher, IClock clock,
            LeagueOptions options, SystemWideValues systemWideValues, bool userIsInLeague, bool userIsInvitedToLeague, SupportedYear supportedYear)
        {
            User = new PlayerViewModel(leagueYear.League, user);
            Publisher = new PublisherViewModel(publisher, clock, userIsInLeague, leagueYear.League.PublicLeague, userIsInvitedToLeague);
            TotalFantasyPoints = publisher.TotalFantasyPoints;
            ProjectedFantasyPoints = publisher.GetProjectedFantasyPoints(options, systemWideValues, supportedYear.Finished);
        }

        public string InvitedEmailAddress { get; }
        public PlayerViewModel User { get; }
        public PublisherViewModel Publisher { get; }
        public decimal TotalFantasyPoints { get; }
        public decimal ProjectedFantasyPoints { get; }

    }
}
