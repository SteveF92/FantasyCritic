namespace FantasyCritic.MySQL.Entities.Conferences;

// Class (not record) so Dapper can map via property setters with TINYINT→int coercion.
internal class ConferenceDraftInfoEntity
{
    public Guid DraftID { get; set; }
    public Guid LeagueID { get; set; }
    public int Year { get; set; }
    public int DraftNumber { get; set; }
}
