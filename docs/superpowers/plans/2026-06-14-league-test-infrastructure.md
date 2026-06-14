# League Test Infrastructure Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Refactor league integration tests to use `LeagueFixture`, reorganize by lifecycle phase, and eliminate duplicated setup boilerplate.

**Architecture:** `LeagueFixtureBuilder` creates leagues via existing `LeagueTestHelpers` primitives. `LeagueFixture` owns sessions and exposes draft lifecycle methods delegating to `DraftSimulator`. Test fixtures keep scenario-specific targets in local records; admin calls stay on typed clients.

**Tech Stack:** NUnit, FantasyCritic.IntegrationTests, NSwag ApiClient

**Spec:** `docs/superpowers/specs/2026-06-14-league-test-infrastructure-design.md`

---

### Task 1: Add LeagueFixture infrastructure

**Files:**
- Create: `src/FantasyCritic.IntegrationTests/Helpers/LeagueFixture.cs`
- Modify: `src/FantasyCritic.IntegrationTests/Helpers/MockedLivePlayer.cs` (add `RunStandardPickCountAsync`)
- Modify: `src/FantasyCritic.IntegrationTests/Helpers/LeagueTestHelpers.cs` (remove `SetUpLeagueAndStartDraftAsync`)

- [ ] Create `TestPublisher`, `LeagueFixture`, `LeagueFixtureBuilder`
- [ ] Add `DraftSimulator.RunStandardPickCountAsync`
- [ ] Remove deprecated helper method

### Task 2: Refactor LeagueDraftTestBase

**Files:**
- Modify: `src/FantasyCritic.IntegrationTests/Tests/League/LeagueDraftTestBase.cs`

- [ ] Replace inline setup with builder + `LeagueFixture`
- [ ] Update shared tests to use `League.Scenario`

### Task 3: Migrate draft edge-case fixtures

**Files:**
- Modify: `DraftEdgeCaseTests.cs`, `DraftCounterPickPhaseTests.cs`, `DraftPauseUndoTests.cs`, `AutoDraftLeagueDraftTests.cs`

- [ ] Use `LeagueFixtureBuilder` and `League.Publishers[n]`

### Task 4: Migrate action fixtures

**Files:**
- Modify: `BidProcessingTests.cs`, `DropProcessingTests.cs`, `EligibilityChangeTests.cs`

- [ ] Replace league setup with builder; use `League.Publishers[n]`; keep admin session separate

### Task 5: Migrate LeagueMemberTests

**Files:**
- Modify: `LeagueMemberTests.cs`

- [ ] Use `CreateLeagueWithMembersAsync`

### Task 6: Reorganize folders and namespaces

**Files:** Move all league test files per spec folder layout; update namespaces.

### Task 7: Update documentation

**Files:**
- Modify: `.cursor/rules/integration-tests.mdc`
- Modify: `.cursor/skills/add-integration-test/SKILL.md`

### Task 8: Verify

```powershell
dotnet test src/FantasyCritic.IntegrationTests/FantasyCritic.IntegrationTests.csproj -c Release
```

Expected: all tests pass.
