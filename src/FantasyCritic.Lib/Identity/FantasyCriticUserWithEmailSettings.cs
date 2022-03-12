using FantasyCritic.Lib.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FantasyCritic.Lib.Identity
{
    public class FantasyCriticUserWithEmailSettings
    {
        public FantasyCriticUserWithEmailSettings(FantasyCriticUser user, IEnumerable<EmailType> emailTypes)
        {
            User = user;
            EmailTypes = emailTypes.ToList();
        }

        public FantasyCriticUser User { get; }
        public IReadOnlyList<EmailType> EmailTypes { get; }
    }
}
