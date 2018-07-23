using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using NLog;
using Newtonsoft.Json;

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
            var response = await _client.GetStringAsync($"/api/game/score?id={openCriticGameID}");
            var parsedResult = JsonConvert.DeserializeObject<OpenCriticScoreResponse>(response);
            if (parsedResult == null)
            {
                return Maybe<OpenCriticGame>.None;
            }

            var openCriticGame = new OpenCriticGame(parsedResult);
            return openCriticGame;
        }
    }
}
