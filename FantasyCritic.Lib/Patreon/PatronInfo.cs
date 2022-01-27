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
        public PatronInfo(FantasyCriticUser user)
        {
            User = user;
        }

        public FantasyCriticUser User { get; }
    }
}
