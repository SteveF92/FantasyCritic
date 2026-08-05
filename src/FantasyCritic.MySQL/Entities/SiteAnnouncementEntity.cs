namespace FantasyCritic.MySQL.Entities;

internal class SiteAnnouncementEntity
{
    public SiteAnnouncementEntity()
    {

    }

    public SiteAnnouncementEntity(SiteAnnouncement domain)
    {
        ID = domain.Id;
        Title = domain.Title;
        Body = domain.Body;
        PostedAt = domain.PostedAt;
        LinkAddress = domain.LinkAddress;
        LinkLabel = domain.LinkLabel;
    }

    public Guid ID { get; set; }
    public string Title { get; set; } = null!;
    public string Body { get; set; } = null!;
    public Instant PostedAt { get; set; }
    public string? LinkAddress { get; set; }
    public string? LinkLabel { get; set; }

    public SiteAnnouncement ToDomain()
    {
        return new SiteAnnouncement(ID, Title, Body, PostedAt, LinkAddress, LinkLabel);
    }
}
