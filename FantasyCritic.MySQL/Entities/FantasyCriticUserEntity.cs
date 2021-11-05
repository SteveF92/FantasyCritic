using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FantasyCritic.Lib.Domain;
using FantasyCritic.Lib.Identity;
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
            DisplayName = user.DisplayName;
            DisplayNumber = user.DisplayNumber;
            EmailAddress = user.Email;
            NormalizedEmailAddress = user.NormalizedEmail;
            EmailConfirmed = user.EmailConfirmed;
            SecurityStamp = user.SecurityStamp;
            PasswordHash = user.PasswordHash;
            LastChangedCredentials = user.GetLastChangedCredentials();
            IsDeleted = user.IsDeleted;
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
        public bool IsDeleted { get; set; }

        public FantasyCriticUser ToDomain()
        {
            FantasyCriticUser domain = new FantasyCriticUser(UserID, DisplayName, DisplayNumber, EmailAddress, NormalizedEmailAddress, EmailConfirmed, 
                SecurityStamp, PasswordHash, LastChangedCredentials, IsDeleted);
            return domain;
        }
    }
}
