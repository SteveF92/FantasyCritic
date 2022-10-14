namespace FantasyCritic.Lib.Discord.UrlBuilders;
public class LeagueUrlBuilder : UrlBuilder
{
    private const string BaseAddressKeyword = "{{BASEADDRESS}}";
    private const string LeagueIdKeyword = "{{LEAGUEID}}";
    private const string YearKeyword = "{{YEAR}}";

    public LeagueUrlBuilder(string baseAddress, Guid leagueId, int year)
    {
        UrlTemplate = $"{BaseAddressKeyword}/league/{LeagueIdKeyword}/{YearKeyword}";

        UrlTemplateKeywordMapping.Add(BaseAddressKeyword, baseAddress);
        UrlTemplateKeywordMapping.Add(LeagueIdKeyword, leagueId.ToString());
        UrlTemplateKeywordMapping.Add(YearKeyword, year.ToString());
    }
}
