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
            string emailBody = $"Please use this link to reset your account password:\n {link}";

            await emailSender.SendEmailAsync(emailAddress, emailSubject, emailBody);
        }

        public static async Task SendChangeEmail(this IEmailSender emailSender, FantasyCriticUser user, string newEmailAddress, string changeCode, string baseURL)
        {
            string emailAddress = user.EmailAddress;
            string emailSubject = "FantasyCritic - Change Your Email.";
            string link = $"{baseURL}/changeEmail?NewEmailAddress={UrlEncoder.Default.Encode(newEmailAddress)}&Code={UrlEncoder.Default.Encode(changeCode)}";
            string emailBody = $"Please use this link to change your account email:\n {link}";

            await emailSender.SendEmailAsync(emailAddress, emailSubject, emailBody);
        }

        public static async Task SendInviteEmail(this IEmailSender emailSender, string inviteEmail, string leagueName, FantasyCriticUser leagueManager, string baseURL)
        {
            string emailAddress = inviteEmail;
            string emailSubject = "You have been invited to join a FantasyCritic league!";
            string emailBody = $"You have been invited to join the league '{leagueName}' by user: '{leagueManager.UserName}' (Email Address:'{leagueManager.EmailAddress}') \n" +
                               "Follow this link to go to the site to sign up and join the league: \n" + baseURL;

            await emailSender.SendEmailAsync(emailAddress, emailSubject, emailBody);
        }
    }
}
