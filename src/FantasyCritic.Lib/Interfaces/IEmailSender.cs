namespace FantasyCritic.Lib.Interfaces;

public interface IEmailSender
{
    Task SendEmailAsync(string email, string subject, string htmlMessage);

    //For an email whose failure must not go unnoticed. SendEmailAsync logs a failure and carries on, which suits a loop over many recipients.
    Task SendEmailOrThrow(string email, string subject, string htmlMessage);
}
