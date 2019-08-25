using FantasyCritic.Lib.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FantasyCritic.Lib.Royale
{
    public class RoyalePublisher
    {
        public RoyalePublisher(Guid publisherID, YearQuarter yearQuarter, FantasyCriticUser user, 
            string publisherName, IEnumerable<RoyalePublisherGame> publisherGames, uint budget)
        {
            PublisherID = publisherID;
            YearQuarter = yearQuarter;
            User = user;
            PublisherName = publisherName;
            PublisherGames = publisherGames.ToList();
            Budget = budget;
        }

        public Guid PublisherID { get; private set; }
        public YearQuarter YearQuarter { get; }
        public FantasyCriticUser User { get; }
        public string PublisherName { get; }
        public IReadOnlyList<RoyalePublisherGame> PublisherGames { get; }
        public uint Budget { get; }
    }
}
