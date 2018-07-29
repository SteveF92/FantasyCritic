using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FantasyCritic.Lib.Domain
{
    public class Publisher
    {
        public Publisher(League league, FantasyCriticUser user, int year, IEnumerable<PublisherGame> publisherGames)
        {
            League = league;
            User = user;
            Year = year;
            PublisherGames = publisherGames.ToList();
        }

        public League League { get; }
        public FantasyCriticUser User { get; }
        public int Year { get; }
        public IReadOnlyList<PublisherGame> PublisherGames { get; }
    }
}
