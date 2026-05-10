namespace FantasyCritic.Web.Models.Responses;

public class SiteAnnouncementViewModel
{
    public SiteAnnouncementViewModel(SiteAnnouncement announcement)
    {
        Id = announcement.Id.ToString();
        HtmlId = announcement.HtmlId;
        Title = announcement.Title;
        Body = announcement.Body;
        PostedAt = announcement.PostedAt;
        LinkAddress = announcement.LinkAddress;
        LinkLabel = announcement.LinkLabel;
    }

    public string Id { get; }
    public string HtmlId { get; }
    public string Title { get; }
    public string Body { get; }
    public Instant PostedAt { get; }
    public string? LinkAddress { get; }
    public string? LinkLabel { get; }
}
