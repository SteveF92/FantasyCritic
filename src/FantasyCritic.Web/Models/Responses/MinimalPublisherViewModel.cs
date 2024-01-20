namespace FantasyCritic.Web.Models.Responses;
public class MinimalPublisherViewModel
{
    public MinimalPublisherViewModel(LeagueYear leagueYear, Publisher publisher, LocalDate currentDate, bool userIsInLeague,
        bool outstandingInvite, SystemWideValues systemWideValues)
    {
        PublisherID = publisher.PublisherID;
        LeagueID = leagueYear.League.LeagueID;
        UserID = publisher.User.Id;
        PublisherName = publisher.PublisherName;
        PublisherIcon = publisher.PublisherIcon;
        LeagueName = leagueYear.League.LeagueName;
        PlayerName = publisher.User.UserName;
        Year = leagueYear.Year;
        DraftPosition = publisher.DraftPosition;
        AutoDraftMode = publisher.AutoDraftMode.Value;

        AverageCriticScore = publisher.AverageCriticScore;
        TotalFantasyPoints = publisher.GetTotalFantasyPoints(leagueYear.SupportedYear, leagueYear.Options);
        TotalProjectedPoints = publisher.GetProjectedFantasyPoints(leagueYear, systemWideValues, currentDate);
        Budget = publisher.Budget;

        UserIsInLeague = userIsInLeague;
        PublicLeague = leagueYear.League.PublicLeague;
        OutstandingInvite = outstandingInvite;

        var dateToCheck = currentDate;
        if (leagueYear.SupportedYear.Finished)
        {
            dateToCheck = new LocalDate(Year, 12, 31);
        }

        GamesReleased = publisher.PublisherGames
            .Where(x => !x.CounterPick)
            .Where(x => x.MasterGame is not null)
            .Count(x => x.MasterGame!.MasterGame.IsReleased(dateToCheck));
        var allWillRelease = publisher.PublisherGames
            .Where(x => !x.CounterPick)
            .Where(x => x.MasterGame is not null)
            .Count(x => x.CouldRelease());
        GamesWillRelease = allWillRelease - GamesReleased;

        FreeGamesDropped = publisher.FreeGamesDropped;
        WillNotReleaseGamesDropped = publisher.WillNotReleaseGamesDropped;
        WillReleaseGamesDropped = publisher.WillReleaseGamesDropped;
        FreeDroppableGames = leagueYear.Options.FreeDroppableGames;
        WillNotReleaseDroppableGames = leagueYear.Options.WillNotReleaseDroppableGames;
        WillReleaseDroppableGames = leagueYear.Options.WillReleaseDroppableGames;
    }

    public Guid PublisherID { get; }
    public Guid LeagueID { get; }
    public Guid UserID { get; }
    public string PublisherName { get; }
    public string? PublisherIcon { get; }
    public string LeagueName { get; }
    public string PlayerName { get; }
    public int Year { get; }
    public int DraftPosition { get; }
    public string AutoDraftMode { get; }
    public decimal? AverageCriticScore { get; }
    public decimal TotalFantasyPoints { get; }
    public decimal TotalProjectedPoints { get; }
    public uint Budget { get; }
    public bool UserIsInLeague { get; }
    public bool PublicLeague { get; }
    public bool OutstandingInvite { get; }
    public int GamesReleased { get; }
    public int GamesWillRelease { get; }
    public int FreeGamesDropped { get; }
    public int WillNotReleaseGamesDropped { get; }
    public int WillReleaseGamesDropped { get; }
    public int FreeDroppableGames { get; }
    public int WillNotReleaseDroppableGames { get; }
    public int WillReleaseDroppableGames { get; }
}
