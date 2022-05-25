using FantasyCritic.Lib.DependencyInjection;
using Microsoft.Extensions.Configuration;

namespace FantasyCritic.Web.Utilities;
public class ConfigurationFileStore : IConfigurationStore
{
    private readonly ConfigurationManager _configurationManager;

    public ConfigurationFileStore(ConfigurationManager configurationManager)
    {
        _configurationManager = configurationManager;
    }

    public string GetConfigValue(string name)
    {
        var value = _configurationManager[name];
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new Exception($"Config value: {name} not found.");
        }

        return value;
    }

    public string GetConnectionString(string name) => GetConfigValue(name);
    public string GetAWSRegion()
    {
        return GetConfigValue("AWS:region");
    }
}
