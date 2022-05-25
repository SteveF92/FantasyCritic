namespace FantasyCritic.Lib.DependencyInjection;

public class ConfigurationStoreSet : IConfigurationStore
{
    private readonly IConfigurationStore _primaryStore;
    private readonly IConfigurationStore _secondaryStore;

    public ConfigurationStoreSet(IConfigurationStore primaryStore, IConfigurationStore secondaryStore)
    {
        _primaryStore = primaryStore;
        _secondaryStore = secondaryStore;
    }

    public string GetConfigValue(string name)
    {
        var primaryStoreValue = _primaryStore.GetConfigValue(name);
        if (primaryStoreValue != "secret")
        {
            return primaryStoreValue;
        }

        return _secondaryStore.GetConfigValue(name);
    }

    public string GetConnectionString(string name)
    {
        var primaryStoreValue = _primaryStore.GetConnectionString(name);
        if (primaryStoreValue != "secret")
        {
            return primaryStoreValue;
        }

        return _secondaryStore.GetConnectionString(name);
    }

    public string GetAWSRegion()
    {
        var primaryStoreValue = _primaryStore.GetAWSRegion();
        if (primaryStoreValue != "secret")
        {
            return primaryStoreValue;
        }

        return _secondaryStore.GetAWSRegion();
    }
}
