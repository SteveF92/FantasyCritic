using System.Collections.Generic;
using FantasyCritic.Lib.Domain;

namespace FantasyCritic.Web.Models.Responses
{
    public class FantasyCriticUserViewModel
    {
        public FantasyCriticUserViewModel(FantasyCriticUser user, IEnumerable<string> roles)
        {
            UserName = user.UserName;
            RealName = user.RealName;
            EmailAddress = user.EmailAddress;
            Roles = roles;
            EmailConfirmed = user.EmailConfirmed;
        }

        public string UserName { get; }
        public string RealName { get; }
        public string EmailAddress { get; }
        public IEnumerable<string> Roles { get; }
        public bool EmailConfirmed { get; }
    }
}
