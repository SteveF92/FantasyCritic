using System.Net.Http;
using Newtonsoft.Json.Linq;
using Microsoft.Extensions.Logging;

namespace FantasyCritic.Lib.OpenCritic;

public class OpenCriticService : IOpenCriticService
{
    private readonly HttpClient _client;
    private readonly ILogger<OpenCriticService> _logger;
    private readonly LocalDate DefaultOpenCriticReleaseDate = new LocalDate(2020, 12, 31);

    public OpenCriticService(HttpClient client, ILogger<OpenCriticService> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<OpenCriticGame?> GetOpenCriticGame(int openCriticGameID)
    {
        try
        {
            var gameResponse = await _client.GetStringAsync($"game/{openCriticGameID}");
            if (string.IsNullOrWhiteSpace(gameResponse))
            {
                return null;
            }
            JObject parsedGameResponse = JObject.Parse(gameResponse);

            var gameName = parsedGameResponse.GetValue("name").Value<string>();
            LocalDate? earliestReleaseDate = null;
            var releaseDateToken = parsedGameResponse.GetValue("firstReleaseDate");
            var firstReleaseDateString = releaseDateToken.Value<string>();
            if (!string.IsNullOrWhiteSpace(firstReleaseDateString))
            {
                var earliestDateTime = releaseDateToken.Value<DateTime>();
                earliestReleaseDate = LocalDate.FromDateTime(earliestDateTime);
                if (earliestReleaseDate == DefaultOpenCriticReleaseDate)
                {
                    earliestReleaseDate = null;
                }
            }

            var score = parsedGameResponse.GetValue("topCriticScore").Value<decimal?>();
            if (score == -1m)
            {
                score = parsedGameResponse.GetValue("averageScore").Value<decimal?>();
                if (score != -1m)
                {
                    _logger.LogWarning($"Using averageScore for game: {openCriticGameID}");
                }
                else
                {
                    score = null;
                }
            }

            var openCriticGame = new OpenCriticGame(openCriticGameID, gameName, score, earliestReleaseDate);
            return openCriticGame;
        }
        catch (HttpRequestException httpEx)
        {
            if (httpEx.Message == "Response status code does not indicate success: 404 (Not Found).")
            {
                return null;
            }
            _logger.LogError(httpEx, $"Getting an open critic game failed: {openCriticGameID}");
            throw;
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"Getting an open critic game failed: {openCriticGameID}");
            throw;
        }
    }
}
