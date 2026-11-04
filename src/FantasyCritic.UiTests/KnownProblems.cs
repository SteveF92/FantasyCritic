using System.Linq;
using System.Text.RegularExpressions;

namespace FantasyCritic.UiTests;

/// <summary>
/// Problems that main already has, outside the client app's migration, so they don't fail tests. Each still shows in
/// console-warnings.md. Keep this list short, and take an entry out once its cause is fixed.
/// </summary>
public static class KnownProblems
{
    private static readonly (Regex Pattern, string Reason)[] Problems =
    [
        (new Regex("^404 from GET /lib/jquery-validation"),
            "in Development the Razor pages load jquery-validation from /lib, which isn't in the repository; production loads it from a CDN"),
        (new Regex("^Console error on /games: AxiosError: Request failed with status code 401.*fetchMyLeaguesForYear", RegexOptions.Singleline),
            "Master Games asks for the user's leagues when nobody is logged in"),
        (new Regex(@"^Console error on /league/.*TypeError: Cannot read properties of undefined \(reading 'autoDraftMode'\)"),
            "draftStatusHeader's userHasAutoDraft is true when the user has no publisher, so its template reads userPublisher.autoDraftMode"),
    ];

    public static string? Explain(string problem) => Problems.FirstOrDefault(known => known.Pattern.IsMatch(problem)).Reason;
}
