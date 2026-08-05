namespace FantasyCritic.Web.Models.Requests.Admin;

public record CreateSiteAnnouncementRequest(string Title, string Body, DateTimeOffset? PostedAt, string? LinkAddress, string? LinkLabel)
{
    public SiteAnnouncement ToDomain(IClock clock)
    {
        var postedAt = PostedAt.HasValue ? Instant.FromDateTimeOffset(PostedAt.Value) : clock.GetCurrentInstant();
        return new SiteAnnouncement(Guid.NewGuid(), Title.Trim(), Body.Trim(), postedAt,
            SiteAnnouncementRequestExtensions.TrimToNull(LinkAddress), SiteAnnouncementRequestExtensions.TrimToNull(LinkLabel));
    }
}
