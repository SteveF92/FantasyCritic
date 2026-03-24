namespace FantasyCritic.Web.Models.Requests.Admin;

public record CloseSupportTicketRequest(Guid SupportTicketID, string? ResolutionNotes);
