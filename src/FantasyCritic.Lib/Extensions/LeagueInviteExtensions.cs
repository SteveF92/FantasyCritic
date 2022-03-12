using FantasyCritic.Lib.Identity;

namespace FantasyCritic.Lib.Extensions;

public static class LeagueInviteExtensions
{
    public static bool UserIsInvited(this IEnumerable<LeagueInvite> invites, string emailAddress)
    {
        var invited = invites.Any(x => string.Equals(x.EmailAddress, emailAddress, StringComparison.OrdinalIgnoreCase));
        return invited;
    }

    public static Maybe<LeagueInvite> GetMatchingInvite(this IEnumerable<LeagueInvite> invites, string emailAddress)
    {
        var matchingInvite = invites.SingleOrDefault(x => string.Equals(x.EmailAddress, emailAddress, StringComparison.OrdinalIgnoreCase));
        return matchingInvite;
    }

    public static Maybe<LeagueInvite> GetMatchingInvite(this IEnumerable<LeagueInvite> invites, FantasyCriticUser user)
    {
        var matchingInvite = invites.SingleOrDefault(x => x.User.HasValue && x.User.Value.Id == user.Id);
        return matchingInvite;
    }
}