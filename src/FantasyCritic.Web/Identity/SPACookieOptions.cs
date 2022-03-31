using Microsoft.AspNetCore.Authentication.Cookies;

namespace FantasyCritic.Web.Identity;
public static class SPACookieOptions
{
    public static CookieAuthenticationEvents GetEvents()
    {
        var options = new CookieAuthenticationEvents()
        {
            OnRedirectToLogin = (ctx) =>
            {
                if (ctx.Request.Path.StartsWithSegments("/api") && ctx.Response.StatusCode == 200)
                {
                    ctx.Response.StatusCode = 401;
                }

                return Task.CompletedTask;
            },
            OnRedirectToAccessDenied = (ctx) =>
            {
                if (ctx.Request.Path.StartsWithSegments("/api") && ctx.Response.StatusCode == 200)
                {
                    ctx.Response.StatusCode = 403;
                }

                return Task.CompletedTask;
            }
        };

        return options;
    }
}
