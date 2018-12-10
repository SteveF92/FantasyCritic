using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using FantasyCritic.Lib.Domain;

namespace FantasyCritic.Web.Models.EmailTemplates
{
    public class ChangeEmailModel
    {
        public ChangeEmailModel(FantasyCriticUser user, string newEmailAddress, string changeCode, string baseURL)
        {
            Username = user.UserName;
            Link = $"{baseURL}/changeEmail?NewEmailAddress={UrlEncoder.Default.Encode(newEmailAddress)}&Code={UrlEncoder.Default.Encode(changeCode)}";
        }

        public string Username { get; }
        public string Link { get; }
    }
}
