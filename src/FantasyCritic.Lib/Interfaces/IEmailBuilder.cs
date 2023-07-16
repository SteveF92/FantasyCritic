using FantasyCritic.Lib.Domain.Combinations;
using FantasyCritic.Lib.Identity;

namespace FantasyCritic.Lib.Interfaces;
public interface IEmailBuilder
{
    public Task<string> BuildChangeEmailEmail(FantasyCriticUser user, string link);
    public Task<string> BuildConfirmEmailEmail(FantasyCriticUser user, string link);
    public Task<string> BuildLeagueInviteEmail(League league, string baseURL);
    public Task<string> BuildSiteInviteEmail(League league, string baseURL);
    public Task<string> BuildPasswordResetEmail(FantasyCriticUser user, string link);
    public Task<string> BuildPublicBidEmail(FantasyCriticUser user, IReadOnlyList<LeagueYearPublicBiddingSet> publicBiddingSets, string baseAddress, bool isProduction);
}
