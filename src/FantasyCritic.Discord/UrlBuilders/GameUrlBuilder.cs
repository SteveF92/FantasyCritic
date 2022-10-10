namespace FantasyCritic.Discord.UrlBuilders;
public class GameUrlBuilder : UrlBuilder
{
    private const string BaseAddressKeyword = "{{BASEADDRESS}}";
    private const string GameIdKeyword = "{{MASTERGAMEID}}";

    public GameUrlBuilder(string baseAddress, Guid masterGameId)
    {
        UrlTemplate = $"{BaseAddressKeyword}/mastergame/{GameIdKeyword}";

        UrlTemplateKeywordMapping.Add(BaseAddressKeyword, baseAddress);
        UrlTemplateKeywordMapping.Add(GameIdKeyword, masterGameId.ToString());
    }
}
