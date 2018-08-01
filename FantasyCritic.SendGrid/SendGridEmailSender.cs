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
            var apiKey = Environment.GetEnvironmentVariable("SENDGRID_API_KEY", EnvironmentVariableTarget.User);
            var client = new SendGridClient(apiKey);
            var from = new EmailAddress("test@example.com", "Example User");
            var subjectWOOP = "Sending with SendGrid is Fun";
            var to = new EmailAddress("test@example.com", "Example User");
            var plainTextContent = "and easy to do anywhere, even with C#";
            var htmlContent = "<strong>and easy to do anywhere, even with C#</strong>";
            var msg = MailHelper.CreateSingleEmail(from, to, subjectWOOP, plainTextContent, htmlContent);
            var response = await client.SendEmailAsync(msg);




            //var apiKey = Environment.GetEnvironmentVariable("SENDGRID_API_KEY", EnvironmentVariableTarget.User);
            //var client = new SendGridClient(apiKey);
            //var from = new EmailAddress(InternalEmail, "FantasyCritic");
            //var to = new EmailAddress(email);
            //var plainTextContent = message;
            //var htmlContent = "";
            //var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
            //await client.SendEmailAsync(msg);
        }
    }
}
