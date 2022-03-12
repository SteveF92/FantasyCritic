using Microsoft.AspNetCore.Identity;

namespace FantasyCritic.Lib.Identity
{
    public class FantasyCriticUserWithExternalLogins
    {
        public FantasyCriticUserWithExternalLogins(FantasyCriticUser user, IEnumerable<UserLoginInfo> userLogins)
        {
            User = user;
            UserLogins = userLogins.ToList();
        }

        public FantasyCriticUser User { get; }
        public IReadOnlyList<UserLoginInfo> UserLogins { get; }
    }
}
