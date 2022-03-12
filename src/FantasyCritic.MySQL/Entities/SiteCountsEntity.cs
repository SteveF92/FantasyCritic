namespace FantasyCritic.MySQL.Entities
{
    public class SiteCountsEntity
    {
        public int UserCount { get; set; }
        public int LeagueCount { get; set; }
        public int MasterGameCount { get; set; }
        public int PublisherGameCount { get; set; }

        public SiteCounts ToDomain()
        {
            return new SiteCounts(UserCount, LeagueCount, MasterGameCount, PublisherGameCount);
        }
    }
}
