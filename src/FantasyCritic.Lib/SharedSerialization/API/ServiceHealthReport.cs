namespace FantasyCritic.Lib.SharedSerialization.API;

/// <summary>
/// The body of GET /health on the worker and the Discord bot. Written by those processes and read by the web app's admin monitor.
/// </summary>
public record ServiceHealthReport(string Status, string? Description, IReadOnlyDictionary<string, string> Data);
