# Game Controller Integration Tests Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add integration test coverage for `GameController`, consolidate the master game request lifecycle tests into dedicated fixtures, and fix a missing `[Authorize]` attribute on six controller actions.

**Architecture:** One prerequisite task (controller fix + client regen), then three new test fixtures under `Tests/Game/`, plus a trim of `FactCheckerTests` to remove the two tests that move to `MasterGameRequestTests`. All tests use the NSwag-generated `GameClient` / `FactCheckerClient` via `ApiSession` — no raw URLs, no direct DB access.

**Tech Stack:** C#, NUnit, ASP.NET Core integration testing (`WebApplicationFactory`), NSwag-generated typed API client (`FantasyCritic.ApiClient`), MySQL (Docker), NodaTime.

---

## File map

| Action | File |
|---|---|
| Modify | `src/FantasyCritic.Web/Controllers/API/GameController.cs` |
| Modify | `src/FantasyCritic.IntegrationTests/Tests/FactChecker/FactCheckerTests.cs` |
| Create | `src/FantasyCritic.IntegrationTests/Tests/Game/GameTests.cs` |
| Create | `src/FantasyCritic.IntegrationTests/Tests/Game/MasterGameRequestTests.cs` |
| Create | `src/FantasyCritic.IntegrationTests/Tests/Game/MasterGameChangeRequestTests.cs` |
| Regenerate | `src/FantasyCritic.ApiClient/Generated/FantasyCriticClients.cs` (script-generated, do not hand-edit) |

---

## Conventions to follow

- Inherit from `IntegrationTestBase` (provides `Factory`, `NewUser()`, `LoginAsLocalAdminAsync`).
- Create one `ApiSession` per user per test; wrap in `using`.
- Use `NewUser()` for every user credential — never hardcode.
- All imports from `FantasyCritic.ApiClient` (generated types). Do **not** import from `FantasyCritic.Web.Models.*` or `FantasyCritic.Lib.*` in test files.
- Catch `ApiException` (from `FantasyCritic.ApiClient`) for error-status assertions.
- Run the full test suite with: `dotnet test src/FantasyCritic.IntegrationTests/FantasyCritic.IntegrationTests.csproj -c Release`
- Docker DB must be running: `docker compose -f infrastructure/docker-compose-mysql.yaml up -d`

---

## Task 1: Add `[Authorize]` to six GameController actions

**Files:**
- Modify: `src/FantasyCritic.Web/Controllers/API/GameController.cs`
- Regenerate: `src/FantasyCritic.ApiClient/Generated/FantasyCriticClients.cs`

- [ ] **Step 1: Add `[Authorize]` to the six POST actions**

In `GameController.cs`, add `[Authorize]` to each of these six actions. The attribute goes on the line immediately before `[HttpPost]`. Each action currently has `[HttpPost]` and one or more `[ProducesResponseType]` attributes; insert `[Authorize]` before `[HttpPost]`:

```csharp
// CreateMasterGameRequest — was:
[HttpPost]
[ProducesResponseType(StatusCodes.Status200OK)]
public async Task<IActionResult> CreateMasterGameRequest(...)

// becomes:
[HttpPost]
[Authorize]
[ProducesResponseType(StatusCodes.Status200OK)]
public async Task<IActionResult> CreateMasterGameRequest(...)
```

Apply the same change to all six actions:
- `CreateMasterGameRequest`
- `CreateMasterGameChangeRequest`
- `DeleteMasterGameRequest`
- `DeleteMasterGameChangeRequest`
- `DismissMasterGameRequest`
- `DismissMasterGameChangeRequest`

- [ ] **Step 2: Rebuild the web project and regenerate the API client**

```powershell
dotnet build src/FantasyCritic.Web/FantasyCritic.Web.csproj
scripts/Regenerate-ApiClient.ps1
dotnet build
```

Expected: build succeeds with no errors. The generated file `src/FantasyCritic.ApiClient/Generated/FantasyCriticClients.cs` will be updated (timestamp changes, possibly minor formatting diffs — that is normal).

