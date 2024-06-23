namespace FantasyCritic.Web.Models.Responses;

public class GameNewsViewModel
{
    public GameNewsViewModel(IReadOnlyList<SingleGameNewsViewModel> upcomingGames, IReadOnlyList<SingleGameNewsViewModel> recentGames)
    {
        UpcomingGames = upcomingGames;
        RecentGames = recentGames;
    }

    public GameNewsViewModel(MyGameNewsSet domain, LocalDate currentDate)
    {
        UpcomingGames = domain.UpcomingGames.Select(x => new SingleGameNewsViewModel(x, currentDate)).ToList();
        RecentGames = domain.RecentGames.Select(x => new SingleGameNewsViewModel(x, currentDate)).ToList();
    }

    public IReadOnlyList<SingleGameNewsViewModel> UpcomingGames { get; }
    public IReadOnlyList<SingleGameNewsViewModel> RecentGames { get; }
}
