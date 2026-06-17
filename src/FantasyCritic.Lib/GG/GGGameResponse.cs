using System.Text.Json.Serialization;

namespace FantasyCritic.Lib.GG;

public class GGGameResponse
{
    public string? CoverPath { get; set; }
    public int? Id { get; set; }
    public string? Name { get; set; }
    public string? Slug { get; set; }
    public string? Token { get; set; }
}

public class GGGraphQLResponse
{
    public GGGraphQLData? Data { get; set; }
}

public class GGGraphQLData
{
    [JsonPropertyName("getGameByToken")]
    public GGGameResponse? GetGameByToken { get; set; }
}
