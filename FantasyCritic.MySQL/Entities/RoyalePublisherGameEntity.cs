using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FantasyCritic.Lib.Domain;
using FantasyCritic.Lib.Royale;
using NodaTime;
using Org.BouncyCastle.Asn1.X509;

namespace FantasyCritic.MySQL.Entities
{
    internal class RoyalePublisherGameEntity
    {
        public RoyalePublisherGameEntity()
        {

        }

        public RoyalePublisherGameEntity(RoyalePublisherGame domainPublisherGame)
        {
            PublisherID = domainPublisherGame.PublisherID;
            MasterGameID = domainPublisherGame.MasterGame.MasterGame.MasterGameID;
            Timestamp = domainPublisherGame.Timestamp.ToDateTimeUtc();
            AmountSpent = domainPublisherGame.AmountSpent;
            AdvertisingMoney = domainPublisherGame.AdvertisingMoney;
            FantasyPoints = domainPublisherGame.FantasyPoints;
        }

        public Guid PublisherID { get; set; }
        public Guid MasterGameID { get; set; }
        public DateTime Timestamp { get; set; }
        public decimal AmountSpent { get; set; }
        public decimal AdvertisingMoney { get; set; }
        public decimal? FantasyPoints { get; set; }

        public RoyalePublisherGame ToDomain(RoyaleYearQuarter yearQuarter, MasterGameYear masterGameYear)
        {
            return new RoyalePublisherGame(PublisherID, yearQuarter, masterGameYear, Instant.FromDateTimeUtc(Timestamp), AmountSpent, AdvertisingMoney, FantasyPoints);
        }
    }
}
