using FantasyCritic.Lib.DependencyInjection;
using FantasyCritic.Lib.Email.EmailModels;
using FantasyCritic.Lib.Identity;
using Serilog;
using RazorLight;
using System.IO;
using FantasyCritic.Lib.Interfaces;
using FantasyCritic.Lib.Domain.Combinations;

namespace FantasyCritic.Lib.Services;

public class EmailSendingService
{
    private static readonly ILogger _logger = Log.ForContext<EmailSendingService>();

    private readonly FantasyCriticUserManager _userManager;
    private readonly IEmailSender _emailSender;
    private readonly LeagueMemberService _leagueMemberService;
    private readonly string _baseAddress;
    private readonly bool _isProduction;

    public EmailSendingService(FantasyCriticUserManager userManager, IEmailSender emailSender,
        LeagueMemberService leagueMemberService, EnvironmentConfiguration configuration)
    {
        _userManager = userManager;
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
        PublicBidEmailModel model = new PublicBidEmailModel(user, publicBiddingSet, _baseAddress, _isProduction);

        var htmlResult = await GetHTMLString("PublicBids.cshtml", model);

        await _emailSender.SendEmailAsync(emailAddress, emailSubject, htmlResult);
    }

    public async Task SendConfirmationEmail(FantasyCriticUser user, string link)
    {
        string emailAddress = user.Email;
        const string emailSubject = "FantasyCritic - Confirm your email address.";
        ConfirmEmailModel model = new ConfirmEmailModel(user, link);

        var htmlResult = await GetHTMLString("ConfirmEmail.cshtml", model);

        await _emailSender.SendEmailAsync(emailAddress, emailSubject, htmlResult);
    }

    public async Task SendForgotPasswordEmail(FantasyCriticUser user, string link)
    {
        string emailAddress = user.Email;
        const string emailSubject = "FantasyCritic - Reset Your Password.";

        PasswordResetModel model = new PasswordResetModel(user, link);
        var htmlResult = await GetHTMLString("PasswordReset.cshtml", model);

        await _emailSender.SendEmailAsync(emailAddress, emailSubject, htmlResult);
    }

    public async Task SendChangeEmail(FantasyCriticUser user, string link)
    {
        string emailAddress = user.Email;
        const string emailSubject = "FantasyCritic - Change Your Email.";

        ChangeEmailModel model = new ChangeEmailModel(user, link);
        var htmlResult = await GetHTMLString("ChangeEmail.cshtml", model);

        await _emailSender.SendEmailAsync(emailAddress, emailSubject, htmlResult);
    }

    public async Task SendSiteInviteEmail(string inviteEmail, League league, string baseURL)
    {
        const string emailSubject = "You have been invited to join a FantasyCritic league!";

        LeagueInviteModel model = new LeagueInviteModel(league, baseURL);
        var htmlResult = await GetHTMLString("SiteInvite.cshtml", model);

        await _emailSender.SendEmailAsync(inviteEmail, emailSubject, htmlResult);
    }

    public async Task SendLeagueInviteEmail(string inviteEmail, League league, string baseURL)
    {
        const string emailSubject = "You have been invited to join a FantasyCritic league!";

        LeagueInviteModel model = new LeagueInviteModel(league, baseURL);
        var htmlResult = await GetHTMLString("LeagueInvite.cshtml", model);

        await _emailSender.SendEmailAsync(inviteEmail, emailSubject, htmlResult);
    }

    private static async Task<string> GetHTMLString(string template, object model)
    {
        var templateFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Email", "EmailTemplates");
        var engine = new RazorLightEngineBuilder()
            .UseFileSystemProject(templateFilePath)
            .UseMemoryCachingProvider()
            .Build();
        string htmlResult = await engine.CompileRenderAsync(template, model);
        return htmlResult;
    }
}
