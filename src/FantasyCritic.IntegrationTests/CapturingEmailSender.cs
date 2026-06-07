using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using FantasyCritic.Lib.Interfaces;

namespace FantasyCritic.IntegrationTests;

/// <summary>
/// Test-only <see cref="IEmailSender"/> that stores each email in memory so that
/// integration tests can extract the confirmation link and follow it.
/// Registered as a singleton in <see cref="FantasyCriticWebApplicationFactory"/>.
/// </summary>
public sealed class CapturingEmailSender : IEmailSender
{
    // Keyed by the *lowercase* recipient email address; stores the raw HTML body.
    private readonly ConcurrentDictionary<string, string> _bodies = new();

    public Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        _bodies[email.ToLowerInvariant()] = htmlMessage;
        return Task.CompletedTask;
    }

    /// <summary>
    /// Returns the relative path (e.g. <c>/Account/ConfirmEmail?userId=...&amp;code=...</c>)
    /// extracted from the most recent email sent to <paramref name="email"/>.
    /// Returns <c>null</c> if no email was captured for that address.
    /// </summary>
    public string? GetConfirmEmailPath(string email)
    {
        if (!_bodies.TryGetValue(email.ToLowerInvariant(), out var html))
            return null;

        // The confirmation link looks like:
        //   href="http://localhost/Account/ConfirmEmail?userId=...&code=..."
        // We only need the path-and-query portion so the test HttpClient can GET it.
        var match = Regex.Match(html, @"href=""([^""]*Account/ConfirmEmail[^""]*)""",
            RegexOptions.IgnoreCase);
        if (!match.Success)
            return null;

        // The href attribute value may have HTML-encoded ampersands (&amp; → &)
        var fullUrl = System.Net.WebUtility.HtmlDecode(match.Groups[1].Value);
        var uri = new System.Uri(fullUrl);
        return uri.PathAndQuery;
    }
}
