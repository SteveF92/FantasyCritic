using FantasyCritic.Lib.Domain.AllTimeStats;

namespace FantasyCritic.Web.Models.Responses.AllTimeStats;

public class LeaguePlayerAllTimeStatsResponse
{
    public LeaguePlayerAllTimeStatsResponse(LeaguePlayerAllTimeStats domain)
    {
        UserID = domain.User.UserID;
        PlayerName = domain.User.DisplayName;
        YearsPlayedIn = domain.YearsPlayedIn;
        YearsWon = domain.YearsWon;
        TotalFantasyPoints = domain.TotalFantasyPoints;
        GamesReleased = domain.GamesReleased;
        AverageFinishRanking = domain.AverageFinishRanking;
        AverageGamesReleased = domain.AverageGamesReleased;
        AverageFantasyPoints = domain.AverageFantasyPoints;
        AverageCriticScore = domain.AverageCriticScore;
        TimesCounterPicked = domain.TimesCounterPicked;
    }

    public Guid UserID { get; }
    public string PlayerName { get; }
    public int YearsPlayedIn { get; }
    public IReadOnlyList<int> YearsWon { get; }
    public decimal TotalFantasyPoints { get; }
    public int GamesReleased { get; }
    public double AverageFinishRanking { get; }
    public double AverageGamesReleased { get; }
    public decimal AverageFantasyPoints { get; }
    public decimal AverageCriticScore { get; }
    public int TimesCounterPicked { get; }
}
