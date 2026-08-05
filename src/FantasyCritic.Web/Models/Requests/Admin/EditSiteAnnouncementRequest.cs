namespace FantasyCritic.Web.Models.Requests.Admin;

public record EditSiteAnnouncementRequest(Guid AnnouncementID, string Title, string Body, DateTimeOffset PostedAt, string? LinkAddress, string? LinkLabel)
{
    public SiteAnnouncement ToDomain()
    {
        return new SiteAnnouncement(AnnouncementID, Title.Trim(), Body.Trim(), Instant.FromDateTimeOffset(PostedAt),
            SiteAnnouncementRequestExtensions.TrimToNull(LinkAddress), SiteAnnouncementRequestExtensions.TrimToNull(LinkLabel));
    }
}

internal static class SiteAnnouncementRequestExtensions
{
    /// <summary>An empty link field posted from the admin form should be stored as NULL, not as "".</summary>
    public static string? TrimToNull(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
