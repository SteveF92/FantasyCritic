using FantasyCritic.Lib.Identity;

namespace FantasyCritic.Lib.Domain.AllTimeStats;
public record LeaguePlayerAllTimeStats(VeryMinimalFantasyCriticUser User, int YearsPlayedIn, IReadOnlyList<int> YearsWon, decimal TotalFantasyPoints, int GamesReleased,
    double AverageFinishRanking, double AverageGamesReleased, decimal AverageFantasyPoints, decimal AverageCriticScore, int TimesCounterPicked);
