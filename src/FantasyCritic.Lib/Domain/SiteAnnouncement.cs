namespace FantasyCritic.Lib.Domain;

public record SiteAnnouncement(Guid Id, string HtmlId, string Title, string Body, Instant PostedAt, string? LinkAddress, string? LinkLabel);
