using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;

namespace FantasyCritic.HostingTests;

internal sealed class TestHostEnvironment : IHostEnvironment
{
    public TestHostEnvironment(string environmentName)
    {
        EnvironmentName = environmentName;
    }

    public string EnvironmentName { get; set; }
    public string ApplicationName { get; set; } = "FantasyCritic.HostingTests";
    public string ContentRootPath { get; set; } = "";
    public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
}
