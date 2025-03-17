using FantasyCritic.EmailTemplates.Models;
using FantasyCritic.Lib.Domain;
using FantasyCritic.Lib.Domain.Combinations;
using FantasyCritic.Lib.Identity;
using FantasyCritic.Lib.Interfaces;
using Razor.Templating.Core;

namespace FantasyCritic.EmailTemplates;

public class RazorEmailBuilder : IEmailBuilder
{
    public async Task<string> BuildEmailChangeNotificationEmail(FantasyCriticUser user, string newEmailAddress)
    {
        ChangeEmailNotificationModel model = new ChangeEmailNotificationModel(user.DisplayName, user.EmailAddress, newEmailAddress);
        var htmlResult = await GetHTMLString("ChangeEmailNotification", model);
        return htmlResult;
    }

    public async Task<string> BuildChangeEmailEmail(FantasyCriticUser user, string link)
    {
        ChangeEmailModel model = new ChangeEmailModel(user, link);
        var htmlResult = await GetHTMLString("ChangeEmail", model);
        return htmlResult;
    }

    public async Task<string> BuildConfirmEmailEmail(FantasyCriticUser user, string link)
    {
        ConfirmEmailModel model = new ConfirmEmailModel(user, link);
        var htmlResult = await GetHTMLString("ConfirmEmail", model);
        return htmlResult;
    }

    public async Task<string> BuildLeagueInviteEmail(League league, string baseURL)
    {
        LeagueInviteModel model = new LeagueInviteModel(league, baseURL);
        var htmlResult = await GetHTMLString("LeagueInvite", model);
        return htmlResult;
    }

    public async Task<string> BuildSiteInviteEmail(League league, string baseURL)
    {
        LeagueInviteModel model = new LeagueInviteModel(league, baseURL);
        var htmlResult = await GetHTMLString("SiteInvite", model);
        return htmlResult;
    }

    public async Task<string> BuildPasswordResetEmail(FantasyCriticUser user, string link)
    {
        PasswordResetModel model = new PasswordResetModel(user, link);
        var htmlResult = await GetHTMLString("PasswordReset", model);
        return htmlResult;
    }

    public async Task<string> BuildPublicBidEmail(FantasyCriticUser user, IReadOnlyList<LeagueYearPublicBiddingSet> publicBiddingSets, string baseAddress, bool isProduction)
    {
        PublicBidEmailModel model = new PublicBidEmailModel(user, publicBiddingSets, baseAddress, isProduction);
        var htmlResult = await GetHTMLString("PublicBids", model);
        return htmlResult;
    }

    private Task<string> GetHTMLString(string template, object model)
    {
        return RazorTemplateEngine.RenderAsync($"/Views/{template}.cshtml", model);
    }
}