- [ ] **Step 3: Commit**

```powershell
git add src/FantasyCritic.Web/Controllers/API/GameController.cs
git add src/FantasyCritic.ApiClient/Generated/FantasyCriticClients.cs
git commit -m "Add [Authorize] to GameController POST actions that require authentication."
```

---

## Task 2: Create `Tests/Game/GameTests.cs`

**Files:**
- Create: `src/FantasyCritic.IntegrationTests/Tests/Game/GameTests.cs`

- [ ] **Step 1: Create the file with all 8 tests**

```csharp
using System;
using System.Threading.Tasks;
using FantasyCritic.ApiClient;
using FantasyCritic.IntegrationTests.Helpers;
using NUnit.Framework;

namespace FantasyCritic.IntegrationTests.Tests.Game;

[TestFixture]
public class GameTests : IntegrationTestBase
{
    // ── Smoke: public read endpoints ──────────────────────────────────────

    [Test]
    public async Task SupportedYears_ReturnsNonEmptyList()
    {
        using var session = new ApiSession(Factory);
        var result = await session.Game.SupportedYearsAsync();
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count, Is.GreaterThan(0));
    }

    [Test]
    public async Task GetMasterGameTags_ReturnsNonEmptyList()
    {
        using var session = new ApiSession(Factory);
        var result = await session.Game.GetMasterGameTagsAsync();
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count, Is.GreaterThan(0));
    }

    [Test]
    public async Task MasterGameAll_ReturnsNonEmptyList()
    {
        using var session = new ApiSession(Factory);
        var result = await session.Game.MasterGameAllAsync();
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count, Is.GreaterThan(0));
    }

    // ── Not-found cases ───────────────────────────────────────────────────

    [Test]
    public async Task MasterGame_UnknownID_Returns404()
    {
        using var session = new ApiSession(Factory);
        ApiException? ex = null;
        try
        {
            await session.Game.MasterGameAsync(Guid.NewGuid());
        }
        catch (ApiException caught)
        {
            ex = caught;
        }
        Assert.That(ex, Is.Not.Null, "Expected ApiException for unknown game ID.");
        Assert.That(ex!.StatusCode, Is.EqualTo(404));
    }

    [Test]
    public async Task MasterGameChangeLog_UnknownID_Returns404()
    {
        using var session = new ApiSession(Factory);
        ApiException? ex = null;
        try
        {
            await session.Game.MasterGameChangeLogAsync(Guid.NewGuid());
        }
        catch (ApiException caught)
        {
            ex = caught;
        }
        Assert.That(ex, Is.Not.Null, "Expected ApiException for unknown game ID.");
        Assert.That(ex!.StatusCode, Is.EqualTo(404));
    }

    // ── Auth edge cases ───────────────────────────────────────────────────

    [Test]
    public async Task MyMasterGameRequests_Unauthenticated_Returns401()
    {
        using var session = new ApiSession(Factory);
        ApiException? ex = null;
        try
        {
            await session.Game.MyMasterGameRequestsAsync();
        }
        catch (ApiException caught)
        {
            ex = caught;
        }
        Assert.That(ex, Is.Not.Null, "Expected ApiException for unauthenticated request.");
        Assert.That(ex!.StatusCode, Is.EqualTo(401));
    }

    [Test]
    public async Task MyMasterGameChangeRequests_Unauthenticated_Returns401()
    {
        using var session = new ApiSession(Factory);
        ApiException? ex = null;
        try
        {
            await session.Game.MyMasterGameChangeRequestsAsync();
        }
        catch (ApiException caught)
        {
            ex = caught;
        }
        Assert.That(ex, Is.Not.Null, "Expected ApiException for unauthenticated request.");
        Assert.That(ex!.StatusCode, Is.EqualTo(401));
    }

    [Test]
    public async Task LeagueYearsWithMasterGame_Unauthenticated_ReturnsEmptyList()
    {
        using var session = new ApiSession(Factory);
        var result = await session.Game.LeagueYearsWithMasterGameAsync(Guid.NewGuid());
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count, Is.EqualTo(0));
    }
}
```

