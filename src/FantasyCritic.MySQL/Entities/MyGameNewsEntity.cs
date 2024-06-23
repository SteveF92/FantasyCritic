namespace FantasyCritic.MySQL.Entities;
internal class MyGameNewsEntity
{
    public Guid MasterGameID { get; set; }
    public bool CounterPick { get; set; }
    public Guid LeagueID { get; set; }
    public string LeagueName { get; set; } = null!;
    public int Year { get; set; }
    public Guid PublisherID { get; set; }
    public string PublisherName { get; set; } = null!;
}
