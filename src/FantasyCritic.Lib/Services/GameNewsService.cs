using FantasyCritic.Lib.BusinessLogicFunctions;
using FantasyCritic.Lib.Domain.Combinations;

namespace FantasyCritic.Lib.Services;
public class GameNewsService
{
    private readonly InterLeagueService _interLeagueService;
    private readonly LeagueMemberService _leagueMemberService;

    public GameNewsService(InterLeagueService interLeagueService, LeagueMemberService leagueMemberService)
    {
        _interLeagueService = interLeagueService;
        _leagueMemberService = leagueMemberService;
    }

    public IReadOnlyList<IGrouping<MasterGameYear, PublisherGame>> GetGameNewsForPublishers(IReadOnlyList<LeagueYearPublisherPair> leagueYearPublisherPairs, LocalDate currentDate, bool recentReleases)
    {
        var gameNewsUpcoming = GameNewsFunctions.GetGameNews(leagueYearPublisherPairs, recentReleases, currentDate);
        return gameNewsUpcoming;
    }

    public Dictionary<MasterGameYear, List<LeagueYearPublisherPair>> GetLeagueYearPublisherLists(IReadOnlyList<LeagueYearPublisherPair> publishers, IReadOnlyList<IGrouping<MasterGameYear, PublisherGame>> gameNews)
    {
        var leagueYearPublisherLists = new Dictionary<MasterGameYear, List<LeagueYearPublisherPair>>();

        foreach (var publisherGameGroup in gameNews)
        {
            var publishersThatHaveGame = publishers.Where(x => publisherGameGroup.Select(y => y.PublisherID).Contains(x.Publisher.PublisherID)).ToList();
            leagueYearPublisherLists.Add(publisherGameGroup.Key, publishersThatHaveGame);
        }

        return leagueYearPublisherLists;
    }
}
