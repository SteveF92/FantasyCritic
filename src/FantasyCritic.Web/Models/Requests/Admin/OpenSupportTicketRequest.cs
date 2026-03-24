namespace FantasyCritic.Web.Models.Requests.Admin;

public record OpenSupportTicketRequest(Guid UserID, string IssueDescription);
