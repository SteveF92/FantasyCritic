using System;
using System.Collections.Generic;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using NodaTime;

namespace FantasyCritic.Lib.Identity
{
    public class FantasyCriticUser : IdentityUser<Guid>
    {
        public FantasyCriticUser()
        {

        }

        public FantasyCriticUser(Guid userID, string displayName, int displayNumber, string emailAddress, string normalizedEmailAddress, 
            bool emailConfirmed, string securityStamp, string passwordHash, Instant lastChangedCredentials, bool isDeleted)
        {
            Id = userID;
            UserName = displayName;
            DisplayNumber = displayNumber;
            Email = emailAddress;
            NormalizedEmail = normalizedEmailAddress;
            EmailConfirmed = emailConfirmed;
            SecurityStamp = securityStamp;
            PasswordHash = passwordHash;
            LastChangedCredentials = lastChangedCredentials.ToDateTimeUtc();
            IsDeleted = isDeleted;
        }

        public Guid UserID => Id;
        public string DisplayName => UserName;

        public int DisplayNumber { get; set; }
        public DateTime LastChangedCredentials { get; set; }
        public bool IsDeleted { get; set; }

        public void UpdateLastUsedCredentials(Instant currentInstant)
        {
            LastChangedCredentials = currentInstant.ToDateTimeUtc();
        }

        public Instant GetLastChangedCredentials()
        {
            return Instant.FromDateTimeUtc(LastChangedCredentials);
        }

        public IReadOnlyList<Claim> GetUserClaims(IEnumerable<string> roles)
        {
            var usersClaims = new List<Claim>()
            {
                new Claim(ClaimTypes.Name, NormalizedEmail),
                new Claim(ClaimTypes.NameIdentifier, UserID.ToString()),
            };

            foreach (var role in roles)
            {
                usersClaims.Add(new Claim(ClaimTypes.Role, role));
            }

            return usersClaims;
        }
    }
}