- [ ] **Step 2: Run the new tests**

```powershell
dotnet test src/FantasyCritic.IntegrationTests/FantasyCritic.IntegrationTests.csproj -c Release --filter "FullyQualifiedName~GameTests"
```

Expected: all 8 tests pass. If `MasterGameAll_ReturnsNonEmptyList` fails, the seed DB has no master games — verify Docker DB is running and seeded.

- [ ] **Step 3: Commit**

```powershell
git add src/FantasyCritic.IntegrationTests/Tests/Game/GameTests.cs
git commit -m "Add GameTests: smoke reads, not-found cases, auth edge cases."
```

---

## Task 3: Create `Tests/Game/MasterGameRequestTests.cs`

**Files:**
- Create: `src/FantasyCritic.IntegrationTests/Tests/Game/MasterGameRequestTests.cs`
- Modify: `src/FantasyCritic.IntegrationTests/Tests/FactChecker/FactCheckerTests.cs`

- [ ] **Step 1: Create the file with all 8 tests**

```csharp
using System;
using System.Linq;
using System.Threading.Tasks;
using FantasyCritic.ApiClient;
using FantasyCritic.IntegrationTests.Helpers;
using NUnit.Framework;

namespace FantasyCritic.IntegrationTests.Tests.Game;

[TestFixture]
public class MasterGameRequestTests : IntegrationTestBase
{
    private async Task GrantFactCheckerRoleAsync(Guid userID)
    {
        using var adminSession = new ApiSession(Factory);
        await LoginAsLocalAdminAsync(adminSession);
        await adminSession.Admin.GrantRoleAsync(new UserRoleRequest
        {
            UserID = userID,
            RoleName = "FactChecker",
        });
    }

    // ── User side ─────────────────────────────────────────────────────────

    [Test]
    public async Task CreateMasterGameRequest_AppearsInMyRequests()
    {
        var (email, password, displayName) = NewUser();
        using var session = new ApiSession(Factory);
        await session.RegisterAsync(email, password, displayName);

        var gameName = $"Request Test {Guid.NewGuid():N}"[..36];
        await session.Game.CreateMasterGameRequestAsync(new MasterGameRequestRequest
        {
            GameName = gameName,
            EstimatedReleaseDate = "2099",
            RequestNote = "Integration test request.",
        });

        var requests = await session.Game.MyMasterGameRequestsAsync();
        Assert.That(requests.Any(r => r.GameName == gameName), Is.True,
            "Created request should appear in MyMasterGameRequests.");
    }

    [Test]
    public async Task DeleteMasterGameRequest_DisappearsFromMyRequests()
    {
        var (email, password, displayName) = NewUser();
        using var session = new ApiSession(Factory);
        await session.RegisterAsync(email, password, displayName);

        var gameName = $"Delete Test {Guid.NewGuid():N}"[..36];
        await session.Game.CreateMasterGameRequestAsync(new MasterGameRequestRequest
        {
            GameName = gameName,
            EstimatedReleaseDate = "2099",
            RequestNote = "Will be deleted.",
        });

        var requests = await session.Game.MyMasterGameRequestsAsync();
        var requestID = requests.Single(r => r.GameName == gameName).RequestID;

        await session.Game.DeleteMasterGameRequestAsync(new MasterGameRequestDeletionRequest
        {
            RequestID = requestID,
        });

        var afterDelete = await session.Game.MyMasterGameRequestsAsync();
        Assert.That(afterDelete.Any(r => r.RequestID == requestID), Is.False,
            "Deleted request should no longer appear in MyMasterGameRequests.");
    }

    [Test]
    public async Task DismissMasterGameRequest_SetsHiddenFlag()
    {
        var (email, password, displayName) = NewUser();
        using var session = new ApiSession(Factory);
        await session.RegisterAsync(email, password, displayName);

        var gameName = $"Dismiss Test {Guid.NewGuid():N}"[..36];
        await session.Game.CreateMasterGameRequestAsync(new MasterGameRequestRequest
        {
            GameName = gameName,
            EstimatedReleaseDate = "2099",
            RequestNote = "Will be dismissed.",
        });

        var requests = await session.Game.MyMasterGameRequestsAsync();
        var requestID = requests.Single(r => r.GameName == gameName).RequestID;

        await session.Game.DismissMasterGameRequestAsync(new MasterGameRequestDismissRequest
        {
            RequestID = requestID,
        });

        var afterDismiss = await session.Game.MyMasterGameRequestsAsync();
        var dismissed = afterDismiss.Single(r => r.RequestID == requestID);
        Assert.That(dismissed.Hidden, Is.True,
            "Dismissed request should have Hidden == true.");
    }

    [Test]
    public async Task DeleteMasterGameRequest_NotOwner_Returns403()
    {
        var (emailA, passwordA, displayNameA) = NewUser();
        using var sessionA = new ApiSession(Factory);
        await sessionA.RegisterAsync(emailA, passwordA, displayNameA);

        var gameName = $"Other User Request {Guid.NewGuid():N}"[..36];
        await sessionA.Game.CreateMasterGameRequestAsync(new MasterGameRequestRequest
        {
            GameName = gameName,
            EstimatedReleaseDate = "2099",
            RequestNote = "Owned by User A.",
        });

        var requestsA = await sessionA.Game.MyMasterGameRequestsAsync();
        var requestID = requestsA.Single(r => r.GameName == gameName).RequestID;

        var (emailB, passwordB, displayNameB) = NewUser();
        using var sessionB = new ApiSession(Factory);
        await sessionB.RegisterAsync(emailB, passwordB, displayNameB);

        ApiException? ex = null;
        try
        {
            await sessionB.Game.DeleteMasterGameRequestAsync(new MasterGameRequestDeletionRequest
            {
                RequestID = requestID,
            });
        }
        catch (ApiException caught)
        {
            ex = caught;
        }
        Assert.That(ex, Is.Not.Null, "Expected ApiException for non-owner delete.");
        Assert.That(ex!.StatusCode, Is.EqualTo(403));
    }

    [Test]
    public async Task DeleteMasterGameRequest_UnknownID_Returns400()
    {
        var (email, password, displayName) = NewUser();
        using var session = new ApiSession(Factory);
        await session.RegisterAsync(email, password, displayName);

        ApiException? ex = null;
        try
        {
            await session.Game.DeleteMasterGameRequestAsync(new MasterGameRequestDeletionRequest
            {
                RequestID = Guid.NewGuid(),
            });
        }
        catch (ApiException caught)
        {
            ex = caught;
        }
        Assert.That(ex, Is.Not.Null, "Expected ApiException for unknown request ID.");
        Assert.That(ex!.StatusCode, Is.EqualTo(400));
    }

    [Test]
    public async Task CreateMasterGameRequest_Unauthenticated_Returns401()
    {
        using var session = new ApiSession(Factory);
        ApiException? ex = null;
        try
        {
            await session.Game.CreateMasterGameRequestAsync(new MasterGameRequestRequest
            {
                GameName = "Some Game",
                EstimatedReleaseDate = "2099",
                RequestNote = "Should be rejected.",
            });
        }
        catch (ApiException caught)
        {
            ex = caught;
        }
        Assert.That(ex, Is.Not.Null, "Expected ApiException for unauthenticated request.");
        Assert.That(ex!.StatusCode, Is.EqualTo(401));
    }

    // ── FactChecker side (moved from FactCheckerTests) ────────────────────

    [Test]
    public async Task ApproveMasterGameRequest_WithLinkedGame_Succeeds()
    {
        var (requesterEmail, requesterPassword, requesterDisplayName) = NewUser();
        using var requesterSession = new ApiSession(Factory);
        await requesterSession.RegisterAsync(requesterEmail, requesterPassword, requesterDisplayName);

        var requestedGameName = $"Silksong Alt {Guid.NewGuid():N}"[..36];
        await requesterSession.Game.CreateMasterGameRequestAsync(new MasterGameRequestRequest
        {
            GameName = requestedGameName,
            EstimatedReleaseDate = "2027",
            RequestNote = "Highly anticipated Metroidvania sequel from Team Cherry. Confirmed to be in development; expected to release in the near future. Steam page exists, significant community demand.",
        });

        var requests = await requesterSession.Game.MyMasterGameRequestsAsync();
        var requestID = requests.Single(r => r.GameName == requestedGameName).RequestID;

        var (fcEmail, fcPassword, fcDisplayName) = NewUser();
        using var fcRegSession = new ApiSession(Factory);
        await fcRegSession.RegisterAsync(fcEmail, fcPassword, fcDisplayName);
        var fcMe = await fcRegSession.Account.CurrentUserAsync();
        await GrantFactCheckerRoleAsync(fcMe.UserID);

        using var fcSession = new ApiSession(Factory);
        await fcSession.LoginAsync(fcEmail, fcPassword);

        var createdGame = await fcSession.FactChecker.CreateMasterGameAsync(new CreateMasterGameRequest
        {
            GameName = requestedGameName,
            EstimatedReleaseDate = "2027",
            Tags = new[] { "NewGame" },
        });

        using var completionResult = await fcSession.FactChecker.CompleteMasterGameRequestAsync(new CompleteMasterGameRequestRequest
        {
            RequestID = requestID,
            ResponseNote = "Added to the database. Meets all eligibility requirements.",
            MasterGameID = createdGame.MasterGameID,
        });

        var updatedRequests = await requesterSession.Game.MyMasterGameRequestsAsync();
        var answeredRequest = updatedRequests.Single(r => r.RequestID == requestID);

        Assert.That(answeredRequest.Answered, Is.True);
        Assert.That(answeredRequest.MasterGame, Is.Not.Null);
        Assert.That(answeredRequest.MasterGame!.MasterGameID, Is.EqualTo(createdGame.MasterGameID));
    }

    [Test]
    public async Task DenyMasterGameRequest_NoLinkedGame_Succeeds()
    {
        var (requesterEmail, requesterPassword, requesterDisplayName) = NewUser();
        using var requesterSession = new ApiSession(Factory);
        await requesterSession.RegisterAsync(requesterEmail, requesterPassword, requesterDisplayName);

        var requestedGameName = $"Unknown Mobile Game {Guid.NewGuid():N}"[..40];
        await requesterSession.Game.CreateMasterGameRequestAsync(new MasterGameRequestRequest
        {
            GameName = requestedGameName,
            EstimatedReleaseDate = "TBD",
            RequestNote = "idk some game i saw on tiktok",
        });

        var requests = await requesterSession.Game.MyMasterGameRequestsAsync();
        var requestID = requests.Single(r => r.GameName == requestedGameName).RequestID;

        var (fcEmail, fcPassword, fcDisplayName) = NewUser();
        using var fcRegSession = new ApiSession(Factory);
        await fcRegSession.RegisterAsync(fcEmail, fcPassword, fcDisplayName);
        var fcMe = await fcRegSession.Account.CurrentUserAsync();
        await GrantFactCheckerRoleAsync(fcMe.UserID);

        using var fcSession = new ApiSession(Factory);
        await fcSession.LoginAsync(fcEmail, fcPassword);

        using var completionResult = await fcSession.FactChecker.CompleteMasterGameRequestAsync(new CompleteMasterGameRequestRequest
        {
            RequestID = requestID,
            ResponseNote = "Request too vague to act on. Please provide a game title, Steam link, or other identifying information.",
            MasterGameID = null,
        });

        var updatedRequests = await requesterSession.Game.MyMasterGameRequestsAsync();
        var answeredRequest = updatedRequests.Single(r => r.RequestID == requestID);

        Assert.That(answeredRequest.Answered, Is.True);
        Assert.That(answeredRequest.MasterGame, Is.Null);
    }
}
```

