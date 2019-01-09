using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using NLog;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NodaTime;
using NodaTime.Text;
using Microsoft.Extensions.Logging;

namespace FantasyCritic.Lib.OpenCritic
{
    public class OpenCriticService : IOpenCriticService
    {
        private readonly HttpClient _client;
        private readonly ILogger<OpenCriticService> _logger;

        public OpenCriticService(HttpClient client, ILogger<OpenCriticService> logger)
        {
            _client = client;
            _logger = logger;
        }

        public async Task<Maybe<OpenCriticGame>> GetOpenCriticGame(int openCriticGameID)
        {
            try
            {
                var gameResponse = await _client.GetStringAsync($"/api/game?id={openCriticGameID}");
                JObject parsedGameResponse = JObject.Parse(gameResponse);

                List<LocalDate> releaseDates = new List<LocalDate>();
                var platforms = parsedGameResponse.GetValue("Platforms");
                foreach (var platform in platforms.Children())
                {
                    var gamePlatforms = platform.SelectToken("GamesPlatforms");
                    var releaseDateToken = gamePlatforms.SelectToken("releaseDate");
                    if (releaseDateToken == null)
                    {
                        continue;
                    }

                    string releaseDateString = releaseDateToken.Value<string>();
                    if (string.IsNullOrWhiteSpace(releaseDateString))
                    {
                        continue;
                    }

                    DateTime releaseDateResult = releaseDateToken.Value<DateTime>();
                    LocalDate releaseDate = LocalDate.FromDateTime(releaseDateResult);

                    releaseDates.Add(releaseDate);
                }

                LocalDate? earliestReleaseDate = null;
                if (releaseDates.Any())
                {
                    earliestReleaseDate = releaseDates.Min();
                }
                 
                var scoreResponse = await _client.GetStringAsync($"/api/game/score?id={openCriticGameID}");
                var parsedResult = JsonConvert.DeserializeObject<OpenCriticScoreResponse>(scoreResponse);
                if (parsedResult == null)
                {
                    return Maybe<OpenCriticGame>.None;
                }

                var openCriticGame = new OpenCriticGame(parsedResult, earliestReleaseDate);
                return openCriticGame;
            }
            catch (HttpRequestException httpEx)
            {
                if (httpEx.Message == "Response status code does not indicate success: 404 (Not Found).")
                {
                    return Maybe<OpenCriticGame>.None;
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
}
