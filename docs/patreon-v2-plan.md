# Patreon: our own API client, and a v2 OAuth client

## Context

The hourly `RefreshPatreonInfo` job started failing in production after the job-system deploy with
`PatreonApiException: Unauthorized`. It was not the deploy.

- **The job's credentials are in the database, not config.** `PatreonService` builds the Patreon.Net
  client from the newest row of `tbl_system_patreonkeys` (a creator access and refresh token). From
  config it takes only the client ID and the campaign ID.
- **Refreshing has worked monthly until now.** On a 401, Patreon.Net refreshes and
  `TokensRefreshedAsync` saves a new row. Prod had one row per ~31 days (rows 44–47, May 23 → Aug 24,
  2026), each written at about 00:00–00:01. Row 47's access token expired around Sep 24. The refresh
  failed, so there is no row 48.
- **Patreon.Net's refresh request doesn't match Patreon's documented contract.** It POSTs
  `/api/oauth2/token?grant_type=refresh_token&refresh_token=…&client_id=…` as a query string, with no
  `client_secret`. Patreon documents a form body that includes the secret, and says both returned
  tokens are single use. Patreon.Net also throws away the refresh response, so we can't see why it
  failed.
- **Why Sep 24 failed is unconfirmed.** There are two candidates:
  - Beta used the shared refresh token first (beta restores from prod snapshots, so it held row 47 too).
  - Patreon started enforcing the documented refresh form. Its 2026-08-07 changelog retires the v1
    API, so it was doing auth work around then.
- Steve fixed prod on 2026-09-27 by copying the creator tokens from the Patreon client page into a
  new row. **Those expire around 2026-10-28.** That is the next refresh, and the deadline for this
  code to be live.

### v1 vs v2

- **The portal's "Client Version: 1" is only the client's registration type.** Everything we call is
  already API v2:
  - The job uses `/api/oauth2/v2/campaigns/{id}/members`.
  - The login (AspNet.Security.OAuth.Patreon 10.0.0) uses `/api/oauth2/v2/identity` with scope
    `identity`.
  - Patreon's docs say a client's creator access token "will automatically have all V2 scopes".
- **The v1 shutdown shouldn't affect us.** Patreon's 2026-10-07 shutdown is of v1 *endpoints*
  (`/api/oauth2/api/...`), which we don't call. Patreon notes that registration type doesn't decide
  which API a client calls.
- **We're relying on Patreon being lenient in two places.** The docs say v2 scopes are "only
  available to v2 clients", yet linking Patreon works today on the v1 client. That, plus the
  non-standard refresh, is enough reason to move.
- **New v1 clients can't be made** (since 2026-03-25), so any replacement client will be v2.

### Why write our own client

No package on NuGet refreshes correctly and is maintained:
- Patreon.Net: last release 2025-01, net5/6, and the refresh above.
- Patreon.Client (Agash): the only active .NET 10 package, but a single-author prerelease with no
  token refresh at all.
- Patreon (T0shik): dormant since 2023.
- Patreon has never published a .NET SDK.

What we use is small: one endpoint with cursor paging, and one token POST. Writing it ourselves is
about 150–200 lines. Login stays on AspNet.Security.OAuth.Patreon.

Sources: https://docs.patreon.com/ (the refresh step, the v2 scopes, and the changelog entries of
2026-03-25 and 2026-08-07).

## Decisions

- **`ClientSecret` joins `PatreonOptions`.** Both hosts need it now, so `PatreonAuthOptions`, which
  existed only to add it for Web, goes away. All hosts read the one `fantasyCritic/<env>/appsettings`
  secret, so the Worker already has the value in beta and prod. Only its `appsettings.json` gains the
  key.
- **Split HTTP from token handling.** `PatreonApiClient` is a typed `HttpClient` that doesn't store
  tokens: it gets members given an access token, and refreshes given a refresh token.
  `PatreonService` keeps the token flow: read the newest row, call, and on a 401 refresh once, save
  the new pair, and retry. That matches today's behaviour.
