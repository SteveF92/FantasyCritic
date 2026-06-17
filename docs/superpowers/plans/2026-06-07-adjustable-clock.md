# Adjustable Clock for Integration Tests — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add an `AdjustableClock` (`IClock` implementation) to `FantasyCritic.Lib`, expose `SetInitialTime` / `SetTime` admin endpoints gated by a config flag, and wire the fake clock into the integration test factory so time-sensitive tests can control the application's sense of "now".

**Architecture:** `AdjustableClock` stores a `(setAt, targetAt)` snapshot pair under a lock; `GetCurrentInstant()` computes `targetAt + (realNow - setAt)` so fake time advances naturally between Set calls. Two new `AdminController` endpoints are gated by `IConfiguration["IntegrationTestMode"]` and return 404 in production. The test factory replaces the `IClock` and `RepositoryConfiguration` DI registrations with a shared `AdjustableClock` singleton.

**Tech Stack:** C# / ASP.NET Core, NodaTime (`IClock`, `Instant`), CSharpFunctionalExtensions (`Result`), NUnit, NSwag typed client generation.

---

## File Map

| File | Action |
|---|---|
| `src/FantasyCritic.Lib/Utilities/AdjustableClock.cs` | **Create** — `IClock` implementation with offset-snapshot math |
| `src/FantasyCritic.Test/AdjustableClockTests.cs` | **Create** — NUnit unit tests for `AdjustableClock` |
| `src/FantasyCritic.Web/Models/Requests/Admin/SetTimeRequest.cs` | **Create** — request record carrying `Instant NewTime` |
| `src/FantasyCritic.Web/Controllers/API/AdminController.cs` | **Modify** — add `IConfiguration`, add `SetInitialTime` and `SetTime` actions |
| `src/FantasyCritic.IntegrationTests/FantasyCriticWebApplicationFactory.cs` | **Modify** — create `AdjustableClock`, register as `IClock`, update `RepositoryConfiguration`, add config flag |
| `src/FantasyCritic.IntegrationTests/Tests/Admin/AdminTests.cs` | **Modify** — add clock-control smoke tests |

---

## Task 1: `AdjustableClock` — unit tests + implementation

**Files:**
- Create: `src/FantasyCritic.Test/AdjustableClockTests.cs`
- Create: `src/FantasyCritic.Lib/Utilities/AdjustableClock.cs`

- [ ] **Step 1: Write the failing unit tests**

Create `src/FantasyCritic.Test/AdjustableClockTests.cs`:

```csharp
using CSharpFunctionalExtensions;
using FantasyCritic.Lib.Utilities;
using NodaTime;
using NUnit.Framework;

namespace FantasyCritic.Test;

[TestFixture]
public class AdjustableClockTests
{
    [Test]
    public void InitialTime_IsApproximatelyRealNow()
    {
        var clock = new AdjustableClock();
        var realNow = SystemClock.Instance.GetCurrentInstant();
        var fakNow = clock.GetCurrentInstant();
        // Allow a 5-second window to account for slow test runners.
        Assert.That(fakNow, Is.EqualTo(realNow).Within(Duration.FromSeconds(5)));
    }

    [Test]
    public void SetInitialTime_SetsTimeUnconditionally()
    {
        var clock = new AdjustableClock();
        var target = Instant.FromUtc(2024, 1, 1, 0, 0, 0);

        clock.SetInitialTime(target);

        var fakNow = clock.GetCurrentInstant();
        // Allow a 5-second window for elapsed real time after the Set call.
        Assert.That(fakNow, Is.EqualTo(target).Within(Duration.FromSeconds(5)));
    }

    [Test]
    public void SetInitialTime_AllowsGoingBackwards()
    {
        var clock = new AdjustableClock();
        var future = Instant.FromUtc(2030, 6, 1, 0, 0, 0);
        clock.SetInitialTime(future);

        var past = Instant.FromUtc(2020, 1, 1, 0, 0, 0);
        clock.SetInitialTime(past);

        var fakNow = clock.GetCurrentInstant();
        Assert.That(fakNow, Is.EqualTo(past).Within(Duration.FromSeconds(5)));
    }

    [Test]
    public void SetTime_ForwardSucceeds()
    {
        var clock = new AdjustableClock();
        var start = Instant.FromUtc(2024, 1, 1, 0, 0, 0);
        clock.SetInitialTime(start);

        var later = Instant.FromUtc(2024, 6, 1, 0, 0, 0);
        var result = clock.SetTime(later);

        Assert.That(result.IsSuccess, Is.True);
        var fakNow = clock.GetCurrentInstant();
        Assert.That(fakNow, Is.EqualTo(later).Within(Duration.FromSeconds(5)));
    }

    [Test]
    public void SetTime_BackwardsFails()
    {
        var clock = new AdjustableClock();
        var start = Instant.FromUtc(2024, 6, 1, 0, 0, 0);
        clock.SetInitialTime(start);

        var earlier = Instant.FromUtc(2024, 1, 1, 0, 0, 0);
        var result = clock.SetTime(earlier);

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Does.Contain("past"));
    }

    [Test]
    public void SetTime_ToCurrentTime_Succeeds()
    {
        var clock = new AdjustableClock();
        var start = Instant.FromUtc(2024, 1, 1, 0, 0, 0);
        clock.SetInitialTime(start);

        // Setting to exactly the current fake time (same instant) should succeed.
        var result = clock.SetTime(start);

        Assert.That(result.IsSuccess, Is.True);
    }
}
```

