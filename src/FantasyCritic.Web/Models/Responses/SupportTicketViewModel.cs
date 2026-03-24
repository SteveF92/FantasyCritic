using FantasyCritic.Lib.Identity;
using FantasyCritic.Lib.SharedSerialization.API;

namespace FantasyCritic.Web.Models.Responses;

public class SupportTicketViewModel
{
    public SupportTicketViewModel(SupportTicket ticket)
    {
        SupportTicketID = ticket.SupportTicketID;
        User = new FantasyCriticUserViewModel(ticket.User);
        VerificationCode = SupportTicket.FormatVerificationCodeForDisplay(ticket.VerificationCode);
        OpenedAt = ticket.OpenedAt;
        IssueDescription = ticket.IssueDescription;
        ClosedAt = ticket.ClosedAt;
        ResolutionNotes = ticket.ResolutionNotes;
    }

    public Guid SupportTicketID { get; }
    public FantasyCriticUserViewModel User { get; }
    public string VerificationCode { get; }
    public Instant OpenedAt { get; }
    public string IssueDescription { get; }
    public Instant? ClosedAt { get; }
    public string? ResolutionNotes { get; }
}
