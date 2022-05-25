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
using Filter = Amazon.SecretsManager.Model.Filter;

namespace FantasyCritic.AWS;
public class SecretsManagerConfigurationStore : IConfigurationStore
{
    private readonly string _region;
    private readonly string _secretsManagerPrefix;
    private readonly Dictionary<string, string> _awsSecretsCache;

    public SecretsManagerConfigurationStore(string region, string secretsManagerPrefix)
    {
        _region = region;
        _secretsManagerPrefix = secretsManagerPrefix;
        _awsSecretsCache = new Dictionary<string, string>();
    }

    public string GetConfigValue(string name)
    {
        if (!_awsSecretsCache.TryGetValue(name, out var value))
        {
            throw new Exception($"Config value: {name} not found.");
        }

        if (string.IsNullOrWhiteSpace(value))
        {
            throw new Exception($"Config value: {name} not found.");
        }

        return value;
    }

    public string GetConnectionString(string name)
    {
        var rawValue = GetConfigValue(name);
        var connectionStringObject = JsonConvert.DeserializeObject<ConnectionStringEntity>(rawValue);
        if (connectionStringObject is null)
        {
            throw new Exception($"Cannot parse connection string: {name}");
        }

        return $"Server={connectionStringObject.Host};Database=fantasycritic;Uid={connectionStringObject.Username};Pwd={connectionStringObject.Password};charset=utf8;sslMode=required;MaximumPoolsize=50;";
    }

    public string GetAWSRegion() => _region;

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
                            _secretsManagerPrefix
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
            var request = new GetSecretValueRequest()
            {
                SecretId = secret.Name
            };

            var secretNameReplaced = secret.Name.TrimStart(_secretsManagerPrefix).Replace('_', ':');
            var secretResponse = await client.GetSecretValueAsync(request);
            if (secretResponse.SecretString != null)
            {
                _awsSecretsCache.Add(secretNameReplaced, secretResponse.SecretString);
            }
            else
            {
                StreamReader reader = new StreamReader(secretResponse.SecretBinary);
                string decodedBinarySecret = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(reader.ReadToEnd()));
                _awsSecretsCache.Add(secretNameReplaced, decodedBinarySecret);
            }
        }
    }
}
