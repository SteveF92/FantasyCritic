using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using FantasyCritic.Lib.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;

namespace FantasyCritic.Web.Utilities
{
    public static class LinkBuilder
    {
        public static async Task<string> GetConfirmEmailLink(UserManager<FantasyCriticUser> userManager, FantasyCriticUser user, HttpRequest request)
        {
            var code = await userManager.GenerateEmailConfirmationTokenAsync(user);
            var encodedCode = UrlEncoder.Default.Encode(code);
            var link = $"{request.Scheme}://{request.Host.Value}/Identity/Account/ConfirmEmail?userID={user.Id}&code={encodedCode}";
            return link;
        }
    }
}
