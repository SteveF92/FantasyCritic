
using FantasyCritic.Lib.Identity;

namespace FantasyCritic.MySQL.Entities;

public class PublisherEntity
{
    public PublisherEntity()
    {

    }

    public PublisherEntity(Publisher publisher)
    {
        PublisherID = publisher.PublisherID;
        PublisherName = publisher.PublisherName;
        PublisherIcon = publisher.PublisherIcon;
        PublisherSlogan = publisher.PublisherSlogan;
        LeagueID = publisher.LeagueYearKey.LeagueID;
        Year = publisher.LeagueYearKey.Year;
        UserID = publisher.User.Id;
        DraftPosition = publisher.DraftPosition;
        FreeGamesDropped = publisher.FreeGamesDropped;
        WillNotReleaseGamesDropped = publisher.WillNotReleaseGamesDropped;
        WillReleaseGamesDropped = publisher.WillReleaseGamesDropped;
        SuperDropsAvailable = publisher.SuperDropsAvailable;
        Budget = publisher.Budget;
        AutoDraftMode = publisher.AutoDraftSettings.Mode.Value;
        OnlyAutoDraftFromWatchlist = publisher.AutoDraftSettings.OnlyDraftFromWatchlist;
    }

    public Guid PublisherID { get; set; }
    public string PublisherName { get; set; } = null!;
    public string? PublisherIcon { get; set; }
    public string? PublisherSlogan { get; set; }
    public Guid LeagueID { get; set; }
    public int Year { get; set; }
    public Guid UserID { get; set; }
    public int DraftPosition { get; set; }
    public int FreeGamesDropped { get; set; }
    public int WillNotReleaseGamesDropped { get; set; }
    public int WillReleaseGamesDropped { get; set; }
    public int SuperDropsAvailable { get; set; }
    public uint Budget { get; set; }
    public string AutoDraftMode { get; set; } = null!;
    public bool OnlyAutoDraftFromWatchlist { get; set; }

    public Publisher ToDomain(FantasyCriticUser user, IEnumerable<PublisherGame> publisherGames, IEnumerable<FormerPublisherGame> formerPublisherGames)
    {
        var autoDraftSettings = new Lib.Enums.AutoDraftSettings(Lib.Enums.AutoDraftMode.FromValue(AutoDraftMode), OnlyAutoDraftFromWatchlist);
        return new Publisher(PublisherID, new LeagueYearKey(LeagueID, Year), user, PublisherName, PublisherIcon, PublisherSlogan, DraftPosition,
            publisherGames, formerPublisherGames, Budget, FreeGamesDropped, WillNotReleaseGamesDropped, WillReleaseGamesDropped, SuperDropsAvailable, autoDraftSettings);
    }
}
