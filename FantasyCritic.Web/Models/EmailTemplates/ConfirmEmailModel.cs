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
    public class ConfirmEmailModel
    {
        public ConfirmEmailModel(FantasyCriticUser user, string confirmCode, string baseURL)
        {
            DisplayName = user.UserName;
            Link = $"{baseURL}/confirmEmail?Id={user.Id}&Code={UrlEncoder.Default.Encode(confirmCode)}";
            UserID = user.Id.ToString();
            Code = confirmCode;
        }

        public string DisplayName { get; }
        public string Link { get; }
        public string UserID { get; }
        public string Code { get; }
    }
}
