# Multi-Draft League UI Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Build the frontend UI so commissioners can add/edit/run multiple drafts, all players can see draft schedules, and the league page surfaces the right readiness state for whichever draft is next.

**Architecture:** Three new Vue components (`manageDrafts.vue` page, `draftReadinessBanner.vue`, `draftCompleteModal.vue`, `draftScheduleModal.vue`); `leagueActions.vue` gains two new sidebar links; `league.vue` replaces its inline draft-readiness blocks with the new components; `leagueYearSettings.vue` gains an `isMultiDraft` prop that hides the per-draft game count fields; `editLeague.vue` passes the new prop. All backend APIs (`CreateLeagueDraft`, `EditLeagueDraft`, `DeleteLeagueDraft`, `SetDraftOrder`, `ResetDraft`) are already implemented. The `editDraftOrderForm` and `resetDraftModal` already send `draftID` — no changes needed there.

**Tech Stack:** Vue 2, Bootstrap-Vue, Axios, Vue Router; all in `src/FantasyCritic.Web/ClientApp/src/`

---

## Task 1: Add `manageDrafts` Route

**Files:**
- Modify: `src/FantasyCritic.Web/ClientApp/src/router/routes.js`

- [ ] **Step 1: Add the import and route entry**

At the top of `routes.js`, add the import alongside the existing view imports:
```js
import ManageDrafts from '@/views/manageDrafts.vue';
```

Then add this route object in the routes array (place it near the `editLeague` route for logical grouping):
```js
{
  path: '/manageDrafts/:leagueid/:year',
  component: ManageDrafts,
  name: 'manageDrafts',
  meta: {
    title: 'Manage Drafts'
  },
  props: (route) => {
    let parsedYear = Number.parseInt(route.params.year, 10);
    if (Number.isNaN(parsedYear)) {
      parsedYear = 0;
    }
    return {
      leagueid: route.params.leagueid,
      year: parsedYear
    };
  }
},
```

- [ ] **Step 2: Create a placeholder view so the route resolves**

Create `src/FantasyCritic.Web/ClientApp/src/views/manageDrafts.vue` with the minimum needed to compile:
```vue
<template>
  <div class="col-md-10 offset-md-1 col-sm-12">
    <h1>Manage Drafts</h1>
    <p>Coming soon.</p>
  </div>
</template>
<script>
export default {
  props: {
    leagueid: { type: String, required: true },
    year: { type: Number, required: true }
  }
};
</script>
```

- [ ] **Step 3: Verify the app compiles**

Run the dev server and navigate to `/manageDrafts/<any-league-id>/<any-year>`. The placeholder page should render without console errors.

- [ ] **Step 4: Commit**
```
git add src/FantasyCritic.Web/ClientApp/src/router/routes.js src/FantasyCritic.Web/ClientApp/src/views/manageDrafts.vue
git commit -m "Add manageDrafts route and placeholder view."
```

---

## Task 2: Manage Drafts Page — Draft Card Read View

**Files:**
- Modify: `src/FantasyCritic.Web/ClientApp/src/views/manageDrafts.vue`

- [ ] **Step 1: Replace the placeholder with the full shell**

Replace the entire file contents with:
```vue
<template>
  <div class="col-md-10 offset-md-1 col-sm-12">
    <div v-if="errorInfo" class="alert alert-danger">{{ errorInfo }}</div>

    <div v-if="leagueYear">
      <h1>Manage Drafts — {{ leagueYear.settings.leagueYearName || leagueYear.league.leagueName }} {{ year }}</h1>
      <router-link :to="{ name: 'league', params: { leagueid: leagueid, year: year } }">← Back to League</router-link>
      <hr />

      <div v-if="!isManager" class="alert alert-danger">You do not have permission to manage drafts for this league.</div>

      <template v-else>
        <div v-for="draft in leagueYear.drafts" :key="draft.draftID" class="mb-3">
          <b-card>
            <div class="draft-card-header">
              <span class="h5">Draft {{ draft.draftNumber }}: {{ draft.name }}</span>
              <b-badge :variant="statusVariant(draft)" class="ml-2">{{ statusLabel(draft) }}</b-badge>
            </div>
            <div class="mt-2">
              <div><strong>Scheduled Date:</strong> {{ draft.scheduledDate || '—' }}</div>
              <div><strong>Games to Draft:</strong> {{ draft.gamesToDraft }}</div>
              <div><strong>Counter Picks to Draft:</strong> {{ draft.counterPicksToDraft }}</div>
            </div>
          </b-card>
        </div>
      </template>
    </div>

    <div v-else class="text-center mt-4">
      <b-spinner></b-spinner>
    </div>
  </div>
</template>
<script>
import axios from 'axios';

export default {
  props: {
    leagueid: { type: String, required: true },
    year: { type: Number, required: true }
  },
  data() {
    return {
      leagueYear: null,
      errorInfo: null
    };
  },
  computed: {
    isManager() {
      return this.leagueYear?.league?.isManager ?? false;
    }
  },
  async created() {
    await this.fetchLeagueYear();
  },
  methods: {
    async fetchLeagueYear() {
      try {
        const response = await axios.get(`/api/League/GetLeagueYear?leagueID=${this.leagueid}&year=${this.year}`);
        this.leagueYear = response.data;
      } catch {
        this.errorInfo = 'Failed to load league data.';
      }
    },
    statusLabel(draft) {
      const map = {
        NotStartedDraft: 'Not Started',
        Drafting: 'In Progress',
        DraftPaused: 'Paused',
        DraftFinal: 'Completed'
      };
      return map[draft.playStatus] ?? draft.playStatus;
    },
    statusVariant(draft) {
      const map = {
        NotStartedDraft: 'secondary',
        Drafting: 'success',
        DraftPaused: 'warning',
        DraftFinal: 'info'
      };
      return map[draft.playStatus] ?? 'secondary';
    }
  }
};
</script>
```

