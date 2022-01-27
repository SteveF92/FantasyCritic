using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
