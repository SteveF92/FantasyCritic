namespace FantasyCritic.Web.Models.Responses;

public record GameNewsViewModel(IReadOnlyList<SingleGameNewsViewModel> UpcomingGames, IReadOnlyList<SingleGameNewsViewModel> RecentGames);