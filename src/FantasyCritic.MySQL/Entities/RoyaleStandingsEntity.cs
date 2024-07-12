namespace FantasyCritic.MySQL.Entities;

public class RoyaleStandingsEntity
{
    public Guid PublisherID { get; set; }
    public Guid UserID { get; set; }
    public string DisplayName { get; set; } = null!;
    public decimal TotalFantasyPoints { get; set; }
}
