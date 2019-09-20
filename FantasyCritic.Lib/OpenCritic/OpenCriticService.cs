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
                var gameResponse = await _client.GetStringAsync($"game/{openCriticGameID}");
                JObject parsedGameResponse = JObject.Parse(gameResponse);

                LocalDate? earliestReleaseDate = null;
                var firstReleaseDateString = parsedGameResponse.GetValue("firstReleaseDate").Value<string>();
                if (!string.IsNullOrWhiteSpace(firstReleaseDateString))
                {
                    var earliestDateTime = parsedGameResponse.GetValue("firstReleaseDate").Value<DateTime>();
                    earliestReleaseDate = LocalDate.FromDateTime(earliestDateTime);
                }

                var gameName = parsedGameResponse.GetValue("name").Value<string>();
                var score = parsedGameResponse.GetValue("topCriticScore").Value<decimal?>();

                var openCriticGame = new OpenCriticGame(openCriticGameID, gameName, score, earliestReleaseDate);
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
