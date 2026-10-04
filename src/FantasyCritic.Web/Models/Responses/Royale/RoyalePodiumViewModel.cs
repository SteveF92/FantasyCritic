using FantasyCritic.Lib.Royale;

namespace FantasyCritic.Web.Models.Responses.Royale;

public class RoyalePodiumViewModel
{
    public RoyalePodiumViewModel(RoyaleYearQuarter podiumYearQuarter, RoyaleYearQuarter viewedYearQuarter, IEnumerable<RoyalePodiumEntry> entries)
    {
        Year = podiumYearQuarter.YearQuarter.Year;
        Quarter = podiumYearQuarter.YearQuarter.Quarter;
        IsPreviousQuarter = !podiumYearQuarter.Equals(viewedYearQuarter);
        Entries = entries.Select(x => new RoyalePodiumEntryViewModel(x)).ToList();
    }

    public int Year { get; }
    public int Quarter { get; }
    public bool IsPreviousQuarter { get; }
    public IReadOnlyList<RoyalePodiumEntryViewModel> Entries { get; }
}

public class RoyalePodiumEntryViewModel
{
    public RoyalePodiumEntryViewModel(RoyalePodiumEntry domain)
    {
        PublisherID = domain.PublisherID;
        UserID = domain.User.UserID;
        PlayerName = domain.User.DisplayName;
        PublisherName = domain.PublisherName;
        PublisherIcon = domain.PublisherIcon;
        TotalFantasyPoints = domain.TotalFantasyPoints;
        Ranking = domain.Ranking;
    }

    public Guid PublisherID { get; }
    public Guid UserID { get; }
    public string PlayerName { get; }
    public string PublisherName { get; }
    public string? PublisherIcon { get; }
    public decimal TotalFantasyPoints { get; }
    public int Ranking { get; }
}
