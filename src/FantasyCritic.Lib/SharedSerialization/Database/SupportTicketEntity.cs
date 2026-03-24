using FantasyCritic.Lib.Identity;

namespace FantasyCritic.Lib.SharedSerialization.Database;

public class SupportTicketEntity
{
    public SupportTicketEntity()
    {

    }

    public SupportTicketEntity(SupportTicket supportTicket)
    {
        SupportTicketID = supportTicket.SupportTicketID;
        UserID = supportTicket.UserID;
        VerificationCode = supportTicket.VerificationCode;
        OpenedAt = supportTicket.OpenedAt;
        IssueDescription = supportTicket.IssueDescription;
        OpenedByUser = supportTicket.OpenedByUser;
        ClosedAt = supportTicket.ClosedAt;
        ResolutionNotes = supportTicket.ResolutionNotes;
    }

    public Guid SupportTicketID { get; set; }
    public Guid UserID { get; set; }
    public string VerificationCode { get; set; } = null!;
    public Instant OpenedAt { get; set; }
    public string IssueDescription { get; set; } = null!;
    public bool OpenedByUser { get; set; }
    public Instant? ClosedAt { get; set; }
    public string? ResolutionNotes { get; set; }

    public SupportTicket ToDomain(FantasyCriticUser user)
    {
        if (user.Id != UserID)
        {
            throw new ArgumentException($"User {user.Id} does not match ticket UserID {UserID}.", nameof(user));
        }

        return new SupportTicket(SupportTicketID, user, VerificationCode, OpenedAt, IssueDescription, OpenedByUser, ClosedAt, ResolutionNotes);
    }
}
