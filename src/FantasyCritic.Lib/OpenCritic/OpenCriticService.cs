using System.Net;
using System.Net.Http;
using System.Text.Json;
using FantasyCritic.Lib.Extensions;
using Microsoft.Extensions.Logging;

namespace FantasyCritic.Lib.OpenCritic;

public class OpenCriticService : IOpenCriticService
{
    private readonly HttpClient _client;
    private readonly ILogger<OpenCriticService> _logger;

    public OpenCriticService(HttpClient client, ILogger<OpenCriticService> logger)
    {
        _client = client;
        _logger = logger;
    }

    public Task<OpenCriticGame?> GetOpenCriticGame(MasterGame masterGame, CancellationToken cancellationToken)
    {
        var openCriticGameID = masterGame.OpenCriticID;
        if (!openCriticGameID.HasValue)
        {
            return Task.FromResult<OpenCriticGame?>(null);
        }

        return GetOpenCriticGame(openCriticGameID.Value, masterGame.GameName, cancellationToken);
    }

    public Task<OpenCriticGame?> GetOpenCriticGame(MasterSubGame masterGame, CancellationToken cancellationToken)
    {
        var openCriticGameID = masterGame.OpenCriticID;
        if (!openCriticGameID.HasValue)
        {
            return Task.FromResult<OpenCriticGame?>(null);
        }

        return GetOpenCriticGame(openCriticGameID.Value, masterGame.GameName, cancellationToken);
    }

    private async Task<OpenCriticGame?> GetOpenCriticGame(int openCriticGameID, string masterGameName, CancellationToken cancellationToken)
    {
        try
        {
            using var httpResponse = await _client.GetAsync($"game/{openCriticGameID}", cancellationToken);
            if (httpResponse.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }

            httpResponse.EnsureSuccessStatusCode();
            var body = await httpResponse.Content.ReadAsStringAsync(cancellationToken);
            var headers = string.Join("; ", httpResponse.Headers.Concat(httpResponse.Content.Headers)
                .Select(header => $"{header.Key}: {string.Join(", ", header.Value)}"));
            _logger.LogInformation("OpenCritic response for game {GameName}:{OpenCriticGameID}. Headers: {Headers} Body: {Body}", masterGameName, openCriticGameID, headers, body);

            var response = JsonSerializer.Deserialize<OpenCriticGameResponse>(body, JsonSerializerOptions.Web);
            if (response is null)
            {
                return null;
            }

            var gameName = response.Name ?? "Unknown Open Critic Game";

            var score = response.TopCriticScore;
            if (score == -1m)
            {
                score = response.AverageScore;
                if (score != -1m)
                {
                    _logger.LogInformation("Using averageScore for game: {GameName}:{OpenCriticGameID}", masterGameName, openCriticGameID);
                }
                else
                {
                    score = null;
                }
            }

            var numReviews = response.NumReviews;
            bool hasAnyReviews = numReviews.HasValue && numReviews > 0;
            string? slug = response.Url?.SubstringStartingFromLastInstanceOf("/");

            var openCriticGame = new OpenCriticGame(openCriticGameID, gameName, score, hasAnyReviews, slug);
            return openCriticGame;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Getting an open critic game failed: {GameName}:{OpenCriticGameID}", masterGameName, openCriticGameID);
            throw;
        }
    }
}
