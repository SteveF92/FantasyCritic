using Microsoft.AspNetCore.Identity;

namespace FantasyCritic.Hosting;

/// <summary>
/// The Identity options the site has always used. Shared so a host that only needs a
/// <see cref="Lib.Identity.FantasyCriticUserManager"/> validates usernames and passwords exactly
/// the way the web app does — a bot that accepted a username the site would reject would be a
/// confusing way to find out these had drifted apart.
/// </summary>
public static class FantasyCriticIdentityOptions
{
    public static void Configure(IdentityOptions options)
    {
        options.SignIn.RequireConfirmedAccount = false;

        const string letters = "abcdefghijklmnopqrstuvwxyz";
        const string numbers = "0123456789";
        const string specials = "-._@+ ";
        options.User.AllowedUserNameCharacters = letters + letters.ToUpper() + numbers + specials;

        options.Password.RequiredLength = 10;
        options.Password.RequireDigit = false;
        options.Password.RequireLowercase = false;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireUppercase = false;
        options.Password.RequiredUniqueChars = 5;
    }
}
