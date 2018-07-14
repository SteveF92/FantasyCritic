using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FantasyCritic.Lib.Domain;

namespace FantasyCritic.Web.Models
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
