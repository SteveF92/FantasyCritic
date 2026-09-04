# Impossible Bid Prevention

## Goal

Prevent publishers from placing or keeping bids that cannot possibly win because they have no roster slot path at processing time. This closes a gap where full-roster players can spam public bids to bait opponents, and blocks the exploit of placing a drop, placing a bid, then cancelling the drop.

## Constraints

- **No bid-count vs slot-count matching.** Three bids with one open slot remains valid — a lower-priority bid may still win if higher-priority bids lose the auction.
- **No schema changes**, no new API endpoints, no NSwag regen.
- **Server-side enforcement is authoritative.** Frontend changes mirror rules for UX only.
- **Grandfather existing invalid bids** already in the database; enforce on new bid placement, bid edit, and drop cancellation only.
- **`SlotEligibilityFunctions` placeholder behavior** (`acquiringNow: false` → slot 1) stays unchanged so watchlist queuing is unaffected.

## Approach

Add a dedicated pure-function module in Lib and call it from `GameAcquisitionService` at three mutation points: place bid, edit bid, cancel drop.

### Core rule: slot acquisition path

A publisher has a **slot acquisition path** for a bid when at least one of:

| Condition | Standard bids | Counter-pick bids |
| --- | --- | --- |
| Open slot of matching type | Yes | Yes |
| Conditional drop on **this** bid | Yes | N/A (already forbidden) |
| ≥1 active pending drop request | Yes | N/A |

Open-slot detection matches existing code:

```csharp
publisher.GetPublisherSlots(leagueYear)
    .Any(x => x.CounterPick == counterPick && x.PublisherGame is null)
```

### Drop-cancellation rule (Option A)

Block cancelling a pending drop when the post-cancellation state would be:

- zero open slots (of the relevant type), **and**
- zero remaining pending drops, **and**
- at least one active bid without its own conditional drop

No pairing of “this drop covers that bid” — avoids counting.

### Bid-edit rule

Block editing a bid (including removing its conditional drop) when the **post-edit** bid would lack a slot acquisition path under the same rules as placement.

## Components

### New file: `FantasyCritic.Lib/BusinessLogicFunctions/BidSlotPathFunctions.cs`

```csharp
public static class BidSlotPathFunctions
{
    public static bool HasBidSlotAcquisitionPath(
        Publisher publisher,
        LeagueYear leagueYear,
        bool counterPick,
        PublisherGame? conditionalDropOnBid,
        IReadOnlyList<DropRequest> activeDropRequests);

    public static bool WouldBlockDropRemoval(
        Publisher publisher,
        LeagueYear leagueYear,
        DropRequest dropToRemove,
        IReadOnlyList<DropRequest> activeDropRequests,
        IReadOnlyList<PickupBid> activeBids);
}
```

**`HasBidSlotAcquisitionPath` logic:**

1. If any open slot of matching `counterPick` type → `true`
2. If `counterPick` → `false` (no conditional drop or pending drop path)
3. If `conditionalDropOnBid is not null` → `true`
4. If `activeDropRequests.Any()` → `true`
5. Otherwise → `false`

**`WouldBlockDropRemoval` logic:**

1. Compute `remainingDrops = activeDropRequests` excluding `dropToRemove`
2. If any open standard slot → `false` (removal allowed)
3. If `remainingDrops.Any()` → `false`
4. If any active bid where `ConditionalDropPublisherGame is null` → `true` (block)
5. Otherwise → `false`

Counter-pick drop requests do not exist (counter picks cannot be dropped), so drop-removal checks only consider standard-slot state. Pending drops only free standard slots.

### Modified: `FantasyCritic.Lib/Services/GameAcquisitionService.cs`

| Method | Change |
| --- | --- |
| `MakePickupBid` | After existing validation, load active drop requests for publisher; call `HasBidSlotAcquisitionPath`; return `ClaimResult` error if false |
| `EditPickupBid` | After computing post-edit conditional drop, load active drop requests; call `HasBidSlotAcquisitionPath` with the edited conditional drop; reject if false |
| `RemoveDropRequest` | Load active bids and drops for publisher; call `WouldBlockDropRemoval`; return `Result.Failure(...)` if true |