- [ ] **Step 2: Test manually**

Navigate to `/manageDrafts/<league-id>/<year>` for a league you manage. You should see all existing drafts listed as cards with their status badges. Verify a non-commissioner gets the access denied message.

- [ ] **Step 3: Commit**
```
git add src/FantasyCritic.Web/ClientApp/src/views/manageDrafts.vue
git commit -m "Manage Drafts page: draft card read view."
```

---

## Task 3: Manage Drafts Page — Edit Draft Inline Form

**Files:**
- Modify: `src/FantasyCritic.Web/ClientApp/src/views/manageDrafts.vue`

- [ ] **Step 1: Add edit state data fields**

In the `data()` function, add:
```js
editingDraftId: null,
editForm: {
  name: '',
  scheduledDate: null,
  gamesToDraft: 1,
  counterPicksToDraft: 0
},
editError: null
```

- [ ] **Step 2: Add edit methods**

In `methods`, add:
```js
startEdit(draft) {
  this.editingDraftId = draft.draftID;
  this.editForm = {
    name: draft.name,
    scheduledDate: draft.scheduledDate,
    gamesToDraft: draft.gamesToDraft,
    counterPicksToDraft: draft.counterPicksToDraft
  };
  this.deletingDraftId = null;
  this.editError = null;
},
cancelEdit() {
  this.editingDraftId = null;
  this.editError = null;
},
async submitEdit(draft) {
  const model = {
    draftID: draft.draftID,
    leagueID: this.leagueid,
    year: this.year,
    name: this.editForm.name,
    scheduledDate: this.editForm.scheduledDate || null,
    gamesToDraft: this.editForm.gamesToDraft,
    counterPicksToDraft: this.editForm.counterPicksToDraft
  };
  try {
    await axios.post('/api/leagueManager/EditLeagueDraft', model);
    this.editingDraftId = null;
    this.editError = null;
    await this.fetchLeagueYear();
  } catch (error) {
    this.editError = error.response?.data || 'An error occurred saving the draft.';
  }
}
```

- [ ] **Step 3: Replace the draft card body with toggle between read and edit mode**

In the template, replace the draft card contents with:
```html
<b-card v-for="draft in leagueYear.drafts" :key="draft.draftID" class="mb-3">
  <div class="draft-card-header">
    <span class="h5">Draft {{ draft.draftNumber }}: {{ draft.name }}</span>
    <b-badge :variant="statusVariant(draft)" class="ml-2">{{ statusLabel(draft) }}</b-badge>
  </div>

  <!-- Read view -->
  <div v-if="editingDraftId !== draft.draftID" class="mt-2">
    <div><strong>Scheduled Date:</strong> {{ draft.scheduledDate || '—' }}</div>
    <div><strong>Games to Draft:</strong> {{ draft.gamesToDraft }}</div>
    <div><strong>Counter Picks to Draft:</strong> {{ draft.counterPicksToDraft }}</div>
    <div class="mt-2">
      <b-button
        v-if="editingDraftId === null"
        size="sm"
        variant="secondary"
        @click="startEdit(draft)">
        Edit
      </b-button>
    </div>
  </div>

  <!-- Edit form -->
  <div v-else class="mt-2">
    <div v-if="editError" class="alert alert-danger">{{ editError }}</div>
    <div class="form-group">
      <label>Draft Name</label>
      <input v-model="editForm.name" type="text" class="form-control" />
    </div>
    <div class="form-group">
      <label>Scheduled Date (optional)</label>
      <input v-model="editForm.scheduledDate" type="date" class="form-control" />
    </div>
    <div class="form-group">
      <label>Games to Draft</label>
      <input
        v-model.number="editForm.gamesToDraft"
        type="number"
        class="form-control"
        :disabled="draft.playStatus !== 'NotStartedDraft'" />
      <small v-if="draft.playStatus !== 'NotStartedDraft'" class="text-muted">Cannot change after draft has started.</small>
    </div>
    <div class="form-group">
      <label>Counter Picks to Draft</label>
      <input
        v-model.number="editForm.counterPicksToDraft"
        type="number"
        class="form-control"
        :disabled="draft.playStatus !== 'NotStartedDraft'" />
    </div>
    <b-button size="sm" variant="primary" @click="submitEdit(draft)">Save</b-button>
    <b-button size="sm" variant="secondary" class="ml-2" @click="cancelEdit()">Cancel</b-button>
  </div>
</b-card>
```

