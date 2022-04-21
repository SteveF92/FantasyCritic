namespace FantasyCritic.Lib.Interfaces;
public interface IEmailSender
{
    Task SendEmailAsync(string email, string subject, string htmlMessage);
}
