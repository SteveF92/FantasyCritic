using Microsoft.AspNetCore.Authentication.Cookies;

namespace FantasyCritic.Web.Identity;
public static class SPACookieOptions
{
    public static CookieAuthenticationEvents GetModifiedEvents(CookieAuthenticationEvents standardEvents)
    {
        var options = new CookieAuthenticationEvents()
        {
            OnRedirectToLogin = (ctx) =>
            {
                if (ctx.Request.Path.StartsWithSegments("/api") && ctx.Response.StatusCode == 200)
                {
                    ctx.Response.StatusCode = 401;
                    return Task.CompletedTask;
                }

                return standardEvents.OnRedirectToLogin(ctx);
            },
            OnRedirectToAccessDenied = (ctx) =>
            {
                if (ctx.Request.Path.StartsWithSegments("/api") && ctx.Response.StatusCode == 200)
                {
                    ctx.Response.StatusCode = 403;
                    return Task.CompletedTask;
                }

                return standardEvents.OnRedirectToAccessDenied(ctx);
            },
            OnCheckSlidingExpiration = (ctx) => standardEvents.OnCheckSlidingExpiration(ctx),
            OnRedirectToLogout = (ctx) => standardEvents.OnRedirectToLogout(ctx),
            OnRedirectToReturnUrl = (ctx) => standardEvents.OnRedirectToLogout(ctx),
            OnSignedIn = (ctx) => standardEvents.OnSignedIn(ctx),
            OnSigningIn = (ctx) => standardEvents.OnSigningIn(ctx),
            OnSigningOut = (ctx) => standardEvents.OnSigningOut(ctx),
            OnValidatePrincipal = (ctx) => standardEvents.OnValidatePrincipal(ctx)
        };

        return options;
    }
}