- [ ] **Step 4: Test manually**

- Click Edit on any draft card. The inline form should appear with current values pre-filled.
- Change the name on any draft (started or not) and save. Verify the card updates.
- On a started draft, verify Games to Draft is disabled.
- Click Cancel. Verify no changes are made.

- [ ] **Step 5: Commit**
```
git add src/FantasyCritic.Web/ClientApp/src/views/manageDrafts.vue
git commit -m "Manage Drafts page: inline edit form for drafts."
```

---

## Task 4: Manage Drafts Page — Delete Draft

**Files:**
- Modify: `src/FantasyCritic.Web/ClientApp/src/views/manageDrafts.vue`

- [ ] **Step 1: Add delete state data field**

In `data()`, add:
```js
deletingDraftId: null,
deleteError: null
```

- [ ] **Step 2: Add delete methods**

In `methods`, add:
```js
confirmDelete(draft) {
  this.deletingDraftId = draft.draftID;
  this.editingDraftId = null;
  this.deleteError = null;
},
cancelDelete() {
  this.deletingDraftId = null;
  this.deleteError = null;
},
async submitDelete(draft) {
  const model = {
    draftID: draft.draftID,
    leagueID: this.leagueid,
    year: this.year
  };
  try {
    await axios.post('/api/leagueManager/DeleteLeagueDraft', model);
    this.deletingDraftId = null;
    await this.fetchLeagueYear();
  } catch (error) {
    this.deleteError = error.response?.data || 'An error occurred deleting the draft.';
  }
}
```

- [ ] **Step 3: Add Delete button and confirmation to the read view section of each card**

Inside the read view `<div v-if="editingDraftId !== draft.draftID">`, after the Edit button, add:
```html
<b-button
  v-if="editingDraftId === null && deletingDraftId === null && draft.draftNumber > 1 && draft.playStatus === 'NotStartedDraft'"
  size="sm"
  variant="danger"
  class="ml-2"
  @click="confirmDelete(draft)">
  Delete
</b-button>

<div v-if="deletingDraftId === draft.draftID" class="alert alert-warning mt-2">
  <div v-if="deleteError" class="alert alert-danger">{{ deleteError }}</div>
  <p>Are you sure you want to delete <strong>{{ draft.name }}</strong>? This cannot be undone.</p>
  <b-button size="sm" variant="danger" @click="submitDelete(draft)">Confirm Delete</b-button>
  <b-button size="sm" variant="secondary" class="ml-2" @click="cancelDelete()">Cancel</b-button>
</div>
```

- [ ] **Step 4: Test manually**

- On a league with 2+ drafts, verify that Draft 1 has no Delete button.
- Verify that a `NotStartedDraft` with `draftNumber > 1` shows a Delete button.
- Click Delete, verify the confirmation appears.
- Confirm. Verify the draft disappears from the list.
- Cancel. Verify no change.

- [ ] **Step 5: Commit**
```
git add src/FantasyCritic.Web/ClientApp/src/views/manageDrafts.vue
git commit -m "Manage Drafts page: delete draft with confirmation."
```

---

## Task 5: Manage Drafts Page — Add Another Draft Form

**Files:**
- Modify: `src/FantasyCritic.Web/ClientApp/src/views/manageDrafts.vue`

- [ ] **Step 1: Add `SpecialGameSlotSelector` import and component registration**

At the top of the `<script>` block, add:
```js
import SpecialGameSlotSelector from '@/components/specialGameSlotSelector.vue';
```

