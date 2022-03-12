using FantasyCritic.Lib.Royale;

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
            Timestamp = domainPublisherGame.Timestamp;
            AmountSpent = domainPublisherGame.AmountSpent;
            AdvertisingMoney = domainPublisherGame.AdvertisingMoney;
            FantasyPoints = domainPublisherGame.FantasyPoints;
        }

        public Guid PublisherID { get; set; }
        public Guid MasterGameID { get; set; }
        public Instant Timestamp { get; set; }
        public decimal AmountSpent { get; set; }
        public decimal AdvertisingMoney { get; set; }
        public decimal? FantasyPoints { get; set; }

        public RoyalePublisherGame ToDomain(RoyaleYearQuarter yearQuarter, MasterGameYear masterGameYear)
        {
            return new RoyalePublisherGame(PublisherID, yearQuarter, masterGameYear, Timestamp, AmountSpent, AdvertisingMoney, FantasyPoints);
        }
    }
}
