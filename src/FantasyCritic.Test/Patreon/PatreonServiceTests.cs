using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FantasyCritic.Lib.Interfaces;
using FantasyCritic.Lib.Patreon;
using NUnit.Framework;

namespace FantasyCritic.Test.Patreon;

/// <summary>
/// The token flow around the API client: use the stored access token, and when Patreon rejects it, refresh, save the new
/// pair, and try once more. UserIsPlusUser is the way in because it needs no Fantasy Critic users.
/// </summary>
[TestFixture]
public class PatreonServiceTests
{
    private const string PlusMember = """
        {
          "data": [
            { "id": "member-1", "type": "member", "attributes": {},
              "relationships": {
                "currently_entitled_tiers": { "data": [ { "id": "tier-plus", "type": "tier" } ] },
                "user": { "data": { "id": "user-1", "type": "user" } } } }
          ],
          "included": [ { "id": "tier-plus", "type": "tier", "attributes": { "title": "Fantasy Critic Plus" } } ]
        }
        """;

    private const string DonorMember = """
        {
          "data": [
            { "id": "member-1", "type": "member", "attributes": {},
              "relationships": {
                "currently_entitled_tiers": { "data": [ { "id": "tier-donor", "type": "tier" } ] },
                "user": { "data": { "id": "user-1", "type": "user" } } } }
          ],
          "included": [ { "id": "tier-donor", "type": "tier", "attributes": { "title": "Fantasy Critic Donor" } } ]
        }
        """;

    private const string RefreshedTokens = """{ "access_token": "new-access", "refresh_token": "new-refresh", "expires_in": 2678400 }""";

    private static HttpResponseMessage Unauthorized() => StubPatreonHandler.Json("""{ "errors": [] }""", HttpStatusCode.Unauthorized);

    private static PatreonService CreateService(StubPatreonHandler handler, FakePatreonTokensRepo tokens)
    {
        // UserIsPlusUser never touches the user store.
        return new PatreonService(handler.CreateClient(), tokens, null!);
    }

    [Test]
    public async Task UsesTheStoredAccessToken_AndLeavesTheTokensAlone_WhenPatreonAcceptsIt()
    {
        var tokens = new FakePatreonTokensRepo(new PatreonTokens("stored-access", "stored-refresh"));
        var handler = new StubPatreonHandler(_ => StubPatreonHandler.Json(PlusMember));

        var isPlusUser = await CreateService(handler, tokens).UserIsPlusUser("user-1");

        using (Assert.EnterMultipleScope())
        {
            Assert.That(isPlusUser, Is.True);
            Assert.That(handler.Requests.Single().BearerToken, Is.EqualTo("stored-access"));
            Assert.That(tokens.Saved, Is.Empty);
        }
    }

    [Test]
    public async Task UserIsPlusUser_CountsTheDonorTier_WhichIncludesPlus()
    {
        var tokens = new FakePatreonTokensRepo(new PatreonTokens("stored-access", "stored-refresh"));
        var handler = new StubPatreonHandler(_ => StubPatreonHandler.Json(DonorMember));
        var service = CreateService(handler, tokens);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(await service.UserIsPlusUser("user-1"), Is.True);
            Assert.That(await service.UserIsPlusUser("someone-else"), Is.False);
        }
    }

    [Test]
    public async Task RefreshesSavesAndRetries_WhenPatreonRejectsTheAccessToken()
    {
        var tokens = new FakePatreonTokensRepo(new PatreonTokens("expired-access", "stored-refresh"));
        var handler = new StubPatreonHandler(request =>
        {
            if (request.Method == HttpMethod.Post)
            {
                return StubPatreonHandler.Json(RefreshedTokens);
            }

            return request.Headers.Authorization!.Parameter == "new-access" ? StubPatreonHandler.Json(PlusMember) : Unauthorized();
        });

        var isPlusUser = await CreateService(handler, tokens).UserIsPlusUser("user-1");

        using (Assert.EnterMultipleScope())
        {
            Assert.That(isPlusUser, Is.True);
            Assert.That(handler.Requests.Select(x => x.Method), Is.EqualTo(new[] { HttpMethod.Get, HttpMethod.Post, HttpMethod.Get }));
            Assert.That(handler.Requests[1].Body, Does.Contain("refresh_token=stored-refresh"));
            Assert.That(tokens.Saved, Is.EqualTo(new[] { new PatreonTokens("new-access", "new-refresh") }));
        }
    }

    [Test]
    public void Throws_WhenPatreonRejectsEvenTheRefreshedAccessToken_HavingSavedTheNewPair()
    {
        var tokens = new FakePatreonTokensRepo(new PatreonTokens("expired-access", "stored-refresh"));
        var handler = new StubPatreonHandler(request => request.Method == HttpMethod.Post ? StubPatreonHandler.Json(RefreshedTokens) : Unauthorized());

        var exception = Assert.ThrowsAsync<InvalidOperationException>(() => CreateService(handler, tokens).UserIsPlusUser("user-1"));

        using (Assert.EnterMultipleScope())
        {
            Assert.That(exception!.Message, Does.Contain("freshly refreshed"));
            Assert.That(tokens.Saved, Has.Count.EqualTo(1), "the old refresh token is spent, so the new pair must be kept");
        }
    }

    [Test]
    public void Throws_WithoutSavingAnything_WhenPatreonRefusesTheRefresh()
    {
        var tokens = new FakePatreonTokensRepo(new PatreonTokens("expired-access", "spent-refresh"));
        var handler = new StubPatreonHandler(request => request.Method == HttpMethod.Post
            ? StubPatreonHandler.Json("""{ "error": "invalid_grant" }""", HttpStatusCode.BadRequest)
            : Unauthorized());

        var exception = Assert.ThrowsAsync<HttpRequestException>(() => CreateService(handler, tokens).UserIsPlusUser("user-1"));

        using (Assert.EnterMultipleScope())
        {
            Assert.That(exception!.Message, Does.Contain("invalid_grant"));
            Assert.That(tokens.Saved, Is.Empty);
        }
    }

    private sealed class FakePatreonTokensRepo : IPatreonTokensRepo
    {
        private readonly PatreonTokens _stored;

        public FakePatreonTokensRepo(PatreonTokens stored)
        {
            _stored = stored;
        }

        public List<PatreonTokens> Saved { get; } = [];

        public Task<PatreonTokens> GetMostRecentTokens() => Task.FromResult(Saved.LastOrDefault() ?? _stored);

        public Task SaveTokens(PatreonTokens keys)
        {
            Saved.Add(keys);
            return Task.CompletedTask;
        }
    }
}