- [ ] **Step 2: Remove the two moved tests from `FactCheckerTests.cs`**

In `src/FantasyCritic.IntegrationTests/Tests/FactChecker/FactCheckerTests.cs`, delete the `ApproveMasterGameRequest_WithLinkedGame_Succeeds` and `DenyMasterGameRequest_NoLinkedGame_Succeeds` test methods in their entirety (from the `[Test]` attribute through the closing `}`). The file should retain only two tests: `ParseEstimatedDate_ValidQuarterInput_ReturnsDateRange` and `CreateMasterGame_Succeeds_AndCanBeRetrievedViaGameController`.

- [ ] **Step 3: Run the new tests and verify the old ones are gone**

```powershell
dotnet test src/FantasyCritic.IntegrationTests/FantasyCritic.IntegrationTests.csproj -c Release --filter "FullyQualifiedName~MasterGameRequestTests"
```

Expected: all 8 tests pass.

```powershell
dotnet test src/FantasyCritic.IntegrationTests/FantasyCritic.IntegrationTests.csproj -c Release --filter "FullyQualifiedName~FactCheckerTests"
```

Expected: 2 tests pass (the approve/deny tests are gone).

- [ ] **Step 4: Commit**

```powershell
git add src/FantasyCritic.IntegrationTests/Tests/Game/MasterGameRequestTests.cs
git add src/FantasyCritic.IntegrationTests/Tests/FactChecker/FactCheckerTests.cs
git commit -m "Add MasterGameRequestTests; move approve/deny tests out of FactCheckerTests."
```

