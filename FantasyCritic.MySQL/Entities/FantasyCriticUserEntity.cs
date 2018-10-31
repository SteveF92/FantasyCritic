using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FantasyCritic.Lib.Domain;
using NodaTime;

namespace FantasyCritic.MySQL.Entities
{
    internal class FantasyCriticUserEntity
    {
        public FantasyCriticUserEntity()
        {

        }

        public FantasyCriticUserEntity(FantasyCriticUser user)
        {
            UserID = user.UserID;
            UserName = user.UserName;
            NormalizedUserName = user.NormalizedUserName;
            EmailAddress = user.EmailAddress;
            NormalizedEmailAddress = user.NormalizedEmailAddress;
            EmailConfirmed = user.EmailConfirmed;
            SecurityStamp = user.SecurityStamp;
            PasswordHash = user.PasswordHash;
            LastChangedCredentials = user.LastChangedCredentials.ToDateTimeUtc();
        }

        public Guid UserID { get; set; }
        public string UserName { get; set; }
        public string NormalizedUserName { get; set; }
        public string EmailAddress { get; set; }
        public string NormalizedEmailAddress { get; set; }
        public bool EmailConfirmed { get; set; }
        public string SecurityStamp { get; set; }
        public string PasswordHash { get; set; }
        public DateTime LastChangedCredentials { get; set; }

        public FantasyCriticUser ToDomain()
        {
            Instant instant = LocalDateTime.FromDateTime(LastChangedCredentials).InZoneStrictly(DateTimeZone.Utc).ToInstant();
            FantasyCriticUser domain = new FantasyCriticUser(UserID, UserName, NormalizedUserName, EmailAddress, NormalizedUserName, EmailConfirmed, SecurityStamp, PasswordHash, instant);
            return domain;
        }
    }
}
