using System;
using System.Collections.Generic;
using FantasyCritic.Lib.Domain;
using FantasyCritic.Lib.Identity;

namespace FantasyCritic.Web.Models.Responses
{
    public class FantasyCriticUserViewModel
    {
        public FantasyCriticUserViewModel(FantasyCriticUser user, IEnumerable<string> roles)
        {
            UserID = user.Id;
            DisplayName = user.UserName;
            DisplayNumber = user.DisplayNumber;
            EmailAddress = user.Email;
            Roles = roles;
            EmailConfirmed = user.EmailConfirmed;
        }

        public Guid UserID { get; }
        public string DisplayName { get; }
        public int DisplayNumber { get; }
        public string EmailAddress { get; }
        public IEnumerable<string> Roles { get; }
        public bool EmailConfirmed { get; }
    }
}
