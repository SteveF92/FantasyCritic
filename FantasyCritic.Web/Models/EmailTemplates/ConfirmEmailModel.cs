using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using FantasyCritic.Lib.Domain;

namespace FantasyCritic.Web.Models.EmailTemplates
{
    public class ConfirmEmailModel
    {
        public ConfirmEmailModel(FantasyCriticUser user, string confirmCode, string baseURL)
        {
            Username = user.UserName;
            Link = $"{baseURL}/confirmEmail?UserID={user.UserID}&Code={UrlEncoder.Default.Encode(confirmCode)}";
        }

        public string Username { get; }
        public string Link { get; }
    }
}
