using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Policy;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using FantasyCritic.Lib.Domain;
using FantasyCritic.Lib.Identity;
using FantasyCritic.Lib.Interfaces;
using FantasyCritic.Web.Models.EmailTemplates;
using Microsoft.AspNetCore.Identity.UI.Services;
using RazorLight;

namespace FantasyCritic.Web.Extensions
{
    public static class EmailSenderExtensions
    {
        private static async Task<string> GetHTMLString(string template, object model)
        {
            var templateFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "EmailTemplates");
            var engine = new RazorLightEngineBuilder()
                .UseFilesystemProject(templateFilePath)
                .UseMemoryCachingProvider()
                .Build();
            string htmlResult = await engine.CompileRenderAsync(template, model);
            return htmlResult;
        }

        public static async Task SendConfirmationEmail(this IEmailSender emailSender, FantasyCriticUser user, string link)
        {
            string emailAddress = user.Email;
            string emailSubject = "FantasyCritic - Confirm your email address.";
            ConfirmEmailModel model = new ConfirmEmailModel(user, link);

            var htmlResult = await GetHTMLString("ConfirmEmail.cshtml", model);

            await emailSender.SendEmailAsync(emailAddress, emailSubject, htmlResult);
        }

        public static async Task SendForgotPasswordEmail(this IEmailSender emailSender, FantasyCriticUser user, string resetCode, string baseURL)
        {
            string emailAddress = user.Email;
            string emailSubject = "FantasyCritic - Reset Your Password.";

            PasswordResetModel model = new PasswordResetModel(user, resetCode, baseURL);
            var htmlResult = await GetHTMLString("PasswordReset.cshtml", model);

            await emailSender.SendEmailAsync(emailAddress, emailSubject, htmlResult);
        }

        public static async Task SendChangeEmail(this IEmailSender emailSender, FantasyCriticUser user, string newEmailAddress, string changeCode, string baseURL)
        {
            string emailAddress = user.Email;
            string emailSubject = "FantasyCritic - Change Your Email.";

            ChangeEmailModel model = new ChangeEmailModel(user, newEmailAddress, changeCode, baseURL);
            var htmlResult = await GetHTMLString("ChangeEmail.cshtml", model);

            await emailSender.SendEmailAsync(emailAddress, emailSubject, htmlResult);
        }

        public static async Task SendSiteInviteEmail(this IEmailSender emailSender, string inviteEmail, League league, string baseURL)
        {
            string emailAddress = inviteEmail;
            string emailSubject = "You have been invited to join a FantasyCritic league!";

            LeagueInviteModel model = new LeagueInviteModel(league, baseURL);
            var htmlResult = await GetHTMLString("SiteInvite.cshtml", model);

            await emailSender.SendEmailAsync(emailAddress, emailSubject, htmlResult);
        }

        public static async Task SendLeagueInviteEmail(this IEmailSender emailSender, string inviteEmail, League league, string baseURL)
        {
            string emailAddress = inviteEmail;
            string emailSubject = "You have been invited to join a FantasyCritic league!";

            LeagueInviteModel model = new LeagueInviteModel(league, baseURL);
            var htmlResult = await GetHTMLString("LeagueInvite.cshtml", model);

            await emailSender.SendEmailAsync(emailAddress, emailSubject, htmlResult);
        }
    }
}
