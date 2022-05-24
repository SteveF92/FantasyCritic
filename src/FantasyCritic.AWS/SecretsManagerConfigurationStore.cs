using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon;
using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using FantasyCritic.Lib.DependencyInjection;

namespace FantasyCritic.AWS;
public class SecretsManagerConfigurationStore : IConfigurationStore
{
    private readonly string _region;
    private readonly string _secretsManagerPrefix;
    private readonly Dictionary<string, string> _parameterCache;

    public SecretsManagerConfigurationStore(string region, string secretsManagerPrefix)
    {
        _region = region;
        _secretsManagerPrefix = secretsManagerPrefix;
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
    }
}
