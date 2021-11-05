using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FantasyCritic.Lib.Domain;
using FantasyCritic.Lib.Royale;
using NodaTime;

namespace FantasyCritic.Web.Models.Responses.Royale
{
    public class RoyalePublisherViewModel
    {
        public RoyalePublisherViewModel(RoyalePublisher domain, LocalDate currentDate, int? ranking, IEnumerable<RoyaleYearQuarter> quartersWon,
            IEnumerable<MasterGameTag> allMasterGameTags)
        {
            PublisherID = domain.PublisherID;
            YearQuarter = new RoyaleYearQuarterViewModel(domain.YearQuarter);
            PlayerName = domain.User.UserName;
            UserID = domain.User.Id;
            PublisherName = domain.PublisherName;
            PublisherGames = domain.PublisherGames.Select(x => new RoyalePublisherGameViewModel(x, currentDate, allMasterGameTags)).ToList();
            Budget = domain.Budget;
            TotalFantasyPoints = domain.GetTotalFantasyPoints();
            if (TotalFantasyPoints > 0)
            {
                Ranking = ranking;
            }

            QuartersWon = quartersWon.Select(x => new RoyaleYearQuarterViewModel(x)).ToList();
            PreviousQuarterWinner = quartersWon.Select(x => x.YearQuarter).Contains(domain.YearQuarter.YearQuarter.LastQuarter);
            OneTimeWinner = quartersWon.Any();
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
        public IReadOnlyList<RoyaleYearQuarterViewModel> QuartersWon { get; }
        public bool PreviousQuarterWinner { get; }
        public bool OneTimeWinner { get; }
    }
}
