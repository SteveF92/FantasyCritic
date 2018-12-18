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
            DisplayName = user.DisplayName;
            Link = $"{baseURL}/changeEmail?NewEmailAddress={UrlEncoder.Default.Encode(newEmailAddress)}&Code={UrlEncoder.Default.Encode(changeCode)}";
        }

        public string DisplayName { get; }
        public string Link { get; }
    }
}
