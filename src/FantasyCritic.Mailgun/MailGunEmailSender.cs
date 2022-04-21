using System.Threading.Tasks;
using FantasyCritic.Lib.Interfaces;
using FluentEmail.Core;
using FluentEmail.Mailgun;

namespace FantasyCritic.Mailgun;

public class MailGunEmailSender : IEmailSender
{
    private readonly string _domain;
    private readonly string _apiKey;
    private readonly string _fromEmail;
    private readonly string _fromName;

    public MailGunEmailSender(string domain, string apiKey, string fromEmail, string fromName)
    {
        _domain = domain;
        _apiKey = apiKey;
        _fromEmail = fromEmail;
        _fromName = fromName;
    }

    public async Task SendEmailAsync(string email, string subject, string message)
    {
        var emailObject = Email
            .From(_fromEmail, _fromName)
            .To(email)
            .Subject(subject)
            .Body(message, true);

        emailObject.Sender = new MailgunSender(_domain, _apiKey);

        var response = await emailObject.SendAsync();
    }
}
