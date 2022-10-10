namespace FantasyCritic.Discord.UrlBuilders;
public class PublisherUrlBuilder : UrlBuilder
{
    private const string BaseAddressKeyword = "{{BASEADDRESS}}";
    private const string PublisherIdKeyword = "{{LEAGUEID}}";

    public PublisherUrlBuilder(string baseAddress, Guid publisherId)
    {
        UrlTemplate = $"{BaseAddressKeyword}/publisher/{PublisherIdKeyword}";

        UrlTemplateKeywordMapping.Add(BaseAddressKeyword, baseAddress);
        UrlTemplateKeywordMapping.Add(PublisherIdKeyword, publisherId.ToString());
    }
}
