namespace FantasyCritic.Lib.DependencyInjection;

public class ConfigurationStoreSet : IConfigurationStore
{
    private readonly IConfigurationStore _normalStore;
    private readonly IConfigurationStore _secretStore;

    public ConfigurationStoreSet(IConfigurationStore normalStore, IConfigurationStore secretStore)
    {
        _normalStore = normalStore;
        _secretStore = secretStore;
    }

    public string GetConfigValue(string name)
    {
        var primaryStoreValue = _normalStore.GetConfigValue(name);
        if (primaryStoreValue != "secret")
        {
            return primaryStoreValue;
        }

        return _secretStore.GetConfigValue(name);
    }

    public string GetConnectionString(string name)
    {
        var primaryStoreValue = _normalStore.GetConnectionString(name);
        if (primaryStoreValue != "secret")
        {
            return primaryStoreValue;
        }

        return _secretStore.GetConnectionString(name);
    }

    public string GetAWSRegion()
    {
        var primaryStoreValue = _normalStore.GetAWSRegion();
        if (primaryStoreValue != "secret")
        {
            return primaryStoreValue;
        }

        return _secretStore.GetAWSRegion();
    }
}
