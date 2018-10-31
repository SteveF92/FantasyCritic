using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Identity;
using NodaTime;

namespace FantasyCritic.Lib.Domain
{
    public class FantasyCriticUser
    {
        public FantasyCriticUser(Guid userID, string userName, string normalizedUserName, string emailAddress, 
            string normalizedEmailAddress, bool emailConfirmed, string securityStamp, string passwordHash, Instant lastChangedCredentials)
        {
            UserID = userID;
            UserName = userName;
            NormalizedUserName = normalizedUserName;
            EmailAddress = emailAddress;
            NormalizedEmailAddress = normalizedEmailAddress;
            EmailConfirmed = emailConfirmed;
            SecurityStamp = securityStamp;
            PasswordHash = passwordHash;
            LastChangedCredentials = lastChangedCredentials;
        }

        public Guid UserID { get; set; }

        public string UserName { get; set; }

        public string NormalizedUserName { get; set; }

        public string EmailAddress { get; set; }

        public string NormalizedEmailAddress { get; set; }

        public bool EmailConfirmed { get; set; }

        public string SecurityStamp { get; set; }

        public string PasswordHash { get; set; }

        public Instant LastChangedCredentials { get; set; }

        public void UpdateLastUsedCredentials(Instant currentInstant)
        {
            LastChangedCredentials = currentInstant;
        }
    }
}
