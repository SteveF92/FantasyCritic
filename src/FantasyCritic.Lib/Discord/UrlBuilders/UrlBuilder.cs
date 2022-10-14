using FantasyCritic.Lib.Discord.Interfaces;

namespace FantasyCritic.Lib.Discord.UrlBuilders;
public abstract class UrlBuilder : IUrlBuilder
{
    protected string UrlTemplate { get; init; } = "";
    protected Dictionary<string, string> UrlTemplateKeywordMapping { get; init; } = new();

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
