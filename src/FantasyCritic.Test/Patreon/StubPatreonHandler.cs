using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FantasyCritic.Lib.Configuration;
using FantasyCritic.Lib.Patreon;

namespace FantasyCritic.Test.Patreon;

/// <summary>
/// Stands in for Patreon: answers each request from the supplied function and records what was sent, reading the body
/// while it still exists.
/// </summary>
public sealed class StubPatreonHandler : HttpMessageHandler
{
    private readonly Func<HttpRequestMessage, HttpResponseMessage> _respond;

    public StubPatreonHandler(Func<HttpRequestMessage, HttpResponseMessage> respond)
    {
        _respond = respond;
    }

    public List<SentRequest> Requests { get; } = [];

    public static PatreonOptions Options { get; } = new() { ClientId = "the-client", CampaignId = "12345", ClientSecret = "the-secret" };

    public PatreonApiClient CreateClient()
    {
        var client = new HttpClient(this) { BaseAddress = new Uri(PatreonApiClient.BaseAddress) };
        return new PatreonApiClient(client, Options);
    }

    public static HttpResponseMessage Json(string json, HttpStatusCode status = HttpStatusCode.OK)
    {
        return new HttpResponseMessage(status) { Content = new StringContent(json, Encoding.UTF8, "application/json") };
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var body = request.Content is null ? null : await request.Content.ReadAsStringAsync(cancellationToken);
        Requests.Add(new SentRequest(request.Method, request.RequestUri!, request.Headers.Authorization?.Parameter, body));
        return _respond(request);
    }

    public record SentRequest(HttpMethod Method, Uri Uri, string? BearerToken, string? Body);
}
