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
    public class RoyalePublisherGameViewModel
    {
        public RoyalePublisherGameViewModel(RoyalePublisherGame domain, IClock clock)
        {
            PublisherID = domain.PublisherID;
            YearQuarter = new RoyaleYearQuarterViewModel(domain.YearQuarter);
            MasterGame = new MasterGameYearViewModel(domain.MasterGame, clock);
            Timestamp = domain.Timestamp;
            AmountSpent = domain.AmountSpent;
            AdvertisingMoney = domain.AdvertisingMoney;
            FantasyPoints = domain.FantasyPoints;
        }

        public Guid PublisherID { get; }
        public RoyaleYearQuarterViewModel YearQuarter { get; }
        public MasterGameYearViewModel MasterGame { get; }
        public Instant Timestamp { get; }
        public decimal AmountSpent { get; }
        public decimal AdvertisingMoney { get; }
        public decimal? FantasyPoints { get; }
    }
}
