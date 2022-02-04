using FantasyCritic.Lib.Domain;
using FantasyCritic.Lib.Enums;
using FantasyCritic.Lib.Extensions;
using FantasyCritic.Lib.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using NLog;
using NodaTime;
using System;
using System.Collections.Generic;
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
        private readonly IClock _clock;

        public EmailSendingService(FantasyCriticUserManager userManager, IEmailSender emailSender, 
            InterLeagueService interLeagueService, FantasyCriticService fantasyCriticService,
            GameAcquisitionService gameAcquisitionService, LeagueMemberService leagueMemberService, 
            IClock clock)
        {
            _userManager = userManager;
            _emailSender = emailSender;
            _interLeagueService = interLeagueService;
            _fantasyCriticService = fantasyCriticService;
            _gameAcquisitionService = gameAcquisitionService;
            _leagueMemberService = leagueMemberService;
            _clock = clock;
        }

        public async Task SendEmails()
        {
            var now = _clock.GetCurrentInstant();
            var nycNow = now.InZone(TimeExtensions.EasternTimeZone);
            
            await SendPublicBidEmails(nycNow);
        }

        private async Task SendPublicBidEmails(ZonedDateTime nycNow)
        {
            var dayOfWeek = nycNow.DayOfWeek;
            var timeOfDay = nycNow.TimeOfDay;
            var earliestTimeToSet = TimeExtensions.PublicBiddingRevealTime.Minus(Period.FromMinutes(1));
            var latestTimeToSet = TimeExtensions.PublicBiddingRevealTime.Plus(Period.FromMinutes(1));
            bool isTimeToSendPublicBidEmails = dayOfWeek == TimeExtensions.PublicBiddingRevealDay && timeOfDay > earliestTimeToSet && timeOfDay < latestTimeToSet;
            if (!isTimeToSendPublicBidEmails)
            {
                return;
            }

            _logger.Info($"Sending public bid emails because date/time is: {nycNow}");
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

                await SendPublicBiddingEmailToUser(user, publicBiddingSetsForUser);
            }
        }

        private async Task SendPublicBiddingEmailToUser(FantasyCriticUser user, IEnumerable<PublicBiddingSet> publicBiddingSet)
        {
            return;
        }
    }
}
