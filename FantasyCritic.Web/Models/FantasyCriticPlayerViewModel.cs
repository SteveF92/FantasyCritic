using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FantasyCritic.Lib.Domain;

namespace FantasyCritic.Web.Models
{
    public class FantasyCriticPlayerViewModel
    {
        public FantasyCriticPlayerViewModel(FantasyCriticUser user)
        {
            UserName = user.UserName;
        }

        public string UserName { get; }
    }
}
