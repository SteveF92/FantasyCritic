namespace FantasyCritic.Lib.DependencyInjection;
public interface IConfigurationStore
{
    string GetConfigValue(string name);
    string GetConnectionString(string name);
    string GetAWSRegion();
}
