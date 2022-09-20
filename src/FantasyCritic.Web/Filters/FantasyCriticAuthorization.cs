using Microsoft.AspNetCore.Authorization;

namespace FantasyCritic.Web.Filters;

// Handler to be able to look at the context when authorization completes
// Allows you to look at success or failure and the user info
// Not necessary in Production, only for debugging
public class FantasyCriticAuthorizationHandler : IAuthorizationHandler
{
    public Task HandleAsync(AuthorizationHandlerContext context)
    {
        var user = context?.User?.Identity?.Name;
        var isAuth = context?.User?.Identity?.IsAuthenticated;
        if (context?.FailureReasons.Count() > 0)
        {
            context.ToString();
        }
        return Task.CompletedTask;
    }
}
