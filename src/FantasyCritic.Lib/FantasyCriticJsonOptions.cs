using System.Text.Json;
using NodaTime.Serialization.SystemTextJson;

namespace FantasyCritic.Lib;

public static class FantasyCriticJsonOptions
{
    public static readonly JsonSerializerOptions Default = CreateDefault();
    public static readonly JsonSerializerOptions Indented = CreateIndented();

    private static JsonSerializerOptions CreateDefault() =>
        new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }
            .ConfigureForNodaTime(DateTimeZoneProviders.Tzdb);

    private static JsonSerializerOptions CreateIndented() =>
        new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, WriteIndented = true }
            .ConfigureForNodaTime(DateTimeZoneProviders.Tzdb);
}