---

## Task 4: Create `Tests/Game/MasterGameChangeRequestTests.cs`

**Files:**
- Create: `src/FantasyCritic.IntegrationTests/Tests/Game/MasterGameChangeRequestTests.cs`

- [ ] **Step 1: Create the file with all 7 tests**

```csharp
using System;
using System.Linq;
using System.Threading.Tasks;
using FantasyCritic.ApiClient;
using FantasyCritic.IntegrationTests.Helpers;
using NUnit.Framework;

namespace FantasyCritic.IntegrationTests.Tests.Game;

[TestFixture]
public class MasterGameChangeRequestTests : IntegrationTestBase
{
    private async Task GrantFactCheckerRoleAsync(Guid userID)
    {
        using var adminSession = new ApiSession(Factory);
        await LoginAsLocalAdminAsync(adminSession);
        await adminSession.Admin.GrantRoleAsync(new UserRoleRequest
        {
            UserID = userID,
            RoleName = "FactChecker",
        });
    }

    /// <summary>
    /// Returns the ID of any master game in the seed DB.
    /// Change request tests need a real MasterGameID as a target.
    /// </summary>
    private async Task<Guid> GetAnyMasterGameIDAsync()
    {
        using var session = new ApiSession(Factory);
        var games = await session.Game.MasterGameAllAsync();
        return games.First().MasterGameID;
    }

    // ── User side ─────────────────────────────────────────────────────────

    [Test]
    public async Task CreateMasterGameChangeRequest_AppearsInMyChangeRequests()
    {
        var masterGameID = await GetAnyMasterGameIDAsync();

        var (email, password, displayName) = NewUser();
        using var session = new ApiSession(Factory);
        await session.RegisterAsync(email, password, displayName);

        await session.Game.CreateMasterGameChangeRequestAsync(new MasterGameChangeRequestRequest
        {
            MasterGameID = masterGameID,
            RequestNote = "The release date needs updating.",
        });

        var changeRequests = await session.Game.MyMasterGameChangeRequestsAsync();
        Assert.That(changeRequests.Any(r => r.MasterGame.MasterGameID == masterGameID), Is.True,
            "Created change request should appear in MyMasterGameChangeRequests.");
    }

    [Test]
    public async Task DeleteMasterGameChangeRequest_DisappearsFromMyChangeRequests()
    {
        var masterGameID = await GetAnyMasterGameIDAsync();

        var (email, password, displayName) = NewUser();
        using var session = new ApiSession(Factory);
        await session.RegisterAsync(email, password, displayName);

        await session.Game.CreateMasterGameChangeRequestAsync(new MasterGameChangeRequestRequest
        {
            MasterGameID = masterGameID,
            RequestNote = "Will be deleted.",
        });

        var changeRequests = await session.Game.MyMasterGameChangeRequestsAsync();
        var requestID = changeRequests.Single(r => r.MasterGame.MasterGameID == masterGameID).RequestID;

        await session.Game.DeleteMasterGameChangeRequestAsync(new MasterGameChangeRequestDeletionRequest
        {
            RequestID = requestID,
        });

        var afterDelete = await session.Game.MyMasterGameChangeRequestsAsync();
        Assert.That(afterDelete.Any(r => r.RequestID == requestID), Is.False,
            "Deleted change request should no longer appear in MyMasterGameChangeRequests.");
    }

    [Test]
    public async Task DismissMasterGameChangeRequest_SetsHiddenFlag()
    {
        var masterGameID = await GetAnyMasterGameIDAsync();

        var (email, password, displayName) = NewUser();
        using var session = new ApiSession(Factory);
        await session.RegisterAsync(email, password, displayName);

        await session.Game.CreateMasterGameChangeRequestAsync(new MasterGameChangeRequestRequest
        {
            MasterGameID = masterGameID,
            RequestNote = "Will be dismissed.",
        });

        var changeRequests = await session.Game.MyMasterGameChangeRequestsAsync();
        var requestID = changeRequests.Single(r => r.MasterGame.MasterGameID == masterGameID).RequestID;

        await session.Game.DismissMasterGameChangeRequestAsync(new MasterGameChangeRequestDismissRequest
        {
            RequestID = requestID,
        });

        var afterDismiss = await session.Game.MyMasterGameChangeRequestsAsync();
        var dismissed = afterDismiss.Single(r => r.RequestID == requestID);
        Assert.That(dismissed.Hidden, Is.True,
            "Dismissed change request should have Hidden == true.");
    }

    [Test]
    public async Task DeleteMasterGameChangeRequest_NotOwner_Returns403()
    {
        var masterGameID = await GetAnyMasterGameIDAsync();

        var (emailA, passwordA, displayNameA) = NewUser();
        using var sessionA = new ApiSession(Factory);
        await sessionA.RegisterAsync(emailA, passwordA, displayNameA);

        await sessionA.Game.CreateMasterGameChangeRequestAsync(new MasterGameChangeRequestRequest
        {
            MasterGameID = masterGameID,
            RequestNote = "Owned by User A.",
        });

        var requestsA = await sessionA.Game.MyMasterGameChangeRequestsAsync();
        var requestID = requestsA.Single(r => r.MasterGame.MasterGameID == masterGameID).RequestID;

        var (emailB, passwordB, displayNameB) = NewUser();
        using var sessionB = new ApiSession(Factory);
        await sessionB.RegisterAsync(emailB, passwordB, displayNameB);

        ApiException? ex = null;
        try
        {
            await sessionB.Game.DeleteMasterGameChangeRequestAsync(new MasterGameChangeRequestDeletionRequest
            {
                RequestID = requestID,
            });
        }
        catch (ApiException caught)
        {
            ex = caught;
        }
        Assert.That(ex, Is.Not.Null, "Expected ApiException for non-owner delete.");
        Assert.That(ex!.StatusCode, Is.EqualTo(403));
    }

    [Test]
    public async Task DeleteMasterGameChangeRequest_UnknownID_Returns400()
    {
        var (email, password, displayName) = NewUser();
        using var session = new ApiSession(Factory);
        await session.RegisterAsync(email, password, displayName);

        ApiException? ex = null;
        try
        {
            await session.Game.DeleteMasterGameChangeRequestAsync(new MasterGameChangeRequestDeletionRequest
            {
                RequestID = Guid.NewGuid(),
            });
        }
        catch (ApiException caught)
        {
            ex = caught;
        }
        Assert.That(ex, Is.Not.Null, "Expected ApiException for unknown request ID.");
        Assert.That(ex!.StatusCode, Is.EqualTo(400));
    }

    [Test]
    public async Task CreateMasterGameChangeRequest_Unauthenticated_Returns401()
    {
        var masterGameID = await GetAnyMasterGameIDAsync();

        using var session = new ApiSession(Factory);
        ApiException? ex = null;
        try
        {
            await session.Game.CreateMasterGameChangeRequestAsync(new MasterGameChangeRequestRequest
            {
                MasterGameID = masterGameID,
                RequestNote = "Should be rejected.",
            });
        }
        catch (ApiException caught)
        {
            ex = caught;
        }
        Assert.That(ex, Is.Not.Null, "Expected ApiException for unauthenticated request.");
        Assert.That(ex!.StatusCode, Is.EqualTo(401));
    }

    // ── FactChecker side ──────────────────────────────────────────────────

    [Test]
    public async Task CompleteMasterGameChangeRequest_Succeeds()
    {
        var masterGameID = await GetAnyMasterGameIDAsync();

        var (requesterEmail, requesterPassword, requesterDisplayName) = NewUser();
        using var requesterSession = new ApiSession(Factory);
        await requesterSession.RegisterAsync(requesterEmail, requesterPassword, requesterDisplayName);

        await requesterSession.Game.CreateMasterGameChangeRequestAsync(new MasterGameChangeRequestRequest
        {
            MasterGameID = masterGameID,
            RequestNote = "Please update the release date.",
        });

        var requests = await requesterSession.Game.MyMasterGameChangeRequestsAsync();
        var requestID = requests.Single(r => r.MasterGame.MasterGameID == masterGameID).RequestID;

        var (fcEmail, fcPassword, fcDisplayName) = NewUser();
        using var fcRegSession = new ApiSession(Factory);
        await fcRegSession.RegisterAsync(fcEmail, fcPassword, fcDisplayName);
        var fcMe = await fcRegSession.Account.CurrentUserAsync();
        await GrantFactCheckerRoleAsync(fcMe.UserID);

        using var fcSession = new ApiSession(Factory);
        await fcSession.LoginAsync(fcEmail, fcPassword);

        await fcSession.FactChecker.CompleteMasterGameChangeRequestAsync(new CompleteMasterGameChangeRequestRequest
        {
            RequestID = requestID,
            ResponseNote = "Reviewed and noted.",
        });

        var updatedRequests = await requesterSession.Game.MyMasterGameChangeRequestsAsync();
        var answeredRequest = updatedRequests.Single(r => r.RequestID == requestID);

        Assert.That(answeredRequest.Answered, Is.True,
            "Change request should be answered after FactChecker completion.");
    }
}
```

- [ ] **Step 2: Run the new tests**

```powershell
dotnet test src/FantasyCritic.IntegrationTests/FantasyCritic.IntegrationTests.csproj -c Release --filter "FullyQualifiedName~MasterGameChangeRequestTests"
```

Expected: all 7 tests pass.

- [ ] **Step 3: Run the full suite to confirm nothing is broken**

```powershell
dotnet test src/FantasyCritic.IntegrationTests/FantasyCritic.IntegrationTests.csproj -c Release
```

Expected: all tests pass.

- [ ] **Step 4: Commit**

```powershell
git add src/FantasyCritic.IntegrationTests/Tests/Game/MasterGameChangeRequestTests.cs
git commit -m "Add MasterGameChangeRequestTests: full change request lifecycle."
```
