namespace FantasyCritic.Web.Models.Responses;

public class PublisherViewModel
{
    public PublisherViewModel(LeagueYear leagueYear, Publisher publisher, LocalDate currentDate, bool userIsInLeague,
        bool outstandingInvite, SystemWideValues systemWideValues, IReadOnlySet<Guid> counterPickedPublisherGameIDs)
        : this(leagueYear, publisher, currentDate, Maybe<Publisher>.None, userIsInLeague, outstandingInvite, systemWideValues, counterPickedPublisherGameIDs)
    {

    }

    public PublisherViewModel(LeagueYear leagueYear, Publisher publisher, LocalDate currentDate, Maybe<Publisher> nextDraftPublisher,
        bool userIsInLeague, bool outstandingInvite, SystemWideValues systemWideValues, IReadOnlySet<Guid> counterPickedPublisherGameIDs)
    {
        PublisherID = publisher.PublisherID;
        LeagueID = leagueYear.League.LeagueID;
        UserID = publisher.User.Id;
        PublisherName = publisher.PublisherName;
        PublisherIcon = publisher.PublisherIcon.GetValueOrDefault();
        LeagueName = leagueYear.League.LeagueName;
        PlayerName = publisher.User.UserName;
        Year = leagueYear.Year;
        DraftPosition = publisher.DraftPosition;
        AutoDraft = publisher.AutoDraft;

        Games = publisher.PublisherGames
            .OrderBy(x => x.Timestamp)
            .Select(x => new PublisherGameViewModel(x, currentDate, counterPickedPublisherGameIDs.Contains(x.PublisherGameID), leagueYear.Options.CounterPicksBlockDrops))
            .ToList();
        FormerGames = publisher.FormerPublisherGames
            .OrderBy(x => x.PublisherGame.Timestamp)
            .Select(x => new PublisherGameViewModel(x, currentDate))
            .ToList();
        GameSlots = publisher.GetPublisherSlots(leagueYear.Options)
            .Select(x => new PublisherSlotViewModel(x, currentDate, leagueYear, systemWideValues, counterPickedPublisherGameIDs))
            .ToList();

        AverageCriticScore = publisher.AverageCriticScore;
        TotalFantasyPoints = publisher.GetTotalFantasyPoints(leagueYear.SupportedYear, leagueYear.Options);

        var ineligiblePointsShouldCount = !SupportedYear.Year2022FeatureSupported(leagueYear.Year);
        TotalProjectedPoints = publisher.GetProjectedFantasyPoints(systemWideValues, false, currentDate, ineligiblePointsShouldCount, leagueYear);
        Budget = publisher.Budget;

        if (nextDraftPublisher.HasValue && nextDraftPublisher.Value.PublisherID == publisher.PublisherID)
        {
            NextToDraft = true;
        }

        UserIsInLeague = userIsInLeague;
        PublicLeague = leagueYear.Options.PublicLeague;
        OutstandingInvite = outstandingInvite;

        var dateToCheck = currentDate;
        if (leagueYear.SupportedYear.Finished)
        {
            dateToCheck = new LocalDate(Year, 12, 31);
        }

        GamesReleased = publisher.PublisherGames
            .Where(x => !x.CounterPick)
            .Where(x => x.MasterGame.HasValue)
            .Count(x => x.MasterGame.Value.MasterGame.IsReleased(dateToCheck));
        var allWillRelease = publisher.PublisherGames
            .Where(x => !x.CounterPick)
            .Where(x => x.MasterGame.HasValue)
            .Count(x => x.WillRelease());
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
    public string PublisherIcon { get; }
    public string LeagueName { get; }
    public string PlayerName { get; }
    public int Year { get; }
    public int DraftPosition { get; }
    public bool AutoDraft { get; }
    public IReadOnlyList<PublisherGameViewModel> Games { get; }
    public IReadOnlyList<PublisherGameViewModel> FormerGames { get; }
    public IReadOnlyList<PublisherSlotViewModel> GameSlots { get; }
    public decimal? AverageCriticScore { get; }
    public decimal TotalFantasyPoints { get; }
    public decimal TotalProjectedPoints { get; }
    public uint Budget { get; }
    public bool NextToDraft { get; }
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