- [ ] **Step 2: Run tests to verify they fail**

```powershell
dotnet test src/FantasyCritic.Test/FantasyCritic.Test.csproj --filter "AdjustableClockTests"
```

Expected: Build error — `AdjustableClock` does not exist yet.

- [ ] **Step 3: Implement `AdjustableClock`**

Create `src/FantasyCritic.Lib/Utilities/AdjustableClock.cs`:

```csharp
using CSharpFunctionalExtensions;
using NodaTime;

namespace FantasyCritic.Lib.Utilities;

/// <summary>
/// An <see cref="IClock"/> whose current time can be controlled programmatically.
/// Intended for integration-test scenarios that need to advance the application's
/// sense of "now" without waiting for real time to pass.
///
/// Time formula: targetAt + (realNow - setAt)
/// This means fake time advances naturally between Set calls, so records written
/// at different moments always get distinct timestamps.
/// </summary>
public sealed class AdjustableClock : IClock
{
    private readonly object _lock = new();
    private Instant _setAt;
    private Instant _targetAt;

    public AdjustableClock()
    {
        var now = SystemClock.Instance.GetCurrentInstant();
        _setAt = now;
        _targetAt = now;
    }

    public Instant GetCurrentInstant()
    {
        lock (_lock)
        {
            return _targetAt + (SystemClock.Instance.GetCurrentInstant() - _setAt);
        }
    }

    /// <summary>
    /// Sets the fake current time unconditionally. May go backwards.
    /// Use this to establish the starting point of a test scenario.
    /// </summary>
    public void SetInitialTime(Instant target)
    {
        lock (_lock)
        {
            _setAt = SystemClock.Instance.GetCurrentInstant();
            _targetAt = target;
        }
    }

    /// <summary>
    /// Advances the fake current time. Only accepts targets &gt;= the current fake time.
    /// Use this to move time forward during a test scenario without risk of accidentally
    /// drifting backwards.
    /// </summary>
    public Result SetTime(Instant target)
    {
        lock (_lock)
        {
            var current = _targetAt + (SystemClock.Instance.GetCurrentInstant() - _setAt);
            if (target < current)
            {
                return Result.Failure($"Target time {target} is in the past relative to the current fake time {current}.");
            }

            _setAt = SystemClock.Instance.GetCurrentInstant();
            _targetAt = target;
            return Result.Success();
        }
    }
}
```

- [ ] **Step 4: Run tests to verify they pass**

```powershell
dotnet test src/FantasyCritic.Test/FantasyCritic.Test.csproj --filter "AdjustableClockTests"
```

Expected: 6 tests pass.

- [ ] **Step 5: Commit**

