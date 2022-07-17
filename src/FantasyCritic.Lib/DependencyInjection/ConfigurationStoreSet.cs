namespace FantasyCritic.Lib.DependencyInjection;

public class ConfigurationStoreSet : IConfigurationStore
{
    private readonly IReadOnlyList<IConfigurationStore> _stores;

    public ConfigurationStoreSet(IEnumerable<IConfigurationStore> stores)
    {
        _stores = stores.Reverse().ToList();
    }

    public string? GetConfigValue(string name)
    {
        foreach (var store in _stores)
        {
            var value = store.GetConfigValue(name);
            if (value is not null)
            {
                return value;
            }
        }

        return null;
    }

    public string? GetConnectionString(string name)
    {
        foreach (var store in _stores)
        {
            var value = store.GetConnectionString(name);
            if (value is not null)
            {
                return value;
            }
        }

        return null;
    }

    public string AssertConfigValue(string name)
    {
        var value = GetConfigValue(name);
        if (value is null)
        {
            throw new Exception($"Cannot get config value: {name}");
        }

        return value;
    }

    public string AssertConnectionString(string name)
    {
        var value = GetConnectionString(name);
        if (value is null)
        {
            throw new Exception($"Cannot get connection string: {name}");
        }

        return value;
    }
}
