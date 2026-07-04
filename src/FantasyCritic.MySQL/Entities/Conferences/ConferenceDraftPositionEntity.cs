namespace FantasyCritic.MySQL.Entities.Conferences;

// Class (not record) so Dapper can map via property setters with TINYINT→int coercion.
internal class ConferenceDraftPositionEntity
{
    public Guid DraftID { get; set; }
    public Guid PublisherID { get; set; }
    public int DraftPosition { get; set; }
}
