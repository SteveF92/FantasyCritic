# API breaking changes

API shape changes that would break existing clients, held until the TypeScript conversion
([typescript-conversion-plan.md](typescript-conversion-plan.md)) is done. Each one was found during the
conversion; the client works around it until then.

## 1. Group a Royale game's owner-only fields

**Where:** `RoyalePublisherGameViewModel` (`FantasyCritic.Web/Models/Responses/Royale`).

**Now:** `MasterGame`, `AmountSpent`, `RefundAmount` and `LockDateTime` are four separate nullable
properties. The server fills all four together when the game isn't hidden or the viewer owns it, and
leaves all four null otherwise. Nothing in the response says which case applies: `GameHidden` is also true
for the owner's own hidden games, which still get the fields.

**Change:** move the four into one nullable object, for example
`RoyalePublisherGameDetailsViewModel? Details`, so the API says they come together or not at all.

**Client today:** `utilities/royaleGames.ts` narrows a game to `OwnRoyalePublisherGame` when the owner
picks it to sell or set a budget, and throws if the fields are missing. With the change, it checks
`details` once instead of each field. `royalePublisher`'s table cells, which check `masterGame` for
"Hidden Until Release", check `details` instead.
