using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using FantasyCritic.Lib.Domain;

namespace FantasyCritic.Lib.Extensions
{
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
    }
}
