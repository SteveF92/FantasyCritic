using System.Text.Encodings.Web;
using FantasyCritic.Lib.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

namespace FantasyCritic.Web.Utilities;

public static class LinkBuilder
{
    public static string GetBaseAddress(HttpRequest request) => $"{request.Scheme}://{request.Host.Value}";

    public static async Task<string> GetConfirmEmailLink(FantasyCriticUserManager userManager, FantasyCriticUser user, HttpRequest request)
    {
        var code = await userManager.GenerateEmailConfirmationTokenAsync(user);
        var encodedCode = UrlEncoder.Default.Encode(code);
        var link = $"{GetBaseAddress(request)}/Identity/Account/ConfirmEmail?userId={user.Id}&code={encodedCode}";
        return link;
    }

    public static async Task<string> GetForgotPasswordLink(FantasyCriticUserManager userManager, FantasyCriticUser user, HttpRequest request)
    {
        var code = await userManager.GeneratePasswordResetTokenAsync(user);
        var encodedCode = UrlEncoder.Default.Encode(code);
        var link = $"{GetBaseAddress(request)}/Identity/Account/ResetPassword?code={encodedCode}";
        return link;
    }

    public static async Task<string> GetChangeEmailLink(FantasyCriticUserManager userManager, FantasyCriticUser user, string newEmail, HttpRequest request)
    {
        var code = await userManager.GenerateChangeEmailTokenAsync(user, newEmail);
        var encodedCode = UrlEncoder.Default.Encode(code);
        var encodedNewEmail = UrlEncoder.Default.Encode(newEmail);
        var link = $"{GetBaseAddress(request)}/Identity/Account/ConfirmEmailChange?userId={user.Id}&email={encodedNewEmail}&code={encodedCode}";
        return link;
    }
}
