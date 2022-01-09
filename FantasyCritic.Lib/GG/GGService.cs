using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using FantasyCritic.Lib.OpenCritic;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NodaTime;

namespace FantasyCritic.Lib.GG
{
    public class GGService : IGGService
    {
        private readonly HttpClient _client;
        private readonly ILogger<GGService> _logger;

        public GGService(HttpClient client, ILogger<GGService> logger)
        {
            _client = client;
            _logger = logger;
        }

        public async Task<Maybe<GGGame>> GetGGGame(string ggToken)
        {
            try
            {
                string query = @"query getGameByToken($token: String!) {
                                    getGameByToken(token: $token) {
                                        id
                                        name
                                        coverPath
                                        token
                                        slug
                                    }
                                }";
                var queryObject = new
                {
                    query,
                    variables = new
                    {
                        token = ggToken
                    }
                };

                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Post,
                    Content = new StringContent(JsonConvert.SerializeObject(queryObject), Encoding.UTF8, "application/json")
                };

                dynamic responseObj;

                using (var response = await _client.SendAsync(request))
                {
                    response.EnsureSuccessStatusCode();

                    var responseString = await response.Content.ReadAsStringAsync();
                    responseObj = JsonConvert.DeserializeObject<dynamic>(responseString);
                }

                return Maybe<GGGame>.None;
            }
            catch (HttpRequestException httpEx)
            {
                if (httpEx.Message == "Response status code does not indicate success: 404 (Not Found).")
                {
                    return Maybe<GGGame>.None;
                }
                _logger.LogError(httpEx, $"Getting an GG| game failed: {ggToken}");
                throw;
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Getting an GG| game failed: {ggToken}");
                throw;
            }
        }
    }
}
