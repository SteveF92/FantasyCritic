using FantasyCritic.Lib.Domain.Combinations;
using FantasyCritic.Lib.Identity;

namespace FantasyCritic.Lib.Interfaces;

public interface IEmailBuilder
{
    Task<string> BuildEmailChangeNotificationEmail(FantasyCriticUser user, string newEmailAddress);
    Task<string> BuildChangeEmailEmail(FantasyCriticUser user, string link);
    Task<string> BuildConfirmEmailEmail(FantasyCriticUser user, string link);
    Task<string> BuildLeagueInviteEmail(League league, string baseURL);
    Task<string> BuildSiteInviteEmail(League league, string baseURL);
    Task<string> BuildPasswordResetEmail(FantasyCriticUser user, string link);
    Task<string> BuildPublicBidEmail(FantasyCriticUser user, IReadOnlyList<LeagueYearPublicBiddingSet> publicBiddingSets, string baseAddress, bool isProduction);
}
