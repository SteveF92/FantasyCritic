using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FantasyCritic.Lib.Domain
{
    public class Publisher
    {
        public Publisher(Guid publisherID, League league, FantasyCriticUser user, int year, string publisherName, IEnumerable<PublisherGame> publisherGames)
        {
            PublisherID = publisherID;
            League = league;
            User = user;
            Year = year;
            PublisherName = publisherName;
            PublisherGames = publisherGames.ToList();
        }

        public Guid PublisherID { get; }
        public League League { get; }
        public FantasyCriticUser User { get; }
        public int Year { get; }
        public string PublisherName { get; }
        public IReadOnlyList<PublisherGame> PublisherGames { get; }
    }
}
