namespace FantasyCritic.Lib.SharedSerialization.API;

/// <summary>
/// One line of a service's health report, labelled for a person to read. Exactly one of Text and Time is set. A time
/// is sent as a time rather than as text so the admin console can show it in the viewer's own time zone.
/// </summary>
public record ServiceHealthDetail(string Label, string? Text, Instant? Time)
{
    public static ServiceHealthDetail FromText(string label, string text) => new(label, text, null);
    public static ServiceHealthDetail FromTime(string label, Instant time) => new(label, null, time);
}
