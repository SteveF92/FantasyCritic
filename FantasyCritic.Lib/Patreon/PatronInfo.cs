using FantasyCritic.Lib.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FantasyCritic.Lib.Patreon
{
    public class PatronInfo
    {
        public PatronInfo(FantasyCriticUser user, bool isPlusUser)
        {
            User = user;
            IsPlusUser = isPlusUser;
        }

        public FantasyCriticUser User { get; }
        public bool IsPlusUser { get; }
    }
}
