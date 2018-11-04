using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FantasyCritic.Lib.Domain;
using NodaTime;

namespace FantasyCritic.Web.Models.Responses
{
    public class PlayerWithPublisherViewModel
    {
        public PlayerWithPublisherViewModel(LeagueYear leagueYear, FantasyCriticUser user)
        {
            User = new PlayerViewModel(leagueYear.League, user);
        }

        public PlayerWithPublisherViewModel(LeagueYear leagueYear, FantasyCriticUser user, Publisher publisher, IClock clock)
        {
            User = new PlayerViewModel(leagueYear.League, user);
            Publisher = new PublisherViewModel(publisher, clock);
        }

        public PlayerViewModel User { get; }
        public PublisherViewModel Publisher { get; }
    }
}
