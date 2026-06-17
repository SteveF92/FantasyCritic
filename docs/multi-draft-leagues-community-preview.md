# Multi-Draft Leagues — Community Preview

> This document describes an upcoming feature for FantasyCritic. It is a preview shared with the community before implementation begins. Details may change.

---

## What Are Multi-Draft Leagues?

Multi-draft leagues let you run more than one draft within a single league year. Instead of picking all your games in one sitting at the start of the year, you can schedule a second (or third) draft later in the year — giving your league a fresh injection of excitement mid-season.

This has always been one of the most-requested features, and it opens up a lot of creative league formats.

---

## How It Works

### Setting Up Your League

When you create a new league, you'll choose from three presets:

- **Standard League** — same as always. One draft, bids and pickups enabled between picks.
- **One Shot League** — same as always. One draft, no bids or pickups.
- **Multi-Draft League** — schedules two drafts from the start. You configure both right on the creation page. Bids are off by default (everyone's roster is "full" between drafts), but you can turn them on if you want.

You can also build a multi-draft league manually after creation — presets are just shortcuts.

### Scheduling Drafts

After your league is created, the commissioner can visit a new **Manage Drafts** page to:

- Add additional drafts (a third, fourth, etc.)
- Give each draft a name (e.g. "Summer Draft", "Winter Draft")
- Set a scheduled date for each draft
- Configure how many games and counter-picks each draft will cover

When adding a new draft, the commissioner can also **expand the league's total roster size** at the same time — adding more game slots (including special slots) to make room for the upcoming draft's picks.

### Roster Strategy

This is where it gets interesting. The system is flexible by design, so your league can choose its own philosophy:

**Option 1 — Plan the whole year upfront.** Set your total roster size and schedule all your drafts before the first one happens. Everyone knows exactly what's coming.

**Option 2 — Tight rosters, multiple drafts.** Set everyone's roster to exactly how many games get drafted each round. In between drafts, rosters are full and (if bids are off) no pickups happen. Then the commissioner expands slots right before the next draft, and you're off again.

**Option 3 — Schedule as you go.** Only schedule the next draft when you're ready. Great for leagues that want flexibility on timing.

### Forcing Draft Themes

By adding the right slots just before a draft, a commissioner can effectively theme a draft. For example: "In this draft, everyone must pick at least one RPG" — by giving everyone one open RPG special slot and two open standard slots, and running a three-game draft. The system doesn't need a special "themed draft" feature; it falls out naturally from slot management.

---

## During the Draft

### Draft Order

Each draft gets its own draft order. The commissioner sets it the same way as the first draft — with options for:

- **Random** — the system picks randomly
- **Manual** — the commissioner sets the order
- **Inverse of current standings** — the player in last place picks first

### Starting a Draft

A draft can only start once at least one player has enough open roster slots to complete their picks. The commissioner can schedule a draft "optimistically" (before the slots are fully expanded) and add the slots later — but the **Start Draft** button won't activate until the settings are valid.

If a scheduled draft's date has passed without being started, the league page will show a soft reminder to all players. The commissioner can reschedule or start it whenever they're ready — there's no hard deadline.

### Skipped Turns

If a player's roster fills up partway through a later draft (say, they won a bid between drafts and used one of their open slots), their remaining turns in that draft are automatically skipped. The league page will clearly show when and why a player was skipped, so there's no confusion.

---

## Bids and Drops Between Drafts

Leagues can choose whether to allow bids and pickups in between drafts:

- **Bids disabled (default for multi-draft)** — no pickups happen between drafts. Rosters stay locked until the next draft.
- **Bids enabled** — bids and pickups work exactly like a standard league in between drafts.

Either way, **bids and drops are always blocked while a draft is in progress or paused**. You can't place or win a bid during an active draft.

If you want to block drops between drafts but don't want to block bids, you simply give players zero drop allowances. The system doesn't need a separate setting for that.

---

## On Your League Page

### Upcoming Drafts

If your league has two or more drafts, a **Drafts** button on the league home page opens a panel showing all scheduled drafts, their dates, and their status.

If a draft is coming up soon (within a week, or if the draft order has already been set), it will be highlighted at the top of the league page so all players can see it.

### The League Icon

Any league with at least two scheduled drafts gets a special icon on the home page — similar to how One Shot leagues have their own icon today.

---

## Conference Leagues

Multi-draft settings work with conference leagues. When the conference commissioner sets up a multi-draft schedule on the primary league, those settings are cloned to all conference sub-leagues automatically. Individual league managers can adjust their draft's scheduled date, but the core draft structure is managed at the conference level.

---

## Frequently Asked Questions

**Can I add a fourth or fifth draft?**  
Yes. There's no hard cap on the number of drafts per league year. The only constraint is that your total games drafted across all drafts can't exceed your league's total roster size.

**Can I go back and edit a draft after it's been scheduled?**  
Yes, as long as it hasn't started. You can rename a draft at any time. To edit other settings on a started draft, the commissioner can reset it first.

**Can I delete a scheduled draft?**  
Yes, as long as it hasn't started and it isn't the first draft. Every league must always have at least one draft.

**What happens if a player wins a bid that fills their roster before a second draft?**  
Their turns in the second draft are automatically skipped. The page will show this clearly.

**Does this work with One Shot mode?**  
One Shot mode is unchanged for leagues that use it. Multi-draft is its own separate option.

**Can I turn bids on or off after my league is already created?**  
Yes. The commissioner can change the bids setting from the league year settings page at any time. The change is recorded in the league's history.
