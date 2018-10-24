using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using FantasyCritic.Lib.Domain;
using FantasyCritic.Lib.Interfaces;

namespace FantasyCritic.Web.Extensions
{
    public static class EmailSenderExtensions
    {
        public static async Task SendConfirmationEmail(this IEmailSender emailSender, FantasyCriticUser user, string confirmCode, string baseURL)
        {
            string emailAddress = user.EmailAddress;
            string emailSubject = "FantasyCritic - Confirm your email address.";
            string link = $"{baseURL}/confirmEmail?UserID={user.UserID}&Code={UrlEncoder.Default.Encode(confirmCode)}";
            string emailBody = $"Please use this link to confirm your FantasyCritic account:\n {link}";

            await emailSender.SendEmailAsync(emailAddress, emailSubject, emailBody);
        }

        public static async Task SendForgotPasswordEmail(this IEmailSender emailSender, FantasyCriticUser user, string resetCode, string baseURL)
        {
            string emailAddress = user.EmailAddress;
            string emailSubject = "FantasyCritic - Reset Your Password.";
            string link = $"{baseURL}/resetPassword?Code={UrlEncoder.Default.Encode(resetCode)}";
            string emailBody = $"Please use this code to reset your account password:\n {link}";

            await emailSender.SendEmailAsync(emailAddress, emailSubject, emailBody);
        }
    }
}
