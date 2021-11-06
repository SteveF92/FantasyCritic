using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FantasyCritic.Lib.Interfaces;
using FluentEmail.Core;
using FluentEmail.Mailgun;
using Microsoft.AspNetCore.Identity.UI.Services;
using MoreLinq.Experimental;

namespace FantasyCritic.Mailgun
{
    public class MailGunEmailSender : IEmailSender
    {
        private readonly string _domain;
        private readonly string _apiKey;
        private readonly string _fromEmail;

        public MailGunEmailSender(string domain, string apiKey, string fromEmail)
        {
            _domain = domain;
            _apiKey = apiKey;
            _fromEmail = fromEmail;
        }

        public async Task SendEmailAsync(string email, string subject, string message)
        {
            var emailObject = Email
                .From(_fromEmail)
                .To(email)
                .Subject(subject)
                .Body(message, true);

            emailObject.Sender = new MailgunSender(_domain, _apiKey);

            var response = await emailObject.SendAsync();
        }
    }
}
