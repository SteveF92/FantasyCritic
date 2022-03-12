using System.Net.Http;
using System.Text;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FantasyCritic.Lib.GG;

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

            string responseString;
            using (var response = await _client.SendAsync(request))
            {
                response.EnsureSuccessStatusCode();
                responseString = await response.Content.ReadAsStringAsync();
            }

            JObject parsedGameResponse = JObject.Parse(responseString);
            var dataSectionToken = parsedGameResponse.GetValue("data");
            if (dataSectionToken is not JObject dataSection)
            {
                return Maybe<GGGame>.None;
            }
            var internalSectionToken = dataSection.GetValue("getGameByToken");
            if (internalSectionToken is not JObject internalSection)
            {
                return Maybe<GGGame>.None;
            }

            var typedData = internalSection.ToObject<GGGameResponse>();
            if (typedData is null)
            {
                return Maybe<GGGame>.None;
            }

            Maybe<string> coverPath = Maybe<string>.None;
            if (!string.IsNullOrWhiteSpace(typedData.CoverPath))
            {
                var split = typedData.CoverPath.Split("/");
                coverPath = split.Last();
            }

            return new GGGame(typedData.Token, coverPath);
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
