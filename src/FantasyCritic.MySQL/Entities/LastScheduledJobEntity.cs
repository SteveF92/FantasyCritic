namespace FantasyCritic.MySQL.Entities;

internal class LastScheduledJobEntity
{
    public string JobType { get; set; } = null!;
    public Instant LastScheduledFor { get; set; }
}
