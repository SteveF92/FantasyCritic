using FantasyCritic.Lib.Identity;

namespace FantasyCritic.Lib.Royale;

public class RoyalePublisher
{
    public RoyalePublisher(Guid publisherID, RoyaleYearQuarter yearQuarter, VeryMinimalFantasyCriticUser user,
        string publisherName, string? publisherIcon, string? publisherSlogan, IEnumerable<RoyalePublisherGame> publisherGames, decimal budget)
    {
        PublisherID = publisherID;
        YearQuarter = yearQuarter;
        User = user;
        PublisherName = publisherName;
        PublisherIcon = publisherIcon;
        PublisherSlogan = publisherSlogan;
        PublisherGames = publisherGames.ToList();
        Budget = budget;
    }

    public Guid PublisherID { get; }
    public RoyaleYearQuarter YearQuarter { get; }
    public VeryMinimalFantasyCriticUser User { get; }
    public string PublisherName { get; }
    public string? PublisherIcon { get; }
    public string? PublisherSlogan { get; }
    public IReadOnlyList<RoyalePublisherGame> PublisherGames { get; }
    public decimal Budget { get; }

    public decimal GetTotalFantasyPoints()
    {
        decimal? points = PublisherGames
            .Sum(x => x.FantasyPoints);
        return points ?? 0m;
    }
}
