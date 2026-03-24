using System.Security.Cryptography;

namespace FantasyCritic.Lib.Identity;

public class SupportTicket
{
    public SupportTicket(Guid supportTicketID, FantasyCriticUser user, string verificationCode, Instant openedAt, string issueDescription,
        bool openedByUser, Instant? closedAt, string? resolutionNotes)
    {
        SupportTicketID = supportTicketID;
        User = user;
        VerificationCode = verificationCode;
        OpenedAt = openedAt;
        IssueDescription = issueDescription;
        OpenedByUser = openedByUser;
        ClosedAt = closedAt;
        ResolutionNotes = resolutionNotes;
    }

    public Guid SupportTicketID { get; }
    public Guid UserID => User.Id;
    public FantasyCriticUser User { get; }
    public string VerificationCode { get; }
    public Instant OpenedAt { get; }
    public string IssueDescription { get; }
    public bool OpenedByUser { get; }
    public Instant? ClosedAt { get; }
    public string? ResolutionNotes { get; }

    public bool Active => !ClosedAt.HasValue;

    public static SupportTicket Create(FantasyCriticUser user, string issueDescription, Instant openedAt, bool openedByUser)
    {
        return new SupportTicket(Guid.NewGuid(), user, GenerateVerificationCode(), openedAt, issueDescription, openedByUser, null, null);
    }

    public SupportTicket UpdateIssueDescription(string issueDescription)
    {
        return new SupportTicket(SupportTicketID, User, VerificationCode, OpenedAt, issueDescription, OpenedByUser, ClosedAt, ResolutionNotes);
    }

    public SupportTicket Close(Instant closedAt, string? resolutionNotes)
    {
        return new SupportTicket(SupportTicketID, User, VerificationCode, OpenedAt, IssueDescription, OpenedByUser, closedAt, resolutionNotes);
    }

    private static string GenerateVerificationCode()
    {
        const int length = 8;
        const string alphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";

        var bytes = new byte[length];
        RandomNumberGenerator.Fill(bytes);

        var chars = new char[length];
        for (int i = 0; i < length; i++)
        {
            chars[i] = alphabet[bytes[i] % alphabet.Length];
        }

        return new string(chars);
    }

    /// <summary>
    /// Formats a stored 8-character code for display as XXXX-XXXX. Hyphen is not stored in the database.
    /// </summary>
    public static string FormatVerificationCodeForDisplay(string? verificationCode)
    {
        if (string.IsNullOrWhiteSpace(verificationCode))
        {
            return string.Empty;
        }

        string trimmed = verificationCode.Trim();
        if (trimmed.Length == 8)
        {
            return $"{trimmed[..4]}-{trimmed[4..]}";
        }

        return trimmed;
    }
}
