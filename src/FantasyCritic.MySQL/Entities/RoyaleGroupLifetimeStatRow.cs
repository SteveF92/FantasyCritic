namespace FantasyCritic.MySQL.Entities;

internal sealed class RoyaleGroupLifetimeStatRow
{
    public Guid UserID { get; set; }
    public int QuartersParticipated { get; set; }
    public decimal TotalPoints { get; set; }
    public double? AverageRankWithinGroup { get; set; }
}
