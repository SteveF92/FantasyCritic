namespace FantasyCritic.Lib.Domain;

public record SiteAnnouncement(Guid Id, string Title, string Body, Instant PostedAt, string? LinkAddress, string? LinkLabel);
