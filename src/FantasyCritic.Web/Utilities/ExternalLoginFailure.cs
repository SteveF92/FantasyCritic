using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FantasyCritic.Web.Utilities;

/// <summary>
/// Sends a failed or cancelled external sign-in back to the page it started from with a message, instead of the error page.
/// A signed-in user was linking a login from Manage/ExternalLogins; anyone else was logging in.
/// </summary>
public static class ExternalLoginFailure
{
    public static async Task Handle(RemoteFailureContext context)
    {
        var httpContext = context.HttpContext;
        var providerName = context.Scheme.DisplayName ?? context.Scheme.Name;

        var logger = httpContext.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger(typeof(ExternalLoginFailure));
        logger.LogWarning("External sign-in with {Provider} failed: {FailureMessage}", context.Scheme.Name, context.Failure?.Message);

        var applicationCookie = await httpContext.AuthenticateAsync(IdentityConstants.ApplicationScheme);
        bool linkingLogin = applicationCookie.Succeeded;

        var tempData = httpContext.RequestServices.GetRequiredService<ITempDataDictionaryFactory>().GetTempData(httpContext);
        if (linkingLogin)
        {
            tempData["StatusMessage"] = $"Error: Linking your {providerName} account was cancelled or could not be completed. Please try again.";
            context.Response.Redirect("/Account/Manage/ExternalLogins");
        }
        else
        {
            tempData["ErrorMessage"] = $"Signing in with {providerName} was cancelled or could not be completed. Please try again.";
            context.Response.Redirect("/Account/Login");
        }

        tempData.Save();
        context.HandleResponse();
    }
}
