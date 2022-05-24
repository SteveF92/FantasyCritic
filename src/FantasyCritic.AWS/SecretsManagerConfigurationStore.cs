using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FantasyCritic.Lib.DependencyInjection;

namespace FantasyCritic.AWS;
public class SecretsManagerConfigurationStore : IConfigurationStore
{
    private readonly string _region;
    private readonly Dictionary<string, string> _parameterCache;

    public SecretsManagerConfigurationStore(string region)
    {
        _region = region;
        _parameterCache = new Dictionary<string, string>();
    }

    public string GetConfigValue(string name)
    {
        if (!_parameterCache.TryGetValue(name, out var value))
        {
            throw new Exception($"Config value: {name} not found.");
        }

        if (string.IsNullOrWhiteSpace(value))
        {
            throw new Exception($"Config value: {name} not found.");
        }

        return value;
    }

    public Task PopulateAllValues()
    {
        return Task.CompletedTask;
    }
}
