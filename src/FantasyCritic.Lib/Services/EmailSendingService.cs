using FantasyCritic.Lib.DependencyInjection;
using FantasyCritic.Lib.Identity;
using FantasyCritic.Lib.Interfaces;
using FantasyCritic.Lib.Domain.Combinations;

namespace FantasyCritic.Lib.Services;

public class EmailSendingService
{
    private readonly FantasyCriticUserManager _userManager;
    private readonly IEmailBuilder _emailBuilder;
    private readonly IEmailSender _emailSender;
    private readonly LeagueMemberService _leagueMemberService;
    private readonly string _baseAddress;
    private readonly bool _isProduction;

    public EmailSendingService(FantasyCriticUserManager userManager, IEmailBuilder emailBuilder, IEmailSender emailSender,
        LeagueMemberService leagueMemberService, EnvironmentConfiguration configuration)
    {
        _userManager = userManager;
        _emailBuilder = emailBuilder;
        _emailSender = emailSender;
        _leagueMemberService = leagueMemberService;
        _baseAddress = configuration.BaseAddress;
        _isProduction = configuration.IsProduction;
    }

    public async Task SendPublicBidEmails(IEnumerable<LeagueYearPublicBiddingSet> publicBiddingSets)
    {
        var publicBiddingSetDictionary = publicBiddingSets.ToDictionary(x => x.LeagueYear.Key);

        var adminUsers = (await _userManager.GetUsersInRoleAsync("Admin")).ToHashSet();
        var userEmailSettings = await _userManager.GetAllEmailSettings();
        var usersWithPublicBidEmails = userEmailSettings.Where(x => !x.User.IsDeleted && x.User.EmailConfirmed && x.EmailTypes.Contains(EmailType.PublicBids)).Select(x => x.User).ToList();
        
        if (!_isProduction)
        {
            usersWithPublicBidEmails = usersWithPublicBidEmails.Intersect(adminUsers).ToList();
        }
        
        var usersWithLeagueYears = await _leagueMemberService.GetUsersWithLeagueYearsWithPublisher();

        foreach (var user in usersWithPublicBidEmails)
        {
            var leagueYearKeys = usersWithLeagueYears.GetValueOrDefault(user);
            if (leagueYearKeys is null)
            {
                continue;
            }

            List<LeagueYearPublicBiddingSet> publicBiddingSetsForUser = new List<LeagueYearPublicBiddingSet>();
            foreach (var leagueYearKey in leagueYearKeys)
            {
                var publicBiddingSet = publicBiddingSetDictionary.GetValueOrDefault(leagueYearKey);
                if (publicBiddingSet is not null)
                {
                    publicBiddingSetsForUser.Add(publicBiddingSet);
                }
            }

            var userIsAdmin = adminUsers.Contains(user);
            if (publicBiddingSetsForUser.Any() && publicBiddingSetsForUser.Any(x => x.MasterGames.Any()) || userIsAdmin)
            {
                await SendPublicBiddingEmailToUser(user, publicBiddingSetsForUser);
            }
        }
    }

    private async Task SendPublicBiddingEmailToUser(FantasyCriticUser user, IReadOnlyList<LeagueYearPublicBiddingSet> publicBiddingSet)
    {
        string emailAddress = user.Email;
        const string emailSubject = "FantasyCritic - This Week's Public Bids";
        var htmlResult = await _emailBuilder.BuildPublicBidEmail(user, publicBiddingSet, _baseAddress, _isProduction);
        await _emailSender.SendEmailAsync(emailAddress, emailSubject, htmlResult);
    }

    public async Task SendConfirmationEmail(FantasyCriticUser user, string link)
    {
        string emailAddress = user.Email;
        const string emailSubject = "FantasyCritic - Confirm your email address.";
        var htmlResult = await _emailBuilder.BuildConfirmEmailEmail(user, link);
        await _emailSender.SendEmailAsync(emailAddress, emailSubject, htmlResult);
    }

    public async Task SendForgotPasswordEmail(FantasyCriticUser user, string link)
    {
        string emailAddress = user.Email;
        const string emailSubject = "FantasyCritic - Reset Your Password.";
        var htmlResult = await _emailBuilder.BuildPasswordResetEmail(user, link);
        await _emailSender.SendEmailAsync(emailAddress, emailSubject, htmlResult);
    }

    public async Task SendChangeEmail(FantasyCriticUser user, string newEmailAddress, string link)
    {
        string emailAddress = user.Email;
        const string oldEmailSubject = "FantasyCritic - Email Change Request Notification.";
        const string newEmailSubject = "FantasyCritic - Change Your Email.";

        var oldEmailHtmlResult = await _emailBuilder.BuildEmailChangeNotificationEmail(user, newEmailAddress);
        var newEmailHtmlResult = await _emailBuilder.BuildChangeEmailEmail(user, link);

        await _emailSender.SendEmailAsync(emailAddress, oldEmailSubject, oldEmailHtmlResult);
        await _emailSender.SendEmailAsync(newEmailAddress, newEmailSubject, newEmailHtmlResult);
    }

    public async Task SendSiteInviteEmail(string inviteEmail, League league, string baseURL)
    {
        const string emailSubject = "You have been invited to join a FantasyCritic league!";
        var htmlResult = await _emailBuilder.BuildLeagueInviteEmail(league, baseURL);
        await _emailSender.SendEmailAsync(inviteEmail, emailSubject, htmlResult);
    }

    public async Task SendLeagueInviteEmail(string inviteEmail, League league, string baseURL)
    {
        const string emailSubject = "You have been invited to join a FantasyCritic league!";
        var htmlResult = await _emailBuilder.BuildSiteInviteEmail(league, baseURL);
        await _emailSender.SendEmailAsync(inviteEmail, emailSubject, htmlResult);
    }
}
