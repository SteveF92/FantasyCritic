using Microsoft.Extensions.Configuration;

namespace FantasyCritic.Lib.DependencyInjection;
public class NativeConfiguration : IConfigurationStore
{
    private readonly IConfigurationRoot _nativeConfiguration;

    public NativeConfiguration(IConfigurationRoot nativeConfiguration)
    {
        _nativeConfiguration = nativeConfiguration;
    }

    public string? GetConfigValue(string name)
    {
        return _nativeConfiguration[name];
    }

    public string? GetConnectionString(string name)
    {
        return _nativeConfiguration.GetConnectionString(name);
    }
}
