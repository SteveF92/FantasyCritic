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

namespace FantasyCritic.Lib.OpenCritic
{
    public class OpenCriticService : IOpenCriticService
    {
        private readonly HttpClient _client;

        public OpenCriticService(HttpClient client)
        {
            _client = client;
        }

        public async Task<Maybe<OpenCriticGame>> GetOpenCriticGame(int openCriticGameID)
        {
            var scoreResponse = await _client.GetStringAsync($"/api/game/score?id={openCriticGameID}");
            var parsedResult = JsonConvert.DeserializeObject<OpenCriticScoreResponse>(scoreResponse);
            if (parsedResult == null)
            {
                return Maybe<OpenCriticGame>.None;
            }

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

                DateTime releaseDateResult = releaseDateToken.Value<DateTime>();
                LocalDate releaseDate = LocalDate.FromDateTime(releaseDateResult);

                releaseDates.Add(releaseDate);
            }

            LocalDate? earliestReleaseDate = releaseDates.Min();

            var openCriticGame = new OpenCriticGame(parsedResult, earliestReleaseDate);
            return openCriticGame;
        }
    }
}