In the `components` option (add the `components` key if it doesn't exist yet):
```js
components: {
  SpecialGameSlotSelector
},
```

- [ ] **Step 2: Add new draft form state data**

In `data()`, add:
```js
newDraft: {
  name: '',
  scheduledDate: null,
  gamesToDraft: 1,
  counterPicksToDraft: 0,
  additionalStandardGames: 0,
  additionalCounterPicks: 0,
  newSpecialGameSlots: []
},
newDraftError: null
```

- [ ] **Step 3: Add `activeDraft` computed and `submitNewDraft` method**

In `computed`, add:
```js
activeDraft() {
  return this.leagueYear?.drafts?.find((d) => d.draftIsActive || d.draftIsPaused) ?? null;
}
```

In `methods`, add:
```js
resetNewDraftForm() {
  this.newDraft = {
    name: '',
    scheduledDate: null,
    gamesToDraft: 1,
    counterPicksToDraft: 0,
    additionalStandardGames: 0,
    additionalCounterPicks: 0,
    newSpecialGameSlots: []
  };
  this.newDraftError = null;
},
async submitNewDraft() {
  const model = {
    leagueID: this.leagueid,
    year: this.year,
    name: this.newDraft.name,
    scheduledDate: this.newDraft.scheduledDate || null,
    gamesToDraft: this.newDraft.gamesToDraft,
    counterPicksToDraft: this.newDraft.counterPicksToDraft,
    additionalStandardGames: this.newDraft.additionalStandardGames,
    additionalCounterPicks: this.newDraft.additionalCounterPicks,
    newSpecialGameSlots: this.newDraft.newSpecialGameSlots
  };
  try {
    await axios.post('/api/leagueManager/CreateLeagueDraft', model);
    this.resetNewDraftForm();
    await this.fetchLeagueYear();
  } catch (error) {
    this.newDraftError = error.response?.data || 'An error occurred adding the draft.';
  }
}
```

- [ ] **Step 4: Add the "Add Another Draft" section to the template**

After the closing `</div>` of the draft cards loop (and still inside the `<template v-else>` commissioner block), add:
```html
<hr />

<div v-if="!activeDraft">
  <h3>Add Another Draft</h3>
  <div v-if="newDraftError" class="alert alert-danger">{{ newDraftError }}</div>
  <b-card>
    <div class="form-group">
      <label>Draft Name</label>
      <input v-model="newDraft.name" type="text" class="form-control" placeholder="e.g. Summer Draft" />
    </div>
    <div class="form-group">
      <label>Scheduled Date (optional)</label>
      <input v-model="newDraft.scheduledDate" type="date" class="form-control" />
    </div>
    <div class="form-group">
      <label>Games to Draft</label>
      <input v-model.number="newDraft.gamesToDraft" type="number" min="0" class="form-control" />
    </div>
    <div class="form-group">
      <label>Counter Picks to Draft</label>
      <input v-model.number="newDraft.counterPicksToDraft" type="number" min="0" class="form-control" />
    </div>
    <div class="form-group">
      <label>Additional Standard Games</label>
      <p class="form-text text-muted">Expand the total roster size to make room for this draft's picks. Set to 0 if existing slots are already available.</p>
      <input v-model.number="newDraft.additionalStandardGames" type="number" min="0" class="form-control" />
    </div>
    <div class="form-group">
      <label>Additional Counter Picks</label>
      <input v-model.number="newDraft.additionalCounterPicks" type="number" min="0" class="form-control" />
    </div>
    <div class="form-group">
      <label>New Special Slots (optional)</label>
      <specialGameSlotSelector v-model="newDraft.newSpecialGameSlots"></specialGameSlotSelector>
    </div>
    <b-button variant="success" :disabled="!newDraft.name" @click="submitNewDraft()">Add Draft</b-button>
  </b-card>
</div>
<div v-else class="alert alert-info">
  You cannot add a draft while one is in progress.
</div>
```

- [ ] **Step 5: Test manually**

- On a single-draft league, fill in the form and submit. Verify a second draft card appears.
- Verify `additionalStandardGames > 0` expands the league's total slots (check the league year settings after saving).
- Add a special slot and verify it persists on the new draft.
- While a draft is in progress, verify the form is hidden and the info message is shown.

- [ ] **Step 6: Commit**
```
git add src/FantasyCritic.Web/ClientApp/src/views/manageDrafts.vue
git commit -m "Manage Drafts page: add another draft form with slot expansion."
```

---

## Task 6: Add "Manage Drafts" Link to LeagueActions Sidebar

**Files:**
- Modify: `src/FantasyCritic.Web/ClientApp/src/components/leagueActions.vue`

- [ ] **Step 1: Add the router-link in the commissioner League Management section**

In `leagueActions.vue`, find the `<strong>League Management</strong>` section inside the commissioner block. Add this item after the "Change Year-Specific Options" link:

```html
<li v-if="!leagueYear.supportedYear.finished" class="fake-link action">
  <router-link :to="{ name: 'manageDrafts', params: { leagueid: league.leagueID, year: leagueYear.year } }">Manage Drafts</router-link>
</li>
```

- [ ] **Step 2: Test manually**

Log in as a commissioner. In the league actions sidebar, "Manage Drafts" should appear under League Management. Click it — it should navigate to the Manage Drafts page. Verify it does not appear for non-commissioners.

- [ ] **Step 3: Commit**
```
git add src/FantasyCritic.Web/ClientApp/src/components/leagueActions.vue
git commit -m "LeagueActions: add Manage Drafts link for commissioners."
```

---

## Task 7: Create `draftScheduleModal.vue`

**Files:**
- Create: `src/FantasyCritic.Web/ClientApp/src/components/modals/draftScheduleModal.vue`

- [ ] **Step 1: Create the component**

```vue
<template>
  <b-modal id="draftScheduleModal" title="Draft Schedule" ok-only size="lg">
    <b-table :items="leagueYear.drafts" :fields="fields" striped bordered>
      <template #cell(playStatus)="data">
        <b-badge :variant="statusVariant(data.item)">{{ statusLabel(data.item) }}</b-badge>
      </template>
      <template #cell(scheduledDate)="data">
        {{ data.value || '—' }}
      </template>
      <template #cell(gamesAndCPs)="data">
        {{ data.item.gamesToDraft }} / {{ data.item.counterPicksToDraft }}
      </template>
    </b-table>
  </b-modal>
</template>
<script>
import LeagueMixin from '@/mixins/leagueMixin.js';

export default {
  mixins: [LeagueMixin],
  data() {
    return {
      fields: [
        { key: 'draftNumber', label: '#' },
        { key: 'name', label: 'Name' },
        { key: 'scheduledDate', label: 'Scheduled Date' },
        { key: 'gamesAndCPs', label: 'Games / CPs' },
        { key: 'playStatus', label: 'Status' }
      ]
    };
  },
  methods: {
    statusLabel(draft) {
      const map = {
        NotStartedDraft: 'Not Started',
        Drafting: 'In Progress',
        DraftPaused: 'Paused',
        DraftFinal: 'Completed'
      };
      return map[draft.playStatus] ?? draft.playStatus;
    },
    statusVariant(draft) {
      const map = {
        NotStartedDraft: 'secondary',
        Drafting: 'success',
        DraftPaused: 'warning',
        DraftFinal: 'info'
      };
      return map[draft.playStatus] ?? 'secondary';
    }
  }
};
</script>
```

- [ ] **Step 2: Commit**
```
git add src/FantasyCritic.Web/ClientApp/src/components/modals/draftScheduleModal.vue
git commit -m "Add draftScheduleModal read-only draft list."
```

---

## Task 8: Add "See Draft Schedule" to LeagueActions + Wire Modal into LeagueActions

**Files:**
- Modify: `src/FantasyCritic.Web/ClientApp/src/components/leagueActions.vue`

- [ ] **Step 1: Import and register `draftScheduleModal`**

At the top of the `<script>` block in `leagueActions.vue`, add the import:
```js
import DraftScheduleModal from '@/components/modals/draftScheduleModal.vue';
```

Add it to `components`:
```js
DraftScheduleModal,
```

- [ ] **Step 2: Add the sidebar link under "Public Actions"**

Find the `<ul class="actions-list">` under the `<h4>Public Actions</h4>` heading. Add this item:
```html
<li v-if="leagueYear.drafts.length >= 2" v-b-modal="'draftScheduleModal'" class="fake-link action">See Draft Schedule</li>
```

- [ ] **Step 3: Add the modal to the modal section**

At the bottom of `leagueActions.vue` in the modals `<div>`, add:
```html
<draftScheduleModal></draftScheduleModal>
```

- [ ] **Step 4: Test manually**

- On a single-draft league: "See Draft Schedule" should not appear.
- On a multi-draft league: "See Draft Schedule" appears under Public Actions. Click it — a modal opens listing all drafts in a table.

- [ ] **Step 5: Commit**
```
git add src/FantasyCritic.Web/ClientApp/src/components/leagueActions.vue
git commit -m "LeagueActions: See Draft Schedule link and modal for multi-draft leagues."
```

---

## Task 9: Create `draftReadinessBanner.vue`

**Files:**
- Create: `src/FantasyCritic.Web/ClientApp/src/components/draftReadinessBanner.vue`

- [ ] **Step 1: Create the component**

```vue
<template>
  <div v-if="pendingDraft">
    <!-- Main status banner: warning if not ready, success if ready -->
    <div :class="bannerClass" role="alert">
      <!-- Not ready -->
      <template v-if="!pendingDraft.readyToDraft">
        <h2 v-if="isFirstDraft">This year is not active yet!</h2>
        <h4 v-else>Your next draft — <strong>{{ pendingDraft.name }}</strong> — isn't ready to start yet.</h4>
        <ul>
          <li v-for="error in pendingDraft.startDraftErrors" :key="error">{{ error }}</li>
        </ul>
        <p v-if="isFirstDraft">Please note that once you start the draft, you can no longer add/remove players. Please make sure that everyone who wants to play this year joins beforehand.</p>
        <b-button v-if="mustSetDraftOrder" v-b-modal="'editDraftOrderForm'" variant="success">Set Draft Order</b-button>
        <router-link v-if="!isFirstDraft && isManager" :to="manageDraftsRoute" class="btn btn-secondary btn-sm ml-2">Manage Drafts</router-link>
      </template>

      <!-- Ready to start -->
      <template v-if="pendingDraft.readyToDraft && !league.outstandingInvite">
        <template v-if="isManager">
          <p v-if="isFirstDraft">Things are all set to get started!</p>
          <p v-else><strong>{{ pendingDraft.name }}</strong> is ready to go!</p>
          <p v-if="isFirstDraft">Please note that once you start the draft, you can no longer add/remove players. Please make sure that everyone who wants to play this year joins beforehand.</p>
          <b-button v-b-modal="'startDraft'" variant="primary" class="mx-2">Start Drafting!</b-button>
        </template>
        <template v-else>
          <span v-if="isFirstDraft">Things are all set! Your league manager can choose when to begin the draft.</span>
          <span v-else><strong>{{ pendingDraft.name }}</strong> is ready to go! Your league manager can choose when to begin.</span>
        </template>
      </template>

      <!-- Imminent / scheduled line — shown inside the banner regardless of ready/not-ready -->
      <div v-if="isImminent" class="mt-2">
        <em v-if="pendingDraft.draftOrderSet && scheduledDateDisplay">Scheduled for {{ scheduledDateDisplay }} — draft order is set.</em>
        <em v-else-if="pendingDraft.draftOrderSet">Draft order is set.</em>
        <em v-else-if="scheduledDateDisplay">Scheduled for {{ scheduledDateDisplay }}.</em>
      </div>
    </div>

    <!-- Soft nudge: no scheduled date and not yet imminent -->
    <div v-if="!pendingDraft.scheduledDate && !isImminent" class="alert alert-secondary">
      No scheduled date set —
      <router-link :to="manageDraftsRoute">set one on the Manage Drafts page</router-link>
      to help your players plan ahead.
    </div>
  </div>
</template>
<script>
import LeagueMixin from '@/mixins/leagueMixin.js';

export default {
  mixins: [LeagueMixin],
  computed: {
    bannerClass() {
      return this.pendingDraft?.readyToDraft ? 'alert alert-success' : 'alert alert-warning';
    },
    isFirstDraft() {
      return this.pendingDraft?.draftNumber === 1;
    },
    mustSetDraftOrder() {
      return (
        (this.pendingDraft?.readyToSetDraftOrder ?? false) &&
        (this.pendingDraft?.startDraftErrors ?? []).includes('You must set the draft order.')
      );
    },
    isImminent() {
      if (!this.pendingDraft) return false;
      if (this.pendingDraft.draftOrderSet) return true;
      if (!this.pendingDraft.scheduledDate) return false;
      // Parse YYYY-MM-DD without timezone conversion
      const parts = this.pendingDraft.scheduledDate.split('-');
      const scheduled = new Date(parseInt(parts[0]), parseInt(parts[1]) - 1, parseInt(parts[2]));
      const today = new Date();
      today.setHours(0, 0, 0, 0);
      return (scheduled - today) / (1000 * 60 * 60 * 24) <= 7;
    },
    scheduledDateDisplay() {
      if (!this.pendingDraft?.scheduledDate) return null;
      const parts = this.pendingDraft.scheduledDate.split('-');
      const date = new Date(parseInt(parts[0]), parseInt(parts[1]) - 1, parseInt(parts[2]));
      return date.toLocaleDateString('en-US', { month: 'long', day: 'numeric', year: 'numeric' });
    },
    manageDraftsRoute() {
      return { name: 'manageDrafts', params: { leagueid: this.league.leagueID, year: this.leagueYear.year } };
    }
  }
};
</script>
```

- [ ] **Step 2: Commit**
```
git add src/FantasyCritic.Web/ClientApp/src/components/draftReadinessBanner.vue
git commit -m "Add draftReadinessBanner component."
```

---

## Task 10: Wire `draftReadinessBanner` into `league.vue`

**Files:**
- Modify: `src/FantasyCritic.Web/ClientApp/src/views/league.vue`

- [ ] **Step 1: Import and register the component**

At the top of the `<script>` block in `league.vue`, add:
```js
import DraftReadinessBanner from '@/components/draftReadinessBanner.vue';
```

Add to `components`:
```js
DraftReadinessBanner,
```

- [ ] **Step 2: Replace the two inline draft-readiness blocks with the component**

Remove these two blocks from `league.vue` (approximately lines 120–151 in the original):

```html
<div v-if="(leagueYear.userIsActive || league.isManager) && !firstDraft.playStarted && !firstDraft.readyToDraft" class="alert alert-warning">
  ...
</div>

<div v-if="!firstDraft.playStarted && firstDraft.readyToDraft && !league.outstandingInvite">
  ...
  <startDraftModal @draftStarted="startDraft"></startDraftModal>
</div>
```

Replace them with a single line:
```html
<draftReadinessBanner></draftReadinessBanner>
```

Keep the `<startDraftModal @draftStarted="startDraft"></startDraftModal>` — move it to the modals section at the bottom of the template (if it isn't already there) so it stays registered in this scope.

- [ ] **Step 3: Test manually**

Test each state:
- League with no draft order set and missing publishers: warning banner with error list.
- League ready to start (draft order set, publishers present): success banner with "Start Drafting!" for manager, informational text for player.
- League with scheduled date within 7 days: "Scheduled for [DATE]" appears in the banner.
- League with draft order set but no date: "Draft order is set." appears.
- Second draft pending: banner uses draft name, shows "Manage Drafts" link for commissioner.
- No scheduled date: soft nudge with Manage Drafts link below banner.

- [ ] **Step 4: Commit**
```
git add src/FantasyCritic.Web/ClientApp/src/views/league.vue
git commit -m "league.vue: replace inline draft-readiness blocks with draftReadinessBanner."
```

---

## Task 11: Fix `startHubConnection` in `league.vue`

**Files:**
- Modify: `src/FantasyCritic.Web/ClientApp/src/views/league.vue`

- [ ] **Step 1: Update the early-return condition**

Find `startHubConnection()` in `league.vue`. Change:
```js
async startHubConnection() {
  if (!this.leagueYear || !this.firstDraft.playStarted || this.firstDraft.draftFinished) {
    return;
  }
```
to:
```js
async startHubConnection() {
  if (!this.leagueYear || !this.activeDraft) {
    return;
  }
```

No other changes to `startHubConnection` are needed.

- [ ] **Step 2: Test manually**

Run a two-draft league. During draft 1, confirm real-time updates work (picks update live). After draft 1 completes and before draft 2 starts, the hub should not be connected (no spurious reconnect loops). Once draft 2 starts, hub connects and real-time picks work again.

- [ ] **Step 3: Commit**
```
git add src/FantasyCritic.Web/ClientApp/src/views/league.vue
git commit -m "league.vue: fix startHubConnection to use activeDraft instead of firstDraft."
```

---

## Task 12: Create `draftCompleteModal.vue`

**Files:**
- Create: `src/FantasyCritic.Web/ClientApp/src/components/modals/draftCompleteModal.vue`

- [ ] **Step 1: Create the component**

The modal branches on: league membership, multi-draft vs single-draft, `isFinalDraft`, and `enableBids`. Prose marked `[Lorem ipsum]` is a placeholder — the commissioner (repo owner) will write the final copy before this ships.

```vue
<template>
  <b-modal id="draftCompleteModal" ref="draftCompleteModalRef" title="Draft Complete!" ok-only>
    <!-- User not in league: short fallback -->
    <template v-if="!league.userIsInLeague">
      <p>The draft is complete!</p>
    </template>

    <!-- Multi-draft: non-final draft -->
    <template v-else-if="isMultiDraft && !isFinalDraft">
      <template v-if="leagueYear.enableBids">
        <p>[Lorem ipsum — draft N done, bids may be open between drafts, next draft coming up.]</p>
      </template>
      <template v-else>
        <p>[Lorem ipsum — draft N done, no bids between drafts, sit tight for the next draft.]</p>
      </template>
    </template>

    <!-- Multi-draft: final draft -->
    <template v-else-if="isMultiDraft && isFinalDraft">
      <template v-if="leagueYear.enableBids">
        <p>[Lorem ipsum — all drafts done, bids now open for the rest of the year.]</p>
      </template>
      <template v-else>
        <p>[Lorem ipsum — all drafts done, rosters locked for the year.]</p>
      </template>
    </template>

    <!-- Single draft, one-shot -->
    <template v-else-if="oneShotMode">
      <p>The draft is complete!</p>
    </template>

    <!-- Single draft, bids enabled -->
    <template v-else-if="leagueYear.enableBids">
      <p>The draft is complete! From here you can make bids for games that were not drafted, however, you may want to hold onto your available budget until later in the year!</p>
    </template>

    <!-- Single draft, bids disabled -->
    <template v-else>
      <p>The draft is complete!</p>
    </template>
  </b-modal>
</template>
<script>
import LeagueMixin from '@/mixins/leagueMixin.js';

export default {
  mixins: [LeagueMixin],
  computed: {
    isMultiDraft() {
      return (this.leagueYear?.drafts?.length ?? 0) >= 2;
    },
    isFinalDraft() {
      return this.pendingDraft === null;
    }
  }
};
</script>
```

- [ ] **Step 2: Commit**
```
git add src/FantasyCritic.Web/ClientApp/src/components/modals/draftCompleteModal.vue
git commit -m "Add draftCompleteModal with league-shape-aware text cases."
```

---

## Task 13: Wire `draftCompleteModal` into `league.vue`

**Files:**
- Modify: `src/FantasyCritic.Web/ClientApp/src/views/league.vue`

- [ ] **Step 1: Import and register**

Add the import to `league.vue`:
```js
import DraftCompleteModal from '@/components/modals/draftCompleteModal.vue';
```

Add to `components`:
```js
DraftCompleteModal,
```

- [ ] **Step 2: Replace the inline modal**

Find the inline `<b-modal>` in `league.vue`:
```html
<b-modal id="draftFinishedModal" ref="draftFinishedModalRef" title="Draft Complete!" ok-only>
  <p v-if="!league.userIsInLeague || oneShotMode">The draft is complete!</p>
  <p v-else>The draft is complete! From here you can make bids for games that were not drafted, however, you may want to hold onto your available budget until later in the year!</p>
</b-modal>
```

Remove it and add the new component in the modals section:
```html
<draftCompleteModal ref="draftCompleteModalRef"></draftCompleteModal>
```

- [ ] **Step 3: Update the SignalR `DraftFinished` handler**

Find the hub event handler in `startHubConnection`:
```js
hubConnection.on('DraftFinished', () => {
  this.$refs.draftFinishedModalRef.show();
});
```

Update the ref name to match:
```js
hubConnection.on('DraftFinished', () => {
  this.$refs.draftCompleteModalRef.show();
});
```

- [ ] **Step 4: Test manually**

Run a league through to a draft completion (or use a test league). The "Draft Complete!" modal should appear. For a single-draft standard league with bids, it should say the bids text. For a one-shot league, it says "The draft is complete!" For a multi-draft non-final draft, it shows the placeholder lorem ipsum (which you'll replace with real copy).

- [ ] **Step 5: Commit**
```
git add src/FantasyCritic.Web/ClientApp/src/views/league.vue
git commit -m "league.vue: replace inline draftFinishedModal with draftCompleteModal component."
```

---

## Task 14: `leagueYearSettings.vue` — Hide Draft Fields for Multi-Draft Leagues

**Files:**
- Modify: `src/FantasyCritic.Web/ClientApp/src/components/leagueYearSettings.vue`

- [ ] **Step 1: Add `isMultiDraft` and `manageDraftsRoute` props**

In the `props` option of `leagueYearSettings.vue`, add:
```js
isMultiDraft: { type: Boolean, default: false },
manageDraftsRoute: { type: Object, default: null }
```

- [ ] **Step 2: Conditionally hide `gamesToDraft` and `counterPicksToDraft`**

Find the `gamesToDraft` field block:
```html
<div v-show="!oneShotMode" class="form-group">
  <label for="gamesToDraft" ...>Number of Games to Draft</label>
  ...
</div>
```
Change `v-show="!oneShotMode"` to `v-show="!oneShotMode && !isMultiDraft"`.

Find the `counterPicksToDraft` field block:
```html
<div v-show="!oneShotMode" class="form-group">
  <label for="counterPicksToDraft" ...>Number of Counter Picks to Draft</label>
  ...
</div>
```
Change `v-show="!oneShotMode"` to `v-show="!oneShotMode && !isMultiDraft"`.

- [ ] **Step 3: Add the multi-draft note**

Directly after the `counterPicksToDraft` block, add:
```html
<div v-if="isMultiDraft" class="alert alert-info">
  This league has multiple drafts.
  <router-link v-if="manageDraftsRoute" :to="manageDraftsRoute">Visit the Manage Drafts page</router-link>
  <span v-else>Visit the Manage Drafts page</span>
  to configure draft settings (games to draft, counter picks, etc.).
</div>
```

- [ ] **Step 4: Test manually**

- On a single-draft league's editLeague page: "Games to Draft" and "Counter Picks to Draft" still appear normally.
- On a multi-draft league's editLeague page (after Task 15 is done): those fields are hidden and the "Visit Manage Drafts" link appears.

- [ ] **Step 5: Commit**
```
git add src/FantasyCritic.Web/ClientApp/src/components/leagueYearSettings.vue
git commit -m "leagueYearSettings: hide draft count fields and show Manage Drafts link for multi-draft leagues."
```

---

## Task 15: `editLeague.vue` — Pass `isMultiDraft` to `leagueYearSettings`

**Files:**
- Modify: `src/FantasyCritic.Web/ClientApp/src/views/editLeague.vue`

- [ ] **Step 1: Add computed properties**

In `editLeague.vue`'s `computed` option, add:
```js
isMultiDraft() {
  return (this.leagueYear?.drafts?.length ?? 0) >= 2;
},
manageDraftsRoute() {
  if (!this.leagueYear) return null;
  return { name: 'manageDrafts', params: { leagueid: this.leagueid, year: this.year } };
}
```

- [ ] **Step 2: Pass the props to `leagueYearSettings`**

Find the `<leagueYearSettings>` component in `editLeague.vue`:
```html
<leagueYearSettings v-model="leagueYearSettings" :year="year" edit-mode :current-number-of-players="activePlayersInLeague" :fresh-settings="freshSettings"></leagueYearSettings>
```

Add the two new props:
```html
<leagueYearSettings
  v-model="leagueYearSettings"
  :year="year"
  edit-mode
  :current-number-of-players="activePlayersInLeague"
  :fresh-settings="freshSettings"
  :is-multi-draft="isMultiDraft"
  :manage-drafts-route="manageDraftsRoute">
</leagueYearSettings>
```

- [ ] **Step 3: Test manually**

- Navigate to the Edit League Year Settings page for a single-draft league. Games to Draft and Counter Picks to Draft appear as normal.
- Navigate to the same page for a league that has 2+ drafts. Those fields are hidden; the "Visit Manage Drafts page" link appears and navigates correctly.

- [ ] **Step 4: Commit**
```
git add src/FantasyCritic.Web/ClientApp/src/views/editLeague.vue
git commit -m "editLeague: pass isMultiDraft and manageDraftsRoute to leagueYearSettings."
```

---

## Self-Review Checklist

After all tasks complete, verify each spec requirement has been addressed:

| Spec requirement | Task(s) |
|---|---|
| Manage Drafts page — list drafts as cards | 2 |
| Manage Drafts page — edit draft inline | 3 |
| Manage Drafts page — delete draft with confirmation | 4 |
| Manage Drafts page — add draft with slot expansion | 5 |
| Manage Drafts page — accessible from leagueActions | 6 |
| Draft Schedule modal for all players | 7, 8 |
| draftReadinessBanner — replaces inline blocks | 9, 10 |
| draftReadinessBanner — handles first vs second+ draft | 9 |
| draftReadinessBanner — imminent / scheduled line | 9 |
| draftReadinessBanner — "no scheduled date" nudge | 9 |
| draftReadinessBanner — manager vs player text variants | 9 |
| startHubConnection — uses activeDraft | 11 |
| draftCompleteModal — league-shape-aware cases | 12, 13 |
| leagueYearSettings — hide draft fields for multi-draft | 14 |
| editLeague — passes isMultiDraft prop | 15 |
| Home page league table multi-draft icon | Already done — no task needed |
| editDraftOrderForm sends draftID | Already done — no task needed |
| resetDraftModal sends draftID | Already done — no task needed |
