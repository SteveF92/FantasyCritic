namespace FantasyCritic.MySQL.Entities;

internal class SiteAnnouncementEntity
{
    public Guid ID { get; set; }
    public string HtmlID { get; set; } = null!;
    public string Title { get; set; } = null!;
    public string Body { get; set; } = null!;
    public Instant PostedAt { get; set; }
    public string? LinkAddress { get; set; }
    public string? LinkLabel { get; set; }

    public SiteAnnouncement ToDomain()
    {
        return new SiteAnnouncement(ID, HtmlID, Title, Body, PostedAt, LinkAddress, LinkLabel);
    }
}
