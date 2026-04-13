using FantasyCritic.Lib.Royale;

namespace FantasyCritic.MySQL.Entities;

internal class RoyalePublisherHistoryEntity
{
    public Guid PublisherID { get; set; }
    public int Year { get; set; }
    public int Quarter { get; set; }
    public string PublisherName { get; set; } = null!;
    public string? PublisherIcon { get; set; }
    public decimal TotalFantasyPoints { get; set; }
    public long Ranking { get; set; }

    public RoyalePublisherHistoryEntry ToDomain()
    {
        int? ranking = TotalFantasyPoints > 0 ? (int)Ranking : null;
        return new RoyalePublisherHistoryEntry(PublisherID, Year, Quarter, PublisherName, PublisherIcon, TotalFantasyPoints, ranking);
    }
}
