using FantasyCritic.Lib.Royale;

namespace FantasyCritic.Web.Models.Responses.Royale;

public class RoyalePublisherHistoryViewModel
{
    public RoyalePublisherHistoryViewModel(RoyalePublisherHistoryEntry domain)
    {
        PublisherID = domain.PublisherID;
        Year = domain.Year;
        Quarter = domain.Quarter;
        PublisherName = domain.PublisherName;
        PublisherIcon = domain.PublisherIcon;
        TotalFantasyPoints = domain.TotalFantasyPoints;
        Ranking = domain.Ranking;
    }

    public Guid PublisherID { get; }
    public int Year { get; }
    public int Quarter { get; }
    public string PublisherName { get; }
    public string? PublisherIcon { get; }
    public decimal TotalFantasyPoints { get; }
    public int? Ranking { get; }
}
