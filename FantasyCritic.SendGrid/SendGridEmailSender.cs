using System;
using System.Threading.Tasks;
using FantasyCritic.Lib.Interfaces;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace FantasyCritic.SendGrid
{
    public class SendGridEmailSender : IEmailSender
    {
        private static readonly string InternalEmail = @"noreply@fantasycritic.games";

        public async Task SendEmailAsync(string email, string subject, string message)
        {
            var apiKey = Environment.GetEnvironmentVariable("SENDGRID_API_KEY_FANTASY_CRITIC", EnvironmentVariableTarget.User);
            var client = new SendGridClient(apiKey);
            var from = new EmailAddress(InternalEmail, "FantasyCritic");
            var to = new EmailAddress(email);
            var msg = MailHelper.CreateSingleEmail(from, to, subject, "", message);
            var response = await client.SendEmailAsync(msg);
        }
    }
}