```powershell
git add src/FantasyCritic.Lib/Utilities/AdjustableClock.cs src/FantasyCritic.Test/AdjustableClockTests.cs
git commit -m "Add AdjustableClock IClock implementation with unit tests."
```

---

## Task 2: `SetTimeRequest` record + `AdminController` endpoints

**Files:**
- Create: `src/FantasyCritic.Web/Models/Requests/Admin/SetTimeRequest.cs`
- Modify: `src/FantasyCritic.Web/Controllers/API/AdminController.cs`

- [ ] **Step 1: Create `SetTimeRequest`**

Create `src/FantasyCritic.Web/Models/Requests/Admin/SetTimeRequest.cs`:

```csharp
using NodaTime;

namespace FantasyCritic.Web.Models.Requests.Admin;

public record SetTimeRequest(Instant NewTime);
```

- [ ] **Step 2: Add `IConfiguration` to `AdminController`**

Open `src/FantasyCritic.Web/Controllers/API/AdminController.cs`.

Add to the field declarations (after `private readonly IFantasyCriticRepo _fantasyCriticRepo;`):

```csharp
private readonly IConfiguration _configuration;
```

Update the constructor signature — add `IConfiguration configuration` as the last parameter:

```csharp
public AdminController(AdminService adminService, FantasyCriticService fantasyCriticService, IClock clock, InterLeagueService interLeagueService,
    ILogger<AdminController> logger, GameAcquisitionService gameAcquisitionService, FantasyCriticUserManager userManager,
    IWebHostEnvironment webHostEnvironment, EmailSendingService emailSendingService, DiscordPushService discordPushService, IMasterGameRepo masterGameRepo,
    IFantasyCriticRepo fantasyCriticRepo, IConfiguration configuration)
    : base(userManager)
```

Add the assignment inside the constructor body (after `_fantasyCriticRepo = fantasyCriticRepo;`):

```csharp
_configuration = configuration;
```

Add `using Microsoft.Extensions.Configuration;` to the top of the file if not already present (check the existing usings first — it may not be there).

- [ ] **Step 3: Add the two new action methods**

Add these two methods anywhere in the body of `AdminController` (e.g. at the end, before the closing brace):

```csharp
[HttpPost]
[ProducesResponseType(StatusCodes.Status200OK)]
public IActionResult SetInitialTime([FromBody] SetTimeRequest request)
{
    var integrationTestMode = _configuration.GetValue<bool>("IntegrationTestMode");
    if (!integrationTestMode)
    {
        return NotFound();
    }

    var adjustableClock = _clock as AdjustableClock;
    if (adjustableClock is null)
    {
        return StatusCode(StatusCodes.Status500InternalServerError,
            "IntegrationTestMode is enabled but the registered IClock is not an AdjustableClock.");
    }

    adjustableClock.SetInitialTime(request.NewTime);
    return Ok();
}

[HttpPost]
[ProducesResponseType(StatusCodes.Status200OK)]
public IActionResult SetTime([FromBody] SetTimeRequest request)
{
    var integrationTestMode = _configuration.GetValue<bool>("IntegrationTestMode");
    if (!integrationTestMode)
    {
        return NotFound();
    }

    var adjustableClock = _clock as AdjustableClock;
    if (adjustableClock is null)
    {
        return StatusCode(StatusCodes.Status500InternalServerError,
            "IntegrationTestMode is enabled but the registered IClock is not an AdjustableClock.");
    }

    var result = adjustableClock.SetTime(request.NewTime);
    if (result.IsFailure)
    {
        return BadRequest(result.Error);
    }

    return Ok();
}
```

Add the required using at the top of the file (these may already be present — add only if missing):

```csharp
using FantasyCritic.Lib.Utilities;
using Microsoft.Extensions.Configuration;
```

- [ ] **Step 4: Build to confirm no errors**

```powershell
dotnet build src/FantasyCritic.Web/FantasyCritic.Web.csproj
```

Expected: Build succeeded, 0 errors.

- [ ] **Step 5: Commit**

