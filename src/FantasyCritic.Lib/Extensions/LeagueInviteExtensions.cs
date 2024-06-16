using FantasyCritic.Lib.Identity;

namespace FantasyCritic.Lib.Extensions;

public static class LeagueInviteExtensions
{
    public static TLeagueInvite? GetMatchingInvite<TLeagueInvite>(this IEnumerable<TLeagueInvite> invites, string emailAddress) where TLeagueInvite : ILeagueInvite
    {
        var matchingInvite = invites.SingleOrDefault(x => string.Equals(x.EmailAddress, emailAddress, StringComparison.OrdinalIgnoreCase));
        return matchingInvite;
    }

    public static TLeagueInvite? GetMatchingInvite<TLeagueInvite>(this IEnumerable<TLeagueInvite> invites, FantasyCriticUser user) where TLeagueInvite : ILeagueInvite
    {
        var matchingInvite = invites.SingleOrDefault(x => x.InviteUser is not null && x.InviteUser.UserID == user.Id);
        return matchingInvite;
    }
}
