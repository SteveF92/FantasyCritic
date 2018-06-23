using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Identity;

namespace FantasyCritic.Lib.Domain
{
    public class FantasyCriticUser
    {
        public FantasyCriticUser(Guid userID, string userName, string normalizedUserName, string emailAddress, 
            string normalizedEmailAddress, bool emailConfirmed, string securityStamp, string passwordHash, bool twoFactorEnabled)
        {
            UserID = userID;
            UserName = userName;
            NormalizedUserName = normalizedUserName;
            EmailAddress = emailAddress;
            NormalizedEmailAddress = normalizedEmailAddress;
            EmailConfirmed = emailConfirmed;
            SecurityStamp = securityStamp;
            PasswordHash = passwordHash;
            TwoFactorEnabled = twoFactorEnabled;
        }

        public Guid UserID { get; }

        public string UserName { get; }

        public string NormalizedUserName { get; }

        public string EmailAddress { get; set; }
        public string NormalizedEmailAddress { get; }

        public bool EmailConfirmed { get; }

        public string SecurityStamp { get; }

        public string PasswordHash { get; }

        public bool TwoFactorEnabled { get; }
    }
}
