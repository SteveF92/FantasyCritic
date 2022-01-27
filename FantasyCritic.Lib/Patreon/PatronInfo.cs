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
        public PatronInfo(FantasyCriticUser user, IEnumerable<string> patreonTiers)
        {
            User = user;
            PatreonTiers = patreonTiers.ToList();
        }

        public FantasyCriticUser User { get; }
        public IReadOnlyList<string> PatreonTiers { get; }
    }
}