- **Refreshing stays 401-driven.** We don't store `expires_in`, since the table has no column for it
  and the 401 already tells us.
- **Fail loudly.** A failed refresh throws with Patreon's status code and response body. There's no
  catch-and-continue.
- **Refresh tokens go in the POST body only.** They never go in a URL, where logs could pick them up.
- **No Patreon on beta.** External logins are already Production-only (`HostingExtensions.cs`), so
  the job is the only Patreon code beta runs. The beta/local cleaner:
  - sets `RefreshPatreonInfo`'s `tbl_job_type.RunType` to `Disabled`, the job system's own off
    switch, which blocks cron and manual runs alike;
  - deletes `tbl_system_patreonkeys`, so beta never holds prod's live refresh token at all.
  Beta keeps no Patreon client and no Patreon secrets.
- **Webhooks are out of scope.** Patreon recommends `members:*` webhooks with polling as a fallback.
  We can revisit later.

## How we work through this

**Each step ends the same way: build and relevant tests green, commit that step alone, then STOP and
review with Steve before starting the next.** Feedback may change later steps; update this plan when
it does.

## Step 0 — Commit the plan

Commit this file. → review.

## Step 1 — `ClientSecret` in `PatreonOptions`

- `Lib/Configuration/SectionOptions.cs`:
  - `PatreonOptions` gains `ClientSecret`, required from Production like `ClientId`.
  - `PatreonAuthOptions` is deleted, and `AuthenticationOptions.Patreon` becomes `PatreonOptions`.
  - `PatreonOptions` becomes `sealed`, and its `Validate` is no longer virtual.
- `ServiceCollectionExtensions.AddFantasyCriticPatreon`: drop the "as the base type" comment.
- `Worker/appsettings.json`: add `Authentication:Patreon:ClientSecret: "secret"`.
- Tests:
  - `HostOptionsTests`: update the Worker's missing-keys message.
  - `ConfigurationSectionTests`: the Worker binding test expects `ClientSecret` too, and the Web
    Patreon test binds `PatreonOptions`.
  - `ServiceRegistrationTests`: update `AdminServices_RegisterWebsPatreonLoginOptions…`, which may
    become redundant.
- No secret changes: the value is already in both environments' blobs.

→ commit → review.

## Step 2 — `PatreonApiClient` replaces Patreon.Net

- **`Lib/Patreon/PatreonApiClient.cs`**, a typed `HttpClient` with base `https://www.patreon.com/`
  (the `www.` host matters: Patreon has returned 401s on the bare host). It has two methods:
  - `GetCampaignMembers(string accessToken, string campaignId)`:
    - GET `api/oauth2/v2/campaigns/{id}/members` with
      `include=currently_entitled_tiers,user&fields[tier]=title&fields[user]=full_name&page[count]=1000`,
      following `links.next` until it is absent.
    - Returns `Result<IReadOnlyList<PatreonMember>, PatreonApiError>`, so a 401 is an expected result
      the caller acts on.
    - Any other non-success status throws, with the status and body.
  - `RefreshTokens(string refreshToken)`:
    - POST `api/oauth2/token` with a `FormUrlEncodedContent` body of `grant_type=refresh_token`,
      `refresh_token`, `client_id` and `client_secret`.
    - Returns the new `PatreonTokens`, or throws with Patreon's status and body.
- **JSON:API response records**, `internal`, next to the client (following
  `OpenCritic/OpenCriticGameResponse.cs`). The client joins `data[].relationships` to `included[]`
  by (type, id). It maps each member to
  `record PatreonMember(string UserId, string? FullName, IReadOnlyList<string> TierTitles)`.
- **`PatreonService`**:
  - Takes `PatreonApiClient`.
  - `GetPatronInfo` and `UserIsPlusUser` share one private `GetMembers()`: read tokens, call, and on a
    401 refresh, save, and retry once.
  - A second 401 after a fresh refresh throws.
  - The Plus/Donor tier logic is unchanged.
