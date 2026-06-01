---
name: add-integration-test
description: >-
  Guide a contributor through adding a new integration test fixture to
  FantasyCritic.IntegrationTests. Use when the user wants to add a new
  integration test, test a new API endpoint, or asks about writing tests
  against the live HTTP stack.
---

# Add an Integration Test

## Project layout

```
src/FantasyCritic.IntegrationTests/
  IntegrationTestBase.cs          ← base class, always inherit this
  FantasyCriticWebApplicationFactory.cs
  NullEmailSender.cs
  Helpers/
    ApiSession.cs                 ← HTTP client wrapper; all test HTTP calls go here
    AntiForgeryHelper.cs
  Tests/
    Auth/AuthTests.cs             ← existing example
    <FeatureArea>/                ← add yours here
```

## Step 1 – Find the request and response types

- Request types: `src/FantasyCritic.Web/Models/Requests/<Area>/*Request.cs`
- Response types: `src/FantasyCritic.Lib/SharedSerialization/API/*ViewModel.cs` or `src/FantasyCritic.Web/Models/Responses/`
- The test project already references `FantasyCritic.Web` (and transitively `FantasyCritic.Lib`), so all these types are available with no extra references.

## Step 2 – Create the fixture

```csharp
using System.Threading.Tasks;
using FantasyCritic.IntegrationTests.Helpers;
using FantasyCritic.Web.Models.Requests.<Area>;   // request type
using FantasyCritic.Lib.SharedSerialization.API;   // response type
using NUnit.Framework;

namespace FantasyCritic.IntegrationTests.Tests.<Area>;

[TestFixture]
public class <Area>Tests : IntegrationTestBase
{
    [Test]
    public async Task <Scenario>()
    {
        var (email, password, displayName) = NewUser();
        using var session = new ApiSession(Factory);
        await session.RegisterAsync(email, password, displayName);  // auto-signs-in

        // POST that returns a resource
        var result = await session.PostJsonAndDeserializeAsync<CreateXRequest, XViewModel>(
            "/api/<Area>/Create<X>", new CreateXRequest(...));
        Assert.That(result.Id, Is.Not.EqualTo(Guid.Empty));

        // GET
        var vm = await session.GetAndDeserializeAsync<XViewModel>($"/api/<Area>/Get<X>/{result.Id}");
        Assert.That(vm.Name, Is.EqualTo("expected name"));
    }
}
```

## Step 3 – Run

```powershell
# From repo root — confirm Docker is running first
docker compose -f infrastructure/docker-compose-mysql.yaml up -d

# -c Release avoids Debug DLL lock conflicts with the dev web server
dotnet test src/FantasyCritic.IntegrationTests/ -c Release
```

Expected: all tests pass, no 5xx responses.

## ApiSession helpers reference

| Method | Use for |
|--------|---------|
| `RegisterAsync(email, pw, name)` | Create a user and sign in (session is authenticated after this) |
| `LoginAsync(email, pw)` | Sign in on a fresh session; returns `true` on success |
| `GetAsync(path)` | Raw GET, returns `HttpResponseMessage` |
| `GetAndDeserializeAsync<T>(path)` | GET + deserialize; throws on non-2xx |
| `PostJsonAsync<T>(path, body)` | POST typed request, returns `HttpResponseMessage` |
| `PostJsonAndDeserializeAsync<TReq, TResp>(path, body)` | POST + deserialize response; throws on non-2xx |

## Rules

- Always use `NewUser()` — never hardcode credentials
- Always pass concrete request/response types — no anonymous objects or `JObject`
- Always build state through the API — no direct DB inserts
- No teardown needed — GUID-based names never collide
