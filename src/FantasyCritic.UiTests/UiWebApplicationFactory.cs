using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using FantasyCritic.IntegrationTests;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;

namespace FantasyCritic.UiTests;

/// <summary>
/// The integration test host on a real Kestrel port, serving the client app's production build from its dist folder.
/// </summary>
public sealed class UiWebApplicationFactory : FantasyCriticWebApplicationFactory
{
    private readonly string _distPath;

    public UiWebApplicationFactory(string distPath)
    {
        _distPath = distPath;
        UseKestrel(options => options.Listen(IPAddress.Loopback, 0));
    }

    /// <summary>
    /// Where the browser reaches the app. Set once <see cref="Start"/> has run.
    /// </summary>
    public Uri BaseAddress { get; private set; } = null!;

    public void Start()
    {
        StartServer();

        var address = Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>()!.Addresses.Single();
        BaseAddress = new UriBuilder(address) { Host = "localhost" }.Uri;
    }

    /// <summary>
    /// On Kestrel the factory's own clients end in a socket handler that follows redirects itself, so a session gets
    /// a plain handler instead.
    /// </summary>
    public override HttpClient CreateSessionClient() => new(new SocketsHttpHandler
    {
        AllowAutoRedirect = false,
        UseCookies = true,
        CookieContainer = new CookieContainer(),
    })
    {
        BaseAddress = BaseAddress,
    };

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);

        var distFiles = new PhysicalFileProvider(_distPath);
        builder.ConfigureServices((context, services) =>
        {
            // MapFallbackToFile serves index.html from the web root, so the build's index.html goes in front of wwwroot.
            context.HostingEnvironment.WebRootFileProvider = new CompositeFileProvider(distFiles, context.HostingEnvironment.WebRootFileProvider);
            services.AddSingleton<IStartupFilter>(new DistStaticFilesStartupFilter(distFiles));
        });
    }

    /// <summary>
    /// The build's scripts and styles aren't in Web's static asset manifest, so they are served as plain static files first.
    /// </summary>
    private sealed class DistStaticFilesStartupFilter(IFileProvider distFiles) : IStartupFilter
    {
        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next) => app =>
        {
            app.UseStaticFiles(new StaticFileOptions { FileProvider = distFiles });
            next(app);
        };
    }

    /// <summary>
    /// The client app's dist folder, refusing to run against a build that is missing or older than the source it came from.
    /// </summary>
    public static string FindCurrentDist()
    {
        var clientApp = Path.Combine(RepositoryRoot.Find(), "src", "FantasyCritic.VueClientApp");
        var dist = Path.Combine(clientApp, "dist");
        var index = new FileInfo(Path.Combine(dist, "index.html"));
        if (!index.Exists)
        {
            throw new InvalidOperationException($"There is no client build at {dist}. Run scripts/Run-UiTests.ps1, or npm run build in {clientApp}.");
        }

        var newestSource = Directory.EnumerateFiles(Path.Combine(clientApp, "src"), "*", SearchOption.AllDirectories)
            .Select(file => new FileInfo(file))
            .MaxBy(file => file.LastWriteTimeUtc)!;
        if (newestSource.LastWriteTimeUtc > index.LastWriteTimeUtc)
        {
            throw new InvalidOperationException(
                $"The client build at {dist} is older than {Path.GetRelativePath(clientApp, newestSource.FullName)}. Run scripts/Run-UiTests.ps1, or npm run build in {clientApp}.");
        }

        return dist;
    }
}