- **`MySQLPatreonTokensRepo.GetMostRecentTokens`**: an empty table fails with "No Patreon tokens in
  tbl_system_patreonkeys" rather than a bare `QuerySingleAsync` exception.
- **Registration**: `AddFantasyCriticPatreon` registers `AddHttpClient<PatreonApiClient>`.
- **Remove** the `Patreon.Net` package reference.
- **Unit tests** in `FantasyCritic.Test`, using a stub `HttpMessageHandler`:
  - Paging across two pages.
  - Includes joined correctly (a member with no tiers, a member whose user isn't linked to FC).
  - The refresh body carries `client_secret`.
  - 401 → refresh → retry, with the new tokens saved.
  - A failed refresh throws with the body.

→ commit → review.

## Step 3 — Beta and local cleans turn Patreon off

- `MySQLBetaCleaner` gains `DisablePatreon`, inside the same transaction:
  - `UPDATE tbl_job_type SET RunType = 'Disabled' WHERE Name = 'RefreshPatreonInfo'`.
  - `DELETE FROM tbl_system_patreonkeys`.
- It covers beta restores (`RestoreSnapshotService`) and local imports and cleans, since both use
  this cleaner. Neither should run the job or hold prod's live tokens.
- **A side effect to accept:** the Scheduler logs a "Skipped slot … RunType does not allow cron"
  warning for each hourly slot on beta. That's the Scheduler's existing behaviour for any disabled
  cron job.
- Update the RdsSnapshotManager README and menu text that lists what gets scrubbed.
- **Beta today** (Steve, once): beta was restored before this existed, so set the RunType and
  delete the tokens there by hand, or re-run the restore.

→ commit → review.

## Step 4 — Deploy (Steve), before ~2026-10-28

- **No config changes.**
- **Checking it after deploy**: run `RefreshPatreonInfo` from the admin console. It succeeds on the
  current access token.
- **The real test is around Oct 28**, when the token expires.
  - A new `tbl_system_patreonkeys` row means the documented refresh works.
  - If it fails, the job now shows Patreon's actual response.

## Step 5 — Upgrade the client to v2 (Steve, in the Patreon portal)

The portal's Edit Client form has a **Client API Version** dropdown (currently 1), so the plan is to
upgrade the existing client in place rather than create a new one. That keeps the name, the
description and the one redirect URI (`https://www.fantasycritic.games/signin-patreon`). Patreon
doesn't document what the switch does to the client ID, secret or creator tokens, so check each.

1. **Before switching, note the client ID** and the current creator token prefixes (the
   `tbl_system_patreonkeys` query from the investigation).
2. **Fix the icon while in the form.** `https://www.fantasycritic.games/img/big-logo.png` has returned
   404 since `f9944fa3b` moved the file into `src/assets/`, where Vite renames it with a hash.
3. **Set Client API Version to 2 and save.**
4. **Compare**:
   - If the **client ID or secret changed**, update `Authentication:Patreon:ClientId` / `ClientSecret`
     in `fantasyCritic/Production/appsettings` and restart web and worker.
   - If the **creator tokens changed**, insert the page's new pair as a `tbl_system_patreonkeys` row.
     The old ones are probably revoked.
5. **Verify**:
   - Run `RefreshPatreonInfo`.
   - Link a Patreon account.
   - Check the consent screen shows the logo.
6. **Beta gets nothing** (see Decisions). If beta's secret holds Patreon values, swap them for
   `"secret"` placeholders. Beta's validation doesn't require Patreon keys.

**Fallback** if the dropdown won't save, or the upgrade breaks login: create a new v2 client with the
same fields, update both secrets, and insert its creator tokens. Keep the old client until the new
one has refreshed successfully once.

Step 5 needs no code, apart from the optional icon file. If it turns out it does, it becomes its own
step with a commit.
