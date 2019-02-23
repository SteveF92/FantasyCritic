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
        public FantasyCriticUser(Guid userID, string displayName, int displayNumber, string emailAddress, string normalizedEmailAddress, 
            bool emailConfirmed, string securityStamp, string passwordHash, Instant lastChangedCredentials)
        {
            UserID = userID;
            DisplayName = displayName;
            DisplayNumber = displayNumber;
            EmailAddress = emailAddress;
            NormalizedEmailAddress = normalizedEmailAddress;
            EmailConfirmed = emailConfirmed;
            SecurityStamp = securityStamp;
            PasswordHash = passwordHash;
            LastChangedCredentials = lastChangedCredentials;
        }

        public Guid UserID { get; set; }
        public string DisplayName { get; set; }
        public int DisplayNumber { get; set; }
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
