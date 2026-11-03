# vee-validate 3 to 4

## Context

`docs/vue3-upgrade-plan.md` left vee-validate for the flip. vee-validate 3 supports only Vue 2;
it installs on Vue 3 only through a `package.json` override, and its `ValidationProvider` finds
its input by walking Vue 2 vnodes. It is the last dependency other than bootstrap-vue that has to
change before `@vue/compat` can go.

The goal is a 1:1 swap to vee-validate 4: same rules, same messages, same disabled submit
buttons. Whether to keep vee-validate at all, or move to another library or our own code, is a
separate decision for later.

The client package lives at `src/FantasyCritic.ClientAppVue2`; paths below are relative to it.

## What exists today

The usage is small and uniform:

- **22 `ValidationProvider`s in 13 files.** Every one wraps a single native `<input v-model>` and
  shows `errors[0]` in a `<span class="text-danger">`. None uses `mode`, `vid`, `immediate`,
  `bails`, `tag` or a ref.
- **7 `ValidationObserver`s.** Five read `invalid` from the slot to disable a submit button
  (`invitePlayerForm`, `masterGameRequest`, `masterGameCreator`, `masterGameEditor`,
  `masterGameChangeRequest`). The two in `bidGameForm` and `bidCounterPickForm` have no slot and
  do nothing. Four of the five already wrap their own `<form @submit.prevent>`. Nothing calls
  `validate()`, `handleSubmit` or `setErrors`.
- **Rules:** `required`, `email`, `min`, `min_value`, `max_value` and `integer`. `max` is
  registered but unused.
- **Messages:** v3's English locale. A message names the field by the provider's `name` prop, or
  else the input's `name` attribute, or else its `id`.

## vee-validate 4 equivalents

| v3 | v4 |
|---|---|
| `extend('required', rules.required)` | `defineRule('required', required)` from `@vee-validate/rules` |
| `localize({ en })` | `configure({ generateMessage: localize({ en }) })` from `@vee-validate/i18n` |
| `<ValidationProvider v-slot="{ errors }" rules="...">` | `<Field v-slot="{ errors }" :model-value="..." name="..." rules="...">` |
| `<ValidationObserver v-slot="{ invalid }">` | `<Form v-slot="{ meta }" as="">`, reading `!meta.valid` |

Checked in the 4.15.1 source:

- **`Field` with a slot and no `as` is renderless.** Binding `:model-value` to the same expression
  as the inner input's `v-model` makes it validate whenever the value changes, with errors shown,
  the way v3 validated on every input. The input keeps its own `v-model` unchanged.
- **`Form` with `as=""` is renderless.** That matters because four of the observers wrap a
  `<form>` already, and nested forms are invalid.
- **Fields validate silently on mount**, so `meta.valid` starts out right without showing errors,
  as v3's `invalid` did.
- **The English messages render the same text as v3's.** Only the placeholder syntax differs:
  v3's `{_field_}` and `{min}` become `{field}` and `0:{min}`.
- **The rules skip empty values except `required`**, as v3's did. So `email` and
  `min_value:1000|max_value:9999` on `invitePlayerForm` still allow an empty field.

## Decisions

- **Register the components globally as `ValidationField` and `ValidationForm`.** That keeps the
  global registration v3 had. It also avoids `Form` and `Field`, which collide with HTML's
  `<form>` under `vue/no-reserved-component-names`.
- **Register them with `compatConfig: { MODE: 3 }`.** vee-validate 4 has no compat opt-out of its
  own (vue-multiselect 3 does), so `@vue/compat` would rewrite `:model-value` into Vue 2's
  `value` prop and break every field. The opt-out goes when compat does.
- **Keep every field's v3 message name**, passed as `name`. That includes the ones v3 got from an
  input attribute: `bidAmount` on the bid forms, and `gamesToDraft-0` / `counterPicksToDraft-0`
  (from the input `id`) in `DraftCreationSettings`. They read badly, but changing them isn't 1:1.
- **Drop the two slot-less observers** in `bidGameForm` and `bidCounterPickForm`, since they did
  nothing. Their fields work standalone.
- **Don't register `max`.** It's unused.
- **The `<span>` that v3's observer rendered around its content goes away**, because `Form` is
  renderless. Nothing styles it.

## Steps

One commit per step, stopping for review after each, per CLAUDE.md. The plan itself gets
committed first.

### 1. Swap the packages and convert every form

This can't be split. Once vee-validate 3 is uninstalled, any form not yet converted stops
rendering its fields.

- `package.json`: remove `vee-validate` 3 and its `overrides` entry. Add `vee-validate`,
  `@vee-validate/rules` and `@vee-validate/i18n`, all 4.15.1.
- `main.js`: `defineRule` the six rules, `configure` the English messages, and register
  `ValidationField` and `ValidationForm` with the compat opt-out.
- The 13 files: each `ValidationProvider` becomes a `ValidationField` with `:model-value` and
  `name`. Each `ValidationObserver` with a slot becomes `<ValidationForm v-slot="{ meta }" as="">`,
  with `invalid` replaced by `!meta.valid`.
- `tests/components/validation.test.ts`: mount a small form with the global setup, and check:
  - a `required` error appears after the value is cleared, but not on mount
  - the message text for each rule
  - `meta.valid` follows the fields
  - an empty optional `email` field is valid
- Check `npm test`, lint, format and a production build.

### 2. Check every form in the running app

Most of these pages need a login, so Steve clicks through them. For each form: type an invalid
value, see the message, fix it, and check the submit button where there is one.

- Create League and Edit League: league name, the `leagueYearSettings` number fields, the
  per-draft fields in `DraftCreationSettings`, and the player-count preset.
- Create Conference: conference and primary league names.
- Bid, counter-pick bid and current bids modals: bid amount.
- Invite Player modal: email and display number, and the Send Invite button. This one carries
  the most risk. Its `ValidationForm` wraps a `b-modal`, whose content bootstrap-vue moves into a
  portal, and the fields have to reach the form through that. If they don't, the button never
  disables. The fallback is to move the `ValidationForm` inside the modal.
- Master game request, creator, editor and change request: required fields and the submit
  buttons.

Then update the vee-validate row in `docs/vue3-upgrade-plan.md`.
