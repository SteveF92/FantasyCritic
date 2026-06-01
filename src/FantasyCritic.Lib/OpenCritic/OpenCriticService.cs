using FantasyCritic.Lib.Extensions;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Net.Http.Json;

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
            var response = await _client.GetFromJsonAsync<OpenCriticGameResponse>($"game/{openCriticGameID}");
            if (response is null)
            {
                return null;
            }

            var gameName = response.Name ?? "Unknown Open Critic Game";

            LocalDate? earliestReleaseDate = null;
            if (!string.IsNullOrWhiteSpace(response.FirstReleaseDate) &&
                DateTime.TryParse(response.FirstReleaseDate, out var parsedDate))
            {
                earliestReleaseDate = LocalDate.FromDateTime(parsedDate);
                if (earliestReleaseDate == DefaultOpenCriticReleaseDate)
                {
                    earliestReleaseDate = null;
                }
            }

            var score = response.TopCriticScore;
            if (score == -1m)
            {
                score = response.AverageScore;
                if (score != -1m)
                {
                    _logger.LogInformation($"Using averageScore for game: {openCriticGameID}");
                }
                else
                {
                    score = null;
                }
            }

            var numReviews = response.NumReviews;
            bool hasAnyReviews = numReviews.HasValue && numReviews > 0;
            string? slug = response.Url?.SubstringStartingFromLastInstanceOf("/");

            var openCriticGame = new OpenCriticGame(openCriticGameID, gameName, score, earliestReleaseDate, hasAnyReviews, slug);
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
