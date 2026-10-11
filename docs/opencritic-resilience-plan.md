# OpenCritic request resilience

## Context

The critic score refresh (inside `FullDataRefresh` and `FullAutomatedActionsProcess`) fails now
and then with `TaskCanceledException: ... HttpClient.Timeout of 100 seconds elapsing`. Logs from
two failed runs (2026-10-10 22:00 UTC and 2026-10-11 00:00 UTC) show:

- Different games each time (21308 about 150 games in; 19922 about 46 games in, and 19922 had
  fetched fine in the other run), so it is not a specific game.
- The request was sent and the client sat waiting for response bytes (`SslStream.EnsureFullTlsFrameAsync`
  inside `HttpConnection.SendAsync`) until the 100s default timeout. Not a connect failure and not
  an error status. RapidAPI answers over-quota calls with an immediate 429, so this does not look
  like rate limiting.
- Requests before the hang completed in 20–300ms. Both failures were at the top of the hour.
- `OpenCriticService` has no retry and rethrows, and `CriticScoreRefresher` does not catch, so one
  hung request out of ~580 fails the whole job. A rerun works.

Decisions made with Steve:
- **Only timeouts + retries, and cancellation.** Letting individual games fail and continuing is
  deferred: it would need tracking of games that keep failing, and an "all games must succeed"
  mode for the run before bids. The hope is that retries alone make the failures go away.

## How we work through this

**Each step ends the same way: build + relevant tests green, commit that step alone, then STOP
and review with Steve before starting the next step.**

## Step 0 — Commit the plan

Commit this plan as `docs/opencritic-resilience-plan.md`. The scope was agreed in chat, so
Step 1 follows straight on.

## Step 1 — Resilience handler on the OpenCritic client

- Add `Microsoft.Extensions.Http.Resilience` to `FantasyCritic.Hosting`.
- In `AddFantasyCriticAdminServices`, chain `.AddStandardResilienceHandler(...)` onto the
  OpenCritic `AddHttpClient`:
  - attempt timeout 10s (normal responses take well under 1s),
  - total request timeout 60s, enough for all four attempts plus backoff and still under the
    100s `HttpClient.Timeout`,
  - retry, circuit breaker and rate limiter left at the standard defaults (3 retries, exponential
    backoff with jitter, honours `Retry-After`; retries timeouts, `HttpRequestException`, 408,
    429 and 5xx; 404 is not retried, so `OpenCriticService`'s 404 → null is unchanged).
- A timed-out attempt's connection is discarded, so the retry goes out on a fresh one.
- Polly logs each retry at Warning under the `Polly` category, so Loki will show whether retries
  are happening and how often.

Commit → review.

## Step 2 — Cancellation through `GetOpenCriticGame`

- `IOpenCriticService.GetOpenCriticGame(int openCriticGameID, CancellationToken cancellationToken)`;
  `OpenCriticService` passes it to `GetFromJsonAsync`.
- A cancellation the caller asked for is rethrown without the "Getting an open critic game failed"
  error log.
- `CriticScoreRefresher` passes its token for both the game and sub-game fetches, so stopping the
  job interrupts a request in flight.

Commit → review.
