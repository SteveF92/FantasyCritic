using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using FantasyCritic.Lib.Patreon;
using NUnit.Framework;

namespace FantasyCritic.Test.Patreon;

[TestFixture]
public class PatreonApiClientTests
{
    private const string NextPage = "https://www.patreon.com/api/oauth2/v2/campaigns/12345/members?page%5Bcursor%5D=page-two";

    // Two members: one on the Plus tier, and one entitled to nothing, whose tier list Patreon sends empty.
    private const string FirstPage = $$"""
        {
          "data": [
            { "id": "member-1", "type": "member", "attributes": {},
              "relationships": {
                "currently_entitled_tiers": { "data": [ { "id": "tier-plus", "type": "tier" } ] },
                "user": { "data": { "id": "user-1", "type": "user" } } } },
            { "id": "member-2", "type": "member", "attributes": {},
              "relationships": {
                "currently_entitled_tiers": { "data": [] },
                "user": { "data": { "id": "user-2", "type": "user" } } } }
          ],
          "included": [
            { "id": "tier-plus", "type": "tier", "attributes": { "title": "Fantasy Critic Plus" } },
            { "id": "user-1", "type": "user", "attributes": { "full_name": "Plus Person" } },
            { "id": "user-2", "type": "user", "attributes": { "full_name": "Lapsed Person" } }
          ],
          "links": { "next": "{{NextPage}}" },
          "meta": { "pagination": { "total": 4 } }
        }
        """;

    // The last page: a Donor, and a member whose user is gone, which nobody can be linked to. No next link.
    private const string SecondPage = """
        {
          "data": [
            { "id": "member-3", "type": "member", "attributes": {},
              "relationships": {
                "currently_entitled_tiers": { "data": [ { "id": "tier-donor", "type": "tier" } ] },
                "user": { "data": { "id": "user-3", "type": "user" } } } },
            { "id": "member-4", "type": "member", "attributes": {},
              "relationships": {
                "currently_entitled_tiers": { "data": [] },
                "user": { "data": null } } }
          ],
          "included": [
            { "id": "tier-donor", "type": "tier", "attributes": { "title": "Fantasy Critic Donor" } },
            { "id": "user-3", "type": "user", "attributes": { "full_name": "Donor Person" } }
          ],
          "meta": { "pagination": { "total": 4 } }
        }
        """;

    [Test]
    public async Task GetCampaignMembers_FollowsNextLinks_AndJoinsEachMemberToItsUserAndTiers()
    {
        var handler = new StubPatreonHandler(request => StubPatreonHandler.Json(request.RequestUri!.Query.Contains("page-two") ? SecondPage : FirstPage));

        var result = await handler.CreateClient().GetCampaignMembers("the-access-token");

        Assert.That(result.IsSuccess, Is.True);
        var members = result.Value;
        using (Assert.EnterMultipleScope())
        {
            Assert.That(members.Select(x => x.UserId), Is.EqualTo(new[] { "user-1", "user-2", "user-3" }));
            Assert.That(members[0].FullName, Is.EqualTo("Plus Person"));
            Assert.That(members[0].TierTitles, Is.EqualTo(new[] { "Fantasy Critic Plus" }));
            Assert.That(members[1].TierTitles, Is.Empty);
            Assert.That(members[2].FullName, Is.EqualTo("Donor Person"));
            Assert.That(members[2].TierTitles, Is.EqualTo(new[] { "Fantasy Critic Donor" }));
            Assert.That(handler.Requests.Select(x => x.BearerToken), Is.All.EqualTo("the-access-token"));
            Assert.That(handler.Requests[1].Uri.AbsoluteUri, Is.EqualTo(NextPage));
        }
    }

    [Test]
    public async Task GetCampaignMembers_AsksForTheCampaignsMembersWithTheirTiersAndUsers()
    {
        var handler = new StubPatreonHandler(_ => StubPatreonHandler.Json(SecondPage));

        await handler.CreateClient().GetCampaignMembers("the-access-token");

        var uri = handler.Requests.Single().Uri;
        var query = HttpUtility.ParseQueryString(uri.Query);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(uri.GetLeftPart(UriPartial.Path), Is.EqualTo("https://www.patreon.com/api/oauth2/v2/campaigns/12345/members"));
            Assert.That(query["include"], Is.EqualTo("currently_entitled_tiers,user"));
            Assert.That(query["fields[tier]"], Is.EqualTo("title"));
            Assert.That(query["fields[user]"], Is.EqualTo("full_name"));
        }
    }

    [Test]
    public async Task GetCampaignMembers_Fails_WhenPatreonRejectsTheAccessToken()
    {
        var handler = new StubPatreonHandler(_ => StubPatreonHandler.Json("""{ "errors": [ { "code_name": "Unauthorized" } ] }""", HttpStatusCode.Unauthorized));

        var result = await handler.CreateClient().GetCampaignMembers("an-expired-token");

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error, Does.Contain("Unauthorized"));
        }
    }

    [Test]
    public void GetCampaignMembers_Throws_WithPatreonsResponse_OnAnyOtherError()
    {
        var handler = new StubPatreonHandler(_ => StubPatreonHandler.Json("""{ "errors": [ { "detail": "Something broke" } ] }""", HttpStatusCode.InternalServerError));

        var exception = Assert.ThrowsAsync<HttpRequestException>(() => handler.CreateClient().GetCampaignMembers("the-access-token"));

        Assert.That(exception!.Message, Does.Contain("500").And.Contain("Something broke"));
    }

    [Test]
    public async Task RefreshTokens_PostsTheDocumentedFormBody_IncludingTheClientSecret()
    {
        var handler = new StubPatreonHandler(_ => StubPatreonHandler.Json("""
            { "access_token": "new-access", "refresh_token": "new-refresh", "expires_in": 2678400, "scope": "campaigns", "token_type": "Bearer" }
            """));

        var tokens = await handler.CreateClient().RefreshTokens("old-refresh");

        var request = handler.Requests.Single();
        var form = HttpUtility.ParseQueryString(request.Body!);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(tokens, Is.EqualTo(new PatreonTokens("new-access", "new-refresh")));
            Assert.That(request.Method, Is.EqualTo(HttpMethod.Post));
            Assert.That(request.Uri.AbsoluteUri, Is.EqualTo("https://www.patreon.com/api/oauth2/token"), "the refresh token belongs in the body, not the URL");
            Assert.That(form["grant_type"], Is.EqualTo("refresh_token"));
            Assert.That(form["refresh_token"], Is.EqualTo("old-refresh"));
            Assert.That(form["client_id"], Is.EqualTo("the-client"));
            Assert.That(form["client_secret"], Is.EqualTo("the-secret"));
        }
    }

    [Test]
    public void RefreshTokens_Throws_WithPatreonsResponse_WhenRefused()
    {
        var handler = new StubPatreonHandler(_ => StubPatreonHandler.Json("""{ "error": "invalid_grant" }""", HttpStatusCode.BadRequest));

        var exception = Assert.ThrowsAsync<HttpRequestException>(() => handler.CreateClient().RefreshTokens("a-spent-refresh-token"));

        Assert.That(exception!.Message, Does.Contain("400").And.Contain("invalid_grant"));
    }
}
