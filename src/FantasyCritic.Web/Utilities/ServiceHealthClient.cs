using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using FantasyCritic.Lib;
using FantasyCritic.Lib.SharedSerialization.API;

namespace FantasyCritic.Web.Utilities;

public record ServiceHealthConfiguration(string WorkerUrl, string DiscordBotUrl);

/// <summary>
/// Asks the worker and the Discord bot for their own GET /health, for the admin monitor.
/// </summary>
public class ServiceHealthClient
{
    public const string UnreachableStatus = "Unreachable";
    public const string UnhealthyStatus = "Unhealthy";

    private readonly HttpClient _httpClient;
    private readonly ServiceHealthConfiguration _configuration;

    public ServiceHealthClient(HttpClient httpClient, ServiceHealthConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
    }

    public Task<ServiceHealthReport> GetWorkerHealth() => GetHealth(_configuration.WorkerUrl);
    public Task<ServiceHealthReport> GetDiscordBotHealth() => GetHealth(_configuration.DiscordBotUrl);

    private async Task<ServiceHealthReport> GetHealth(string serviceUrl)
    {
        //A service that cannot be reached is the thing this exists to report, not a failure of the request,
        //so these are caught and turned into a status rather than left to become a 500.
        try
        {
            //An unhealthy service answers 503 with the same body, so the status code is deliberately not checked.
            using var response = await _httpClient.GetAsync($"{serviceUrl.TrimEnd('/')}/health");
            var report = await response.Content.ReadFromJsonAsync<ServiceHealthReport>(FantasyCriticJsonOptions.Default);
            return report ?? Unreachable("The health endpoint returned an empty response.");
        }
        catch (HttpRequestException ex)
        {
            return Unreachable(ex.Message);
        }
        catch (TaskCanceledException)
        {
            return Unreachable($"No answer within {_httpClient.Timeout.TotalSeconds} seconds.");
        }
        catch (JsonException ex)
        {
            return Unreachable($"The health endpoint did not return a health report: {ex.Message}");
        }
    }

    private static ServiceHealthReport Unreachable(string description)
    {
        return new ServiceHealthReport(UnreachableStatus, description, new Dictionary<string, string>());
    }
}
