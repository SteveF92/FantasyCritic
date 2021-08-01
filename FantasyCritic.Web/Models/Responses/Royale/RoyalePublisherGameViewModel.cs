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
        public RoyalePublisherGameViewModel(RoyalePublisherGame domain, LocalDate currentDate)
        {
            PublisherID = domain.PublisherID;
            YearQuarter = new RoyaleYearQuarterViewModel(domain.YearQuarter);
            MasterGame = new MasterGameYearViewModel(domain.MasterGame, currentDate);
            Locked = domain.IsLocked(currentDate);
            Timestamp = domain.Timestamp;
            AmountSpent = domain.AmountSpent;
            AdvertisingMoney = domain.AdvertisingMoney;
            FantasyPoints = domain.FantasyPoints;
        }

        public Guid PublisherID { get; }
        public RoyaleYearQuarterViewModel YearQuarter { get; }
        public MasterGameYearViewModel MasterGame { get; }
        public bool Locked { get; }
        public Instant Timestamp { get; }
        public decimal AmountSpent { get; }
        public decimal AdvertisingMoney { get; }
        public decimal? FantasyPoints { get; }
    }
}
