using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Identity;

namespace FantasyCritic.Lib.Domain
{
    public class FantasyCriticUser
    {
        public Guid UserID { get; set; }

        public string UserName { get; set; }

        public string NormalizedUserName { get; set; }

        public string EmailAddress { get; set; }
        public string NormalizedEmailAddress { get; set; }

        public bool EmailConfirmed { get; set; }

        public string SecurityStamp { get; set; }

        public string PasswordHash { get; set; }
    }
}
