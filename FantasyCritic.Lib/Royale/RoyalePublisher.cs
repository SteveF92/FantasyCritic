using FantasyCritic.Lib.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NodaTime;

namespace FantasyCritic.Lib.Royale
{
    public class RoyalePublisher
    {
        public RoyalePublisher(Guid publisherID, RoyaleYearQuarter yearQuarter, FantasyCriticUser user, 
            string publisherName, IEnumerable<RoyalePublisherGame> publisherGames, decimal budget)
        {
            PublisherID = publisherID;
            YearQuarter = yearQuarter;
            User = user;
            PublisherName = publisherName;
            PublisherGames = publisherGames.ToList();
            Budget = budget;
        }

        public Guid PublisherID { get; }
        public RoyaleYearQuarter YearQuarter { get; }
        public FantasyCriticUser User { get; }
        public string PublisherName { get; }
        public IReadOnlyList<RoyalePublisherGame> PublisherGames { get; }
        public decimal Budget { get; }

        public decimal GetTotalFantasyPoints(IClock clock)
        {
            var points = PublisherGames.Where(x => x.MasterGame.MasterGame.IsReleased(clock) && x.FantasyPoints.HasValue)
                .Sum(x => x.FantasyPoints.Value);
            return points;
        }
    }
}
