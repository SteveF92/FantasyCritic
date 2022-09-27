using System;
using System.IO;
using System.Threading.Tasks;
using Amazon;
using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;

namespace FantasyCritic.AWS;
public class SecretsManagerConfigurationStore
{
    private readonly string _region;
    private readonly string _appName;
    private readonly string _environment;


    public SecretsManagerConfigurationStore(string region, string appName, string environment)
    {
        _region = region;
        _appName = appName;
        _environment = environment.ToLower();
    }

    public async Task<string> GetConfiguration()
    {
        IAmazonSecretsManager client = new AmazonSecretsManagerClient(RegionEndpoint.GetBySystemName(_region));

        var request = new GetSecretValueRequest()
        {
            SecretId = $"{_appName}/{_environment}/appsettings"
        };
        var secretResponse = await client.GetSecretValueAsync(request);

        string fullJSONString;
        if (secretResponse.SecretString != null)
        {
            fullJSONString = secretResponse.SecretString;
        }
        else
        {
            StreamReader reader = new StreamReader(secretResponse.SecretBinary);
            fullJSONString = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(reader.ReadToEnd()));
        }

        return fullJSONString;
    }
}
