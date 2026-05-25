namespace FantasyCritic.MySQL.Entities;

internal class LeagueDraftPublisherEntity
{
    public Guid DraftID { get; set; }
    public Guid PublisherID { get; set; }
    public int DraftPosition { get; set; }
}
