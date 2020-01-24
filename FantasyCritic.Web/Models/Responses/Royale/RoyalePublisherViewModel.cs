using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FantasyCritic.Lib.Royale;
using NodaTime;

namespace FantasyCritic.Web.Models.Responses.Royale
{
    public class RoyalePublisherViewModel
    {
        public RoyalePublisherViewModel(RoyalePublisher domain, IClock clock, int? ranking)
        {
            PublisherID = domain.PublisherID;
            YearQuarter = new RoyaleYearQuarterViewModel(domain.YearQuarter);
            PlayerName = domain.User.DisplayName;
            UserID = domain.User.UserID;
            PublisherName = domain.PublisherName;
            PublisherGames = domain.PublisherGames.Select(x => new RoyalePublisherGameViewModel(x, clock)).ToList();
            Budget = domain.Budget;
            TotalFantasyPoints = domain.GetTotalFantasyPoints();
            if (TotalFantasyPoints > 0)
            {
                Ranking = ranking;
            }
        }

        public Guid PublisherID { get; }
        public RoyaleYearQuarterViewModel YearQuarter { get; }
        public Guid UserID { get; }
        public string PlayerName { get; }
        public string PublisherName { get; }
        public IReadOnlyList<RoyalePublisherGameViewModel> PublisherGames { get; }
        public decimal Budget { get; }
        public decimal TotalFantasyPoints { get; }
        public int? Ranking { get; }
    }
}
