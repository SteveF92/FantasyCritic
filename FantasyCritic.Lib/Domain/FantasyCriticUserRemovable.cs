using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Identity;
using NodaTime;

namespace FantasyCritic.Lib.Domain
{
    public class FantasyCriticUserRemovable
    {
        public FantasyCriticUserRemovable(FantasyCriticUser user, bool removable)
        {
            User = user;
            Removable = removable;
        }

        public FantasyCriticUser User { get; }
        public bool Removable { get; }
    }
}
