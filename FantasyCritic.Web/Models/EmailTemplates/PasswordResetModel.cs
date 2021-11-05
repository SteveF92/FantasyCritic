using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using FantasyCritic.Lib.Domain;
using FantasyCritic.Lib.Identity;

namespace FantasyCritic.Web.Models.EmailTemplates
{
    public class PasswordResetModel
    {
        public PasswordResetModel(FantasyCriticUser user, string resetCode, string baseURL)
        {
            DisplayName = user.DisplayName;
            Link = $"{baseURL}/resetPassword?Code={UrlEncoder.Default.Encode(resetCode)}";
        }

        public string DisplayName { get; }
        public string Link { get; }
    }
}