```powershell
git add src/FantasyCritic.Web/Models/Requests/Admin/SetTimeRequest.cs src/FantasyCritic.Web/Controllers/API/AdminController.cs
git commit -m "Add SetInitialTime and SetTime admin endpoints gated by IntegrationTestMode config flag."
```

---

## Task 3: Wire `AdjustableClock` into the test factory

**Files:**
- Modify: `src/FantasyCritic.IntegrationTests/FantasyCriticWebApplicationFactory.cs`

- [ ] **Step 1: Update `FantasyCriticWebApplicationFactory`**

Open `src/FantasyCritic.IntegrationTests/FantasyCriticWebApplicationFactory.cs`.

Add this using at the top if not already present:

```csharp
using FantasyCritic.Lib.Utilities;
```

Inside `ConfigureTestServices`, make three changes:

**Change 1 — Add `IntegrationTestMode` to the in-memory config overrides** (inside the existing `configBuilder.AddInMemoryCollection(...)` dictionary in `ConfigureAppConfiguration`):

```csharp
configBuilder.AddInMemoryCollection(new Dictionary<string, string?>
{
    ["BotToken"] = "secret",
    ["ConnectionStrings:DefaultConnection"] = "Server=localhost;Port=3307;Database=fantasycritic;Uid=fantasycritic;Pwd=afantasticpassword;SslMode=required;charset=utf8;",
    ["IntegrationTestMode"] = "true",   // <-- add this line
});
```

**Change 2 — Create the `AdjustableClock` and register it as `IClock`** (add before the existing `services.RemoveAll<IEmailSender>()` line):

```csharp
// Replace IClock with a controllable fake so tests can advance time via the API.
var adjustableClock = new AdjustableClock();
services.RemoveAll<IClock>();
services.AddSingleton<IClock>(adjustableClock);
```

**Change 3 — Update the existing `RepositoryConfiguration` replacement** to use `adjustableClock` instead of `SystemClock.Instance`:

```csharp
// Force the local Docker MySQL connection string, regardless of user secrets.
// Also use the same AdjustableClock so the repo sees the same fake time.
services.RemoveAll<RepositoryConfiguration>();
services.AddSingleton<RepositoryConfiguration>(_ => new RepositoryConfiguration(
    "Server=localhost;Port=3307;Database=fantasycritic;Uid=fantasycritic;Pwd=afantasticpassword;SslMode=required;charset=utf8;",
    adjustableClock));
```

The full `ConfigureTestServices` block after the edits should look like this:

```csharp
builder.ConfigureTestServices(services =>
{
    // Replace IClock with a controllable fake so tests can advance time via the API.
    var adjustableClock = new AdjustableClock();
    services.RemoveAll<IClock>();
    services.AddSingleton<IClock>(adjustableClock);

    // Replace the Postmark sender with a capturing sender so tests can
    // extract confirmation links and follow them.
    services.RemoveAll<IEmailSender>();
    services.AddSingleton<IEmailSender>(CapturingEmailSender);

    // Program.GetConfiguration() builds its own IConfigurationRoot (including user
    // secrets) and passes it directly into ConfigureServices. That means config values
    // like BotToken and the connection string may come from user secrets — pointing at
    // real infrastructure. We fix that here at the DI level.

    // Force the local Docker MySQL connection string, regardless of user secrets.
    // Also use the same AdjustableClock so the repo sees the same fake time.
    services.RemoveAll<RepositoryConfiguration>();
    services.AddSingleton<RepositoryConfiguration>(_ => new RepositoryConfiguration(
        "Server=localhost;Port=3307;Database=fantasycritic;Uid=fantasycritic;Pwd=afantasticpassword;SslMode=required;charset=utf8;",
        adjustableClock));

    // Remove all IHostedService registrations. In Development mode the schedulers are
    // already not registered (gated by !IsDevelopment()), but the Discord bot may still
    // be registered if a real BotToken came in via user secrets.
    services.RemoveAll<IHostedService>();
});
```

- [ ] **Step 2: Build to confirm no errors**

```powershell
dotnet build src/FantasyCritic.IntegrationTests/FantasyCritic.IntegrationTests.csproj
```

Expected: Build succeeded, 0 errors.

