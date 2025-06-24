using FantasyCritic.Lib.Discord.Interfaces;

namespace FantasyCritic.Lib.Discord.UrlBuilders;
public abstract class UrlBuilder : IUrlBuilder
{
    protected string UrlTemplate { get; init; } = "";
    protected Dictionary<string, string> UrlTemplateKeywordMapping { get; init; } = new();
    private string _url = "";

    public string BuildUrl(string displayText = "", bool hidePreview = false)
    {
        BuildUrlWithTemplate();
        if (hidePreview)
        {
            _url = $"<{_url}>";
        }
        return !string.IsNullOrEmpty(displayText) ? $"[{displayText}]({_url})" : _url;
    }

    private void BuildUrlWithTemplate()
    {
        _url = UrlTemplate;
        foreach (var mapping in UrlTemplateKeywordMapping)
        {
            _url = _url.Replace(mapping.Key, mapping.Value);
        }
    }

    public string GetOnlyUrl()
    {
        BuildUrlWithTemplate();
        return _url;
    }
}
