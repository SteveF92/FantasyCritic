using FantasyCritic.Lib.DependencyInjection;
using FantasyCritic.Lib.Domain;
using FantasyCritic.Lib.Email.EmailModels;
using FantasyCritic.Lib.Enums;
using FantasyCritic.Lib.Extensions;
using FantasyCritic.Lib.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using NLog;
using NodaTime;
using RazorLight;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FantasyCritic.Lib.Services
{
    public class EmailSendingService
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        private readonly FantasyCriticUserManager _userManager;
        private readonly IEmailSender _emailSender;
        private readonly InterLeagueService _interLeagueService;
        private readonly FantasyCriticService _fantasyCriticService;
        private readonly GameAcquisitionService _gameAcquisitionService;
        private readonly LeagueMemberService _leagueMemberService;
        private readonly string _baseAddress;
        private readonly IClock _clock;

        public EmailSendingService(FantasyCriticUserManager userManager, IEmailSender emailSender, 
            InterLeagueService interLeagueService, FantasyCriticService fantasyCriticService,
            GameAcquisitionService gameAcquisitionService, LeagueMemberService leagueMemberService, 
            EmailSendingServiceConfiguration configuration, IClock clock)
        {
            _userManager = userManager;
            _emailSender = emailSender;
            _interLeagueService = interLeagueService;
            _fantasyCriticService = fantasyCriticService;
            _gameAcquisitionService = gameAcquisitionService;
            _leagueMemberService = leagueMemberService;
            _baseAddress = configuration.BaseAddress;
            _clock = clock;
        }

        public async Task SendScheduledEmails()
        {
            var now = _clock.GetCurrentInstant();
            var nycNow = now.InZone(TimeExtensions.EasternTimeZone);

            var dayOfWeek = nycNow.DayOfWeek;
            var timeOfDay = nycNow.TimeOfDay;
            var earliestTimeToSet = TimeExtensions.PublicBiddingRevealTime.Minus(Period.FromMinutes(1));
            var latestTimeToSet = TimeExtensions.PublicBiddingRevealTime.Plus(Period.FromMinutes(1));
            bool isTimeToSendPublicBidEmails = dayOfWeek == TimeExtensions.PublicBiddingRevealDay && timeOfDay > earliestTimeToSet && timeOfDay < latestTimeToSet;
            if (isTimeToSendPublicBidEmails)
            {
                _logger.Info($"Sending public bid emails because date/time is: {nycNow}");
                await SendPublicBidEmails();
            }
        }

        public async Task SendPublicBidEmails()
        {
            var supportedYears = await _interLeagueService.GetSupportedYears();
            var activeYears = supportedYears.Where(x => x.OpenForPlay && !x.Finished);

            var allPublishers = new List<Publisher>();
            var publicBiddingSetDictionary = new Dictionary<LeagueYearKey, PublicBiddingSet>();
            foreach (var year in activeYears)
            {
                var publicBiddingSets = await _gameAcquisitionService.GetPublicBiddingGames(year.Year);
                foreach (var publicBiddingSet in publicBiddingSets)
                {
                    publicBiddingSetDictionary[publicBiddingSet.LeagueYear.Key] = publicBiddingSet;
                }
            }

            var userEmailSettings = await _userManager.GetAllEmailSettings();
            var usersWithPublicBidEmails = userEmailSettings.Where(x => x.EmailTypes.Contains(EmailType.PublicBids)).Select(x => x.User).ToList();
            var usersWithLeagueYears = await _leagueMemberService.GetUsersWithLeagueYearsWithPublisher();

            foreach (var user in usersWithPublicBidEmails)
            {
                bool hasLeagueYears = usersWithLeagueYears.TryGetValue(user, out var leagueYearKeys);
                if (!hasLeagueYears)
                {
                    continue;
                }

                List<PublicBiddingSet> publicBiddingSetsForUser = new List<PublicBiddingSet>();
                foreach (var leagueYearKey in leagueYearKeys)
                {
                    bool hasPublicBiddingSet = publicBiddingSetDictionary.TryGetValue(leagueYearKey, out var publicBiddingSet);
                    if (hasPublicBiddingSet)
                    {
                        publicBiddingSetsForUser.Add(publicBiddingSet);
                    }
                }

                if (publicBiddingSetsForUser.Any())
                {
                    await SendPublicBiddingEmailToUser(user, publicBiddingSetsForUser);
                }
            }
        }

        private async Task SendPublicBiddingEmailToUser(FantasyCriticUser user, IReadOnlyList<PublicBiddingSet> publicBiddingSet)
        {
            string emailAddress = user.Email;
            string emailSubject = "FantasyCritic - This Week's Public Bids";
            PublicBidEmailModel model = new PublicBidEmailModel(user, publicBiddingSet, _baseAddress);

            var htmlResult = await GetHTMLString("PublicBids.cshtml", model);

            await _emailSender.SendEmailAsync(emailAddress, emailSubject, htmlResult);
        }

        public async Task SendConfirmationEmail(FantasyCriticUser user, string link)
        {
            string emailAddress = user.Email;
            string emailSubject = "FantasyCritic - Confirm your email address.";
            ConfirmEmailModel model = new ConfirmEmailModel(user, link);

            var htmlResult = await GetHTMLString("ConfirmEmail.cshtml", model);

            await _emailSender.SendEmailAsync(emailAddress, emailSubject, htmlResult);
        }

        public async Task SendForgotPasswordEmail(FantasyCriticUser user, string link)
        {
            string emailAddress = user.Email;
            string emailSubject = "FantasyCritic - Reset Your Password.";

            PasswordResetModel model = new PasswordResetModel(user, link);
            var htmlResult = await GetHTMLString("PasswordReset.cshtml", model);

            await _emailSender.SendEmailAsync(emailAddress, emailSubject, htmlResult);
        }

        public async Task SendChangeEmail(FantasyCriticUser user, string link)
        {
            string emailAddress = user.Email;
            string emailSubject = "FantasyCritic - Change Your Email.";

            ChangeEmailModel model = new ChangeEmailModel(user, link);
            var htmlResult = await GetHTMLString("ChangeEmail.cshtml", model);

            await _emailSender.SendEmailAsync(emailAddress, emailSubject, htmlResult);
        }

        public async Task SendSiteInviteEmail(string inviteEmail, League league, string baseURL)
        {
            string emailAddress = inviteEmail;
            string emailSubject = "You have been invited to join a FantasyCritic league!";

            LeagueInviteModel model = new LeagueInviteModel(league, baseURL);
            var htmlResult = await GetHTMLString("SiteInvite.cshtml", model);

            await _emailSender.SendEmailAsync(emailAddress, emailSubject, htmlResult);
        }

        public async Task SendLeagueInviteEmail(string inviteEmail, League league, string baseURL)
        {
            string emailAddress = inviteEmail;
            string emailSubject = "You have been invited to join a FantasyCritic league!";

            LeagueInviteModel model = new LeagueInviteModel(league, baseURL);
            var htmlResult = await GetHTMLString("LeagueInvite.cshtml", model);

            await _emailSender.SendEmailAsync(emailAddress, emailSubject, htmlResult);
        }

        private static async Task<string> GetHTMLString(string template, object model)
        {
            var templateFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Email", "EmailTemplates");
            var engine = new RazorLightEngineBuilder()
                .UseFilesystemProject(templateFilePath)
                .UseMemoryCachingProvider()
                .Build();
            string htmlResult = await engine.CompileRenderAsync(template, model);
            return htmlResult;
        }
    }
}
