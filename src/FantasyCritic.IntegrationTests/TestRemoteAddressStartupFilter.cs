using System;
using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;

namespace FantasyCritic.IntegrationTests;

/// <summary>
/// TestServer sends every request with no remote address. This gives each one the loopback address a browser on the
/// developer's machine would have, or the address in <see cref="HeaderName"/> when a test wants to come from elsewhere.
/// </summary>
public sealed class TestRemoteAddressStartupFilter : IStartupFilter
{
    public const string HeaderName = "X-Test-Remote-Address";

    public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
    {
        return app =>
        {
            app.Use(async (context, nextMiddleware) =>
            {
                context.Connection.RemoteIpAddress = context.Request.Headers.TryGetValue(HeaderName, out var address)
                    ? IPAddress.Parse(address.ToString())
                    : IPAddress.Loopback;

                await nextMiddleware();
            });

            next(app);
        };
    }
}
