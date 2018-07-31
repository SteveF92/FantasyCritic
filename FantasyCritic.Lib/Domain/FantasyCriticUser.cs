using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Identity;

namespace FantasyCritic.Lib.Domain
{
    public class FantasyCriticUser
    {
        public FantasyCriticUser(Guid userID, string userName, string normalizedUserName, string realName, string emailAddress, 
            string normalizedEmailAddress, bool emailConfirmed, string securityStamp, string passwordHash)
        {
            UserID = userID;
            UserName = userName;
            NormalizedUserName = normalizedUserName;
            RealName = realName;
            EmailAddress = emailAddress;
            NormalizedEmailAddress = normalizedEmailAddress;
            EmailConfirmed = emailConfirmed;
            SecurityStamp = securityStamp;
            PasswordHash = passwordHash;
        }

        public Guid UserID { get; set; }

        public string UserName { get; set; }

        public string NormalizedUserName { get; set; }

        public string RealName { get; set; }

        public string EmailAddress { get; set; }

        public string NormalizedEmailAddress { get; set; }

        public bool EmailConfirmed { get; set; }

        public string SecurityStamp { get; set; }

        public string PasswordHash { get; set; }

    }
}
