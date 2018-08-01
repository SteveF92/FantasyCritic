using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FantasyCritic.Lib.Domain;
using FantasyCritic.Lib.Interfaces;

namespace FantasyCritic.Web.Extensions
{
    public static class EmailSenderExtensions
    {
        public static async Task SendConfirmationEmail(this IEmailSender emailSender, FantasyCriticUser user, string confirmCode)
        {
            string emailAddress = user.EmailAddress;
            string emailSubject = "FantasyCritic - Confirm your email address.";
            string emailBody = $"Please use this code to confirm your FantasyCritic account: {confirmCode}";

            await emailSender.SendEmailAsync(emailAddress, emailSubject, emailBody);
        }
    }
}
