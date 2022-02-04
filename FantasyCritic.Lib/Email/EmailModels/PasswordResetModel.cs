using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using FantasyCritic.Lib.Domain;
using FantasyCritic.Lib.Identity;

namespace FantasyCritic.Lib.Email.EmailModels
{
    public class PasswordResetModel
    {
        public PasswordResetModel(FantasyCriticUser user, string link)
        {
            DisplayName = user.UserName;
            Link = link;
        }

        public string DisplayName { get; }
        public string Link { get; }
    }
}
