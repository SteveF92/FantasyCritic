namespace FantasyCritic.Discord.Interfaces;
public interface IUrlBuilder
{
    string UrlTemplate { get; }
    Dictionary<string, string> UrlTemplateKeywordMapping { get; init; }
    string BuildUrl(string displayText);
}
