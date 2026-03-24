using FantasyCritic.Lib.Identity;
using FantasyCritic.Lib.SharedSerialization.API;

namespace FantasyCritic.Web.Models.Responses;

public class SupportTicketAdminListEntryViewModel
{
    public SupportTicketAdminListEntryViewModel(SupportTicket ticket)
    {
        SupportTicketID = ticket.SupportTicketID;
        UserID = ticket.UserID;
        User = new FantasyCriticUserViewModel(ticket.User);
        UserDisplayName = ticket.User.UserName;
        VerificationCode = SupportTicket.FormatVerificationCodeForDisplay(ticket.VerificationCode);
        IssueDescription = ticket.IssueDescription;
        OpenedAt = ticket.OpenedAt;
    }

    public Guid SupportTicketID { get; }
    public Guid UserID { get; }
    public FantasyCriticUserViewModel User { get; }
    public string UserDisplayName { get; }
    public string VerificationCode { get; }
    public string IssueDescription { get; }
    public Instant OpenedAt { get; }
}
