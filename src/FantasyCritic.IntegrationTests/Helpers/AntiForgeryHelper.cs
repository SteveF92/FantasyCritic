using System;
using System.Text.RegularExpressions;

namespace FantasyCritic.IntegrationTests.Helpers;

internal static class AntiForgeryHelper
{
    // Matches both attribute orderings:
    //   <input name="__RequestVerificationToken" ... value="TOKEN" ...>
    //   <input ... value="TOKEN" ... name="__RequestVerificationToken" ...>
    private static readonly Regex TokenByValue = new(
        @"<input[^>]*name=""__RequestVerificationToken""[^>]*value=""([^""]+)""",
        RegexOptions.Compiled | RegexOptions.Singleline);

    private static readonly Regex TokenByName = new(
        @"<input[^>]*value=""([^""]+)""[^>]*name=""__RequestVerificationToken""",
        RegexOptions.Compiled | RegexOptions.Singleline);

    public static string ExtractToken(string html)
    {
        var m = TokenByValue.Match(html);
        if (m.Success) return m.Groups[1].Value;

        m = TokenByName.Match(html);
        if (m.Success) return m.Groups[1].Value;

        throw new InvalidOperationException(
            "Could not find __RequestVerificationToken in the page HTML. " +
            "Confirm the GET response was a Razor Page with antiforgery enabled, " +
            "not a redirect or error page.");
    }
}