- [ ] **Step 3: Commit**

```powershell
git add src/FantasyCritic.IntegrationTests/FantasyCriticWebApplicationFactory.cs
git commit -m "Wire AdjustableClock into integration test factory as IClock and RepositoryConfiguration clock."
```

---

## Task 4: Regenerate NSwag client + integration smoke tests

**Files:**
- Run: NSwag client regeneration script
- Modify: `src/FantasyCritic.IntegrationTests/Tests/Admin/AdminTests.cs`

- [ ] **Step 1: Regenerate the typed API client**

```powershell
dotnet build src/FantasyCritic.Web/FantasyCritic.Web.csproj
scripts/Regenerate-ApiClient.ps1
dotnet build
```

Expected: Build succeeded. The `AdminClient` in `FantasyCritic.ApiClient` now has `SetInitialTimeAsync(SetTimeRequest)` and `SetTimeAsync(SetTimeRequest)` methods.

- [ ] **Step 2: Add clock-control smoke tests to `AdminTests.cs`**

Open `src/FantasyCritic.IntegrationTests/Tests/Admin/AdminTests.cs` and add the following tests. Note: NSwag maps NodaTime `Instant` to `System.DateTimeOffset` in the generated client, so `SetTimeRequest.NewTime` is `DateTimeOffset`, not `Instant`.

```csharp
[Test]
public async Task SetInitialTime_AsAdmin_Returns200()
{
    using var adminSession = new ApiSession(Factory);
    await LoginAsLocalAdminAsync(adminSession);

    await adminSession.Admin.SetInitialTimeAsync(new SetTimeRequest
    {
        NewTime = new DateTimeOffset(2025, 1, 6, 0, 0, 0, TimeSpan.Zero)
    });
    // No exception thrown means the endpoint returned 200.
}

[Test]
public async Task SetTime_Forward_Returns200()
{
    using var adminSession = new ApiSession(Factory);
    await LoginAsLocalAdminAsync(adminSession);

    await adminSession.Admin.SetInitialTimeAsync(new SetTimeRequest
    {
        NewTime = new DateTimeOffset(2025, 1, 6, 0, 0, 0, TimeSpan.Zero)
    });

    await adminSession.Admin.SetTimeAsync(new SetTimeRequest
    {
        NewTime = new DateTimeOffset(2025, 1, 13, 0, 0, 0, TimeSpan.Zero)
    });
    // No exception thrown means the endpoint returned 200.
}

[Test]
public async Task SetTime_Backwards_Returns400()
{
    using var adminSession = new ApiSession(Factory);
    await LoginAsLocalAdminAsync(adminSession);

    await adminSession.Admin.SetInitialTimeAsync(new SetTimeRequest
    {
        NewTime = new DateTimeOffset(2025, 6, 1, 0, 0, 0, TimeSpan.Zero)
    });

    ApiException? ex = null;
    try
    {
        await adminSession.Admin.SetTimeAsync(new SetTimeRequest
        {
            NewTime = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)
        });
    }
    catch (ApiException caught)
    {
        ex = caught;
    }

    Assert.That(ex, Is.Not.Null, "Expected ApiException for backwards time travel.");
    Assert.That(ex!.StatusCode, Is.EqualTo(400));
}
```

No additional usings needed — `FantasyCritic.ApiClient` and `System` are already imported in the file.

- [ ] **Step 3: Run only the admin integration tests to verify**

```powershell
dotnet test src/FantasyCritic.IntegrationTests/FantasyCritic.IntegrationTests.csproj -c Release --filter "AdminTests"
```

Expected: 3 new tests pass (plus any pre-existing admin tests).

- [ ] **Step 4: Run the full integration test suite**

```powershell
dotnet test src/FantasyCritic.IntegrationTests/FantasyCritic.IntegrationTests.csproj -c Release
```

Expected: All tests pass.

- [ ] **Step 5: Commit**

```powershell
git add src/FantasyCritic.IntegrationTests/Tests/Admin/AdminTests.cs
git commit -m "Add clock-control smoke tests (SetInitialTime, SetTime forward/backward)."
```
