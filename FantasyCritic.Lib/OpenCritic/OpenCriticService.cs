using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;

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
            var result = await _client.GetStringAsync("/");
            return Maybe<OpenCriticGame>.None;
        }
    }
}
