using System.Diagnostics.CodeAnalysis;
using System.Net;
using FantasyCritic.Lib.DependencyInjection;
using FantasyCritic.Lib.Identity;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MySqlConnector;
using Serilog;
using IResult = Microsoft.AspNetCore.Http.IResult;

namespace FantasyCritic.Web.Development;

/// <summary>
/// Signs in as any user, with no password, on a developer's machine. See docs/dev-sign-in-as-plan.md.
/// </summary>
public static class DevSignInEndpoint
{
    private static readonly ILogger _logger = Log.ForContext(typeof(DevSignInEndpoint));

    private static readonly IReadOnlyList<string> LocalServers = ["localhost", "127.0.0.1", "::1"];

    public static void MapDevSignIn(this WebApplication app)
    {
        var connectionString = app.Services.GetRequiredService<RepositoryConfiguration>().ConnectionString;
        if (!IsAllowed(app.Environment.EnvironmentName, connectionString))
        {
            return;
        }

        _logger.Warning("Dev sign-in-as is enabled at /dev/sign-in-as.");
        app.MapGet("/dev/sign-in-as", SignInAs).ExcludeFromDescription();
    }

    public static bool IsAllowed(string environmentName, string connectionString)
    {
        if (!string.Equals(environmentName, Environments.Development, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var servers = new MySqlConnectionStringBuilder(connectionString).Server
            .Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

        return servers.Length > 0 && servers.All(server => LocalServers.Contains(server, StringComparer.OrdinalIgnoreCase));
    }

    private static async Task<IResult> SignInAs(Guid userId, string? returnUrl, HttpContext context,
        FantasyCriticUserManager userManager, FantasyCriticSignInManager signInManager)
    {
        var remoteAddress = context.Connection.RemoteIpAddress;
        if (remoteAddress is null || !IPAddress.IsLoopback(remoteAddress))
        {
            return Results.NotFound();
        }

        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null || user.IsDeleted)
        {
            return Results.NotFound();
        }

        _logger.Warning("Dev sign-in-as: signed in as {UserID} ({DisplayName}).", user.Id, user.UserName);
        await signInManager.SignInAsync(user, isPersistent: false);

        return Results.Redirect(IsLocalPath(returnUrl) ? returnUrl : "/");
    }

    private static bool IsLocalPath([NotNullWhen(true)] string? url)
    {
        return url is not null && url.StartsWith('/') && !url.StartsWith("//") && !url.StartsWith("/\\");
    }
}
