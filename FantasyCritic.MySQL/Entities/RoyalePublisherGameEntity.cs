using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FantasyCritic.Lib.Domain;
using FantasyCritic.Lib.Extensions;
using FantasyCritic.Lib.Royale;
using FantasyCritic.Lib.Utilities;
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
            if (Timestamp.Kind != DateTimeKind.Utc)
            {
                LocalDateTime localDateTime = LocalDateTime.FromDateTime(Timestamp);
                Instant instant = localDateTime.InZoneStrictly(TimeExtensions.EasternTimeZone).ToInstant();
                return new RoyalePublisherGame(PublisherID, yearQuarter, masterGameYear, instant, AmountSpent, AdvertisingMoney, FantasyPoints);
            }
            return new RoyalePublisherGame(PublisherID, yearQuarter, masterGameYear, Instant.FromDateTimeUtc(Timestamp), AmountSpent, AdvertisingMoney, FantasyPoints);
        }
    }
}
