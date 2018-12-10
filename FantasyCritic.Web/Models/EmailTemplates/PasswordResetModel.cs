using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using FantasyCritic.Lib.Domain;

namespace FantasyCritic.Web.Models.EmailTemplates
{
    public class PasswordResetModel
    {
        public PasswordResetModel(FantasyCriticUser user, string resetCode, string baseURL)
        {
            Username = user.UserName;
            Link = $"{baseURL}/resetPassword?Code={UrlEncoder.Default.Encode(resetCode)}";
        }

        public string Username { get; }
        public string Link { get; }
    }
}
