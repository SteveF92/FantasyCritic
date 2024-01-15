namespace FantasyCritic.Lib.Discord.UrlBuilders;
public class ConferenceUrlBuilder : UrlBuilder
{
    private const string BaseAddressKeyword = "{{BASEADDRESS}}";
    private const string ConferenceIDKeyword = "{{CONFERENCEID}}";
    private const string YearKeyword = "{{YEAR}}";

    public ConferenceUrlBuilder(string baseAddress, Guid conferenceID, int year)
    {
        UrlTemplate = $"{BaseAddressKeyword}/conference/{ConferenceIDKeyword}/{YearKeyword}";

        UrlTemplateKeywordMapping.Add(BaseAddressKeyword, baseAddress);
        UrlTemplateKeywordMapping.Add(ConferenceIDKeyword, conferenceID.ToString());
        UrlTemplateKeywordMapping.Add(YearKeyword, year.ToString());
    }
}
