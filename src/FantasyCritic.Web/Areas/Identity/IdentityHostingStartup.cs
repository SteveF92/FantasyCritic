using Microsoft.AspNetCore.Hosting;

[assembly: HostingStartup(typeof(FantasyCritic.Web.Areas.Identity.IdentityHostingStartup))]
namespace FantasyCritic.Web.Areas.Identity;

public class IdentityHostingStartup : IHostingStartup
{
    public void Configure(IWebHostBuilder builder)
    {
        builder.ConfigureServices((context, services) =>
        {
        });
    }
}