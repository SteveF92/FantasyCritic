using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Amazon;
using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using FantasyCritic.Lib.DependencyInjection;
using FantasyCritic.Lib.Extensions;
using Newtonsoft.Json;

namespace FantasyCritic.AWS;
public class SecretsManagerConfigurationStore : IConfigurationStore
{
    private const string SharedPrefix = "shared/";
    private readonly string _region;
    private readonly string _appName;
    private readonly string _environment;

    private readonly Dictionary<string, string> _environmentsSecretsCache;
    private readonly Dictionary<string, string> _sharedSecretsCache;

    public SecretsManagerConfigurationStore(string region, string appName, string environment)
    {
        _region = region;
        _appName = appName + "/";
        _environment = environment.ToLower() + "/";
        _environmentsSecretsCache = new Dictionary<string, string>();
        _sharedSecretsCache = new Dictionary<string, string>();
    }

    public string? GetConfigValue(string name)
    {
        if (_environmentsSecretsCache.TryGetValue(name, out var environmentValue))
        {
            return environmentValue;
        }

        if (_sharedSecretsCache.TryGetValue(name, out var sharedValue))
        {
            return sharedValue;
        }

        return null;
    }

    public string? GetConnectionString(string name)
    {
        var rawValue = GetConfigValue("ConnectionStrings:" + name);
        if (rawValue is null)
        {
            return null;
        }

        var connectionStringObject = JsonConvert.DeserializeObject<ConnectionStringEntity>(rawValue)!;
        return $"Server={connectionStringObject.Host};Database=fantasycritic;Uid={connectionStringObject.Username};Pwd={connectionStringObject.Password};charset=utf8;sslMode=required;MaximumPoolsize=50;";
    }

    public async Task PopulateAllValues()
    {
        IAmazonSecretsManager client = new AmazonSecretsManagerClient(RegionEndpoint.GetBySystemName(_region));

        string? nextToken = null;
        List<SecretListEntry> allSecrets = new List<SecretListEntry>();
        while (true)
        {
            var secretsRequest = new ListSecretsRequest()
            {
                Filters = new List<Filter>()
                {
                    new Filter()
                    {
                        Key = "name",
                        Values = new List<string>()
                        {
                            _appName
                        }
                    }
                }
            };

            if (nextToken is not null)
            {
                secretsRequest.NextToken = nextToken;
            }

            ListSecretsResponse? response = await client.ListSecretsAsync(secretsRequest);
            if (response is null)
            {
                break;
            }

            allSecrets.AddRange(response.SecretList);
            if (string.IsNullOrWhiteSpace(response.NextToken))
            {
                break;
            }

            nextToken = response.NextToken;
        }

        foreach (var secret in allSecrets)
        {
            var secretNameWithoutAppName = secret.Name.TrimStart(_appName);
            if (secretNameWithoutAppName.StartsWith(SharedPrefix))
            {
                await AddValueToCache(client, secret, secretNameWithoutAppName, SharedPrefix, _sharedSecretsCache);
            }
            else if (secretNameWithoutAppName.StartsWith(_environment))
            {
                await AddValueToCache(client, secret, secretNameWithoutAppName, _environment, _environmentsSecretsCache);
            }
        }
    }

    private async Task AddValueToCache(IAmazonSecretsManager client, SecretListEntry secret, string secretWithoutAppName, string environment, Dictionary<string, string> cacheToUse)
    {
        var request = new GetSecretValueRequest()
        {
            SecretId = secret.Name
        };

        var withoutEnvironment = secretWithoutAppName.TrimStart(environment).Replace('_', ':');
        var secretResponse = await client.GetSecretValueAsync(request);
        if (secretResponse.SecretString != null)
        {
            cacheToUse.Add(withoutEnvironment, secretResponse.SecretString);
        }
        else
        {
            StreamReader reader = new StreamReader(secretResponse.SecretBinary);
            string decodedBinarySecret = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(reader.ReadToEnd()));
            cacheToUse.Add(withoutEnvironment, decodedBinarySecret);
        }
    }
}
