using FantasyCritic.Discord.Interfaces;

namespace FantasyCritic.Discord.UrlBuilders;
public abstract class UrlBuilder : IUrlBuilder
{
    public string UrlTemplate { get; init; } = "";
    public Dictionary<string, string> UrlTemplateKeywordMapping { get; init; } = new();

    public string BuildUrl(string displayText = "")
    {
        var url = UrlTemplate;
        foreach (var mapping in UrlTemplateKeywordMapping)
        {
            url = url.Replace(mapping.Key, mapping.Value);
        }
        return !string.IsNullOrEmpty(displayText) ? $"[{displayText}]({url})" : url;
    }
}