**Error messages:**

| Action | Message |
| --- | --- |
| Place / edit bid | `"You have no open roster spots. Place a drop request or add a conditional drop to this bid."` |
| Cancel drop | `"You can't cancel this drop while you have active bids that depend on it."` |

Counter-pick bids use the same placement message when counter-pick slots are full (no separate path exists).

Special auction bids flow through `MakePickupBid` and receive the same enforcement.

## Frontend (UX mirrors, not enforcement)

### `bidGameForm.vue`

- Disable "Place Bid" when standard slots are full AND no conditional drop selected AND `myActiveDrops` is empty.
- Change warning text from "you can still make bids" to requiring a conditional drop or pending drop.

### `bidCounterPickForm.vue`

- Disable "Place Counter Pick Bid" when counter-pick slots are full.

### `currentBidsForm.vue`

- Add explicit `<option :value="null">None</option>` to conditional drop select (if missing).
- Disable save / show inline warning when edit would remove conditional drop without a slot path (full roster, no active drops).

## Testing

### Unit tests: `FantasyCritic.Test/BidSlotPathFunctionsTests.cs`

| Case | Expected |
| --- | --- |
| Open standard slot | Path exists |
| Full roster + conditional drop on bid | Path exists |
| Full roster + pending drop | Path exists |
| Full roster + neither | No path |
| Full counter-pick slots | No path |
| Drop removal with orphaned non-conditional bid | Blocked |
| Drop removal when remaining bids all have conditional drops | Allowed |
| Drop removal when open slot exists | Allowed |
| Drop removal when other pending drops remain | Allowed |

Build league/publisher fixtures inline (same pattern as `SlotAssignmentFunctionsTests`).

### Integration tests: `FantasyCritic.IntegrationTests/Tests/League/Actions/ImpossibleBidPreventionTests.cs`

Use `LeagueScenarios.FourPlayerDrops` (4 standard + 1 counter-pick drafted per publisher = full rosters after draft). Each test creates its own league via `LeagueFixtureBuilder.CreateAndStartDraftAsync` + `DraftToCompletionAsync` for isolation.

Extend `LeaguePickupActions` with non-throwing helpers:

```csharp
TryPlaceBidAsync(...) → PickupBidResultViewModel
TryDeleteDropAsync(...) → calls DeleteDropRequestAsync, returns success/failure
TryEditPickupBidAsync(...) → PickupBidResultViewModel
```

| Test | Steps | Expected |
| --- | --- | --- |
| `FullRoster_BidWithoutDropOrConditional_Rejected` | Draft complete → bid with no drop path | `Success == false` |
| `FullRoster_PendingDrop_AllowsBid` | Draft complete → drop → bid | `Success == true` |
| `DropThenBidThenCancelDrop_Blocked` | Draft complete → drop → bid → cancel drop | Cancel returns 400 / failure message |
| `EditBid_RemoveConditionalDropWhileFull_Rejected` | Draft complete → bid with conditional drop → edit conditional to null | `Success == false` |
| `FullCounterPickSlots_CounterPickBid_Rejected` | Draft complete → counter-pick bid | `Success == false` |

Use `PickAvailableBidTargetAsync` / droppable game helpers from existing action tests (`DropProcessingTests`) for game selection.

Run:

```powershell
dotnet test src/FantasyCritic.Test/FantasyCritic.Test.csproj --filter "FullyQualifiedName~BidSlotPathFunctions"
dotnet test src/FantasyCritic.IntegrationTests/FantasyCritic.IntegrationTests.csproj -c Release --filter "FullyQualifiedName~ImpossibleBidPrevention"
```

## Out of scope

- Matching bid count to available slots
- Retroactive invalidation of pre-existing impossible bids
- Other anti-spam measures for public bidding
- Manager override / admin bid paths
- Changes to Saturday bid-processing logic (already fails `noSpaceLeftBids` at acquisition time)
