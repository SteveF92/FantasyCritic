# Adjustable Clock for Integration Tests — Design Spec

**Date:** 2026-06-07
**Status:** Approved

## Overview

Integration tests that exercise time-sensitive flows (bidding windows, draft deadlines, season transitions) need a way to move the application's sense of "now" forward without waiting for real time to pass. This spec describes an `AdjustableClock` that wraps the real system clock with a controllable offset, exposed via two gated admin endpoints.

---

## 1. `AdjustableClock`

### Location

`FantasyCritic.Lib/Utilities/AdjustableClock.cs`

### Contract

Implements NodaTime's `IClock` interface.

### State

Two `Instant` fields, updated atomically under a lock:

| Field | Meaning |
|---|---|
| `_setAt` | The real-clock instant at the moment of the last `Set*` call |
| `_targetAt` | The desired fake instant requested at that moment |

Both are initialized to `SystemClock.Instance.GetCurrentInstant()` on construction, so the initial offset is zero and fake time equals real time.

### `GetCurrentInstant()`

```
_targetAt + (SystemClock.Instance.GetCurrentInstant() - _setAt)
```

Because the real clock keeps advancing, fake time advances naturally between Set calls — records written at different moments get distinct timestamps without any extra effort.

### `SetInitialTime(Instant target)`

Sets `_setAt` and `_targetAt` unconditionally. No restrictions on direction — callers may go backwards. Intended to establish the starting point of a test scenario.

### `SetTime(Instant target)`

Only accepts `target >= GetCurrentInstant()`. Returns `Result` (CSharpFunctionalExtensions) with a descriptive failure message when the guard triggers. Intended for advancing time during a test; the forward-only constraint prevents accidentally drifting backwards during a scenario.

### Thread Safety

Both fields are updated together inside a `lock` so that `GetCurrentInstant()` never sees a partially-updated pair.

---

## 2. API Endpoints

Both live on `AdminController` under `[Authorize("Admin")]`, consistent with the rest of that controller.

### Gating

Both endpoints check `IConfiguration["IntegrationTestMode"]`. If the value is not `"true"`, they return `404 Not Found`. Using 404 (rather than 403) makes the endpoints invisible in production rather than merely forbidden.

The `_clock` field (type `IClock`) is cast to `AdjustableClock` inside each endpoint after the flag check. If the cast yields `null` (e.g. the flag was manually enabled without registering the fake clock), the endpoint returns `500 Internal Server Error` with a descriptive message rather than crashing with a null reference.

### `POST /api/Admin/SetInitialTime`

- **Request body:** `SetTimeRequest { Instant NewTime }`
- **Behavior:** Calls `AdjustableClock.SetInitialTime(request.NewTime)`
- **Response:** `200 OK`
- **NSwag annotation:** `[ProducesResponseType(StatusCodes.Status200OK)]`

### `POST /api/Admin/SetTime`

- **Request body:** `SetTimeRequest { Instant NewTime }`
- **Behavior:** Calls `AdjustableClock.SetTime(request.NewTime)`
- **Response:** `200 OK` on success; `400 Bad Request` with error message if target is in the past
- **NSwag annotation:** `[ProducesResponseType(StatusCodes.Status200OK)]`

### `SetTimeRequest`

Defined in `FantasyCritic.Web/Models/Requests/Admin/SetTimeRequest.cs`:

```csharp
public record SetTimeRequest(Instant NewTime);
```

`Instant` serializes as an ISO 8601 string via the existing NodaTime/System.Text.Json pipeline (`FantasyCriticJsonOptions`).

---

## 3. DI Wiring

### Production (`HostingExtensions.cs`)

No change. `IClock` continues to be registered as `SystemClock.Instance`. `AdjustableClock` is not registered.

### Test Factory (`FantasyCriticWebApplicationFactory.cs`)

Two additions in `ConfigureTestServices`:

**Replace `IClock` with `AdjustableClock`:**

```csharp
var adjustableClock = new AdjustableClock();
services.RemoveAll<IClock>();
services.AddSingleton<IClock>(adjustableClock);
```

**Update `RepositoryConfiguration`** to use the same instance (it currently hardcodes `SystemClock.Instance`):

```csharp
services.RemoveAll<RepositoryConfiguration>();
services.AddSingleton<RepositoryConfiguration>(_ => new RepositoryConfiguration(
    "Server=localhost;Port=3307;Database=fantasycritic;Uid=fantasycritic;Pwd=afantasticpassword;SslMode=required;charset=utf8;",
    adjustableClock));
```

**Enable the endpoints** via the in-memory config overrides:

```csharp
["IntegrationTestMode"] = "true"
```

The `AdjustableClock` instance is not exposed as a property on the factory. Tests control time exclusively through the HTTP endpoints.

---

## 4. Test Usage Pattern

Tests that need time control log in as the local admin and call the typed NSwag client:

```csharp
// Establish a known starting point (may go backwards)
await adminSession.Admin.SetInitialTimeAsync(new SetTimeRequest { NewTime = Instant.FromUtc(2025, 1, 6, 0, 0) });

// ... do some setup ...

// Advance past the bidding deadline (forward only)
await adminSession.Admin.SetTimeAsync(new SetTimeRequest { NewTime = Instant.FromUtc(2025, 1, 13, 0, 0) });
```

**Clock reset between fixtures:** Each test fixture creates its own `FantasyCriticWebApplicationFactory` (via `IntegrationTestBase`), so the `AdjustableClock` is freshly constructed (offset = zero) for each fixture. No explicit reset is needed.

---

## 5. NSwag Regeneration

After adding the endpoints, regenerate the typed API client before writing or running tests:

```powershell
dotnet build src/FantasyCritic.Web/FantasyCritic.Web.csproj
scripts/Regenerate-ApiClient.ps1
dotnet build
```

---

## Files Touched

| File | Change |
|---|---|
| `FantasyCritic.Lib/Utilities/AdjustableClock.cs` | **New** — `IClock` implementation |
| `FantasyCritic.Web/Models/Requests/Admin/SetTimeRequest.cs` | **New** — request record |
| `FantasyCritic.Web/Controllers/API/AdminController.cs` | **Edit** — add `SetInitialTime` and `SetTime` actions |
| `FantasyCritic.IntegrationTests/FantasyCriticWebApplicationFactory.cs` | **Edit** — register `AdjustableClock`, add config flag |
