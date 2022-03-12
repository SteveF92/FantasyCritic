namespace FantasyCritic.Web.Models.Responses;

public class SiteCountsViewModel
{
    public SiteCountsViewModel(SiteCounts siteCounts)
    {
        UserCount = siteCounts.UserCount;
        LeagueCount = siteCounts.LeagueCount;
        MasterGameCount = siteCounts.MasterGameCount;
        PublisherGameCount = siteCounts.PublisherGameCount;
    }

    public int UserCount { get; }
    public int LeagueCount { get; }
    public int MasterGameCount { get; }
    public int PublisherGameCount { get; }
}