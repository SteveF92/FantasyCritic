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
    ];

    public static string? Explain(string problem) => Problems.FirstOrDefault(known => known.Pattern.IsMatch(problem)).Reason;
}
