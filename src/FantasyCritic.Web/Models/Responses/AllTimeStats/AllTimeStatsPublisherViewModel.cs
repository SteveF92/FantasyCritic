namespace FantasyCritic.Web.Models.Responses.AllTimeStats;

public class AllTimeStatsPublisherViewModel
{
    public AllTimeStatsPublisherViewModel(LeagueYear leagueYear, Publisher publisher, int ranking, SystemWideValues systemWideValues, LocalDate currentDate)
    {
        PublisherID = publisher.PublisherID;
        UserID = publisher.User.Id;
        PublisherName = publisher.PublisherName;
        LeagueName = leagueYear.League.LeagueName;
        PlayerName = publisher.User.UserName;
        Year = publisher.LeagueYearKey.Year;
        DraftPosition = publisher.DraftPosition;

        AverageCriticScore = publisher.AverageCriticScore;
        TotalFantasyPoints = publisher.GetTotalFantasyPoints(leagueYear.SupportedYear, leagueYear.Options);
        TotalProjectedPoints = publisher.GetProjectedFantasyPoints(leagueYear, systemWideValues);
        Ranking = ranking;
        Budget = publisher.Budget;

        var dateToCheck = currentDate;
        if (leagueYear.SupportedYear.Finished)
        {
            dateToCheck = new LocalDate(Year, 12, 31);
        }

        GamesReleased = publisher.PublisherGames
            .Where(x => !x.CounterPick)
            .Where(x => x.MasterGame is not null)
            .Count(x => x.MasterGame!.MasterGame.IsReleased(dateToCheck));

        FreeGamesDropped = publisher.FreeGamesDropped;
        WillNotReleaseGamesDropped = publisher.WillNotReleaseGamesDropped;
        WillReleaseGamesDropped = publisher.WillReleaseGamesDropped;
        FreeDroppableGames = leagueYear.Options.FreeDroppableGames;
        WillNotReleaseDroppableGames = leagueYear.Options.WillNotReleaseDroppableGames;
        WillReleaseDroppableGames = leagueYear.Options.WillReleaseDroppableGames;
    }

    public Guid PublisherID { get; }
    public Guid UserID { get; }
    public string PublisherName { get; }
    public string LeagueName { get; }
    public string PlayerName { get; }
    public int Year { get; }
    public int DraftPosition { get; }
    public decimal? AverageCriticScore { get; }
    public decimal TotalFantasyPoints { get; }
    public decimal TotalProjectedPoints { get; }
    public int Ranking { get; }
    public uint Budget { get; }

    public int GamesReleased { get; }
    public int FreeGamesDropped { get; }
    public int WillNotReleaseGamesDropped { get; }
    public int WillReleaseGamesDropped { get; }
    public int FreeDroppableGames { get; }
    public int WillNotReleaseDroppableGames { get; }
    public int WillReleaseDroppableGames { get; }
}
