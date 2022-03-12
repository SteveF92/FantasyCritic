using FantasyCritic.Lib.Identity;

namespace FantasyCritic.Lib.Domain.Calculations
{
    public class YearCalculatedStatsSet
    {
        public YearCalculatedStatsSet(IReadOnlyDictionary<Guid, PublisherGameCalculatedStats> publisherGameCalculatedStats,
            IReadOnlyDictionary<LeagueYearKey, FantasyCriticUser> winningUsers)
        {
            PublisherGameCalculatedStats = publisherGameCalculatedStats;
            WinningUsers = winningUsers;
        }

        public IReadOnlyDictionary<Guid, PublisherGameCalculatedStats> PublisherGameCalculatedStats { get; }
        public IReadOnlyDictionary<LeagueYearKey, FantasyCriticUser> WinningUsers { get; }
    }
}
