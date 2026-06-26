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
      errorInfo: null,
      editingDraftId: null,
      editForm: {
        name: '',
        scheduledDate: null,
        gamesToDraft: 1,
        counterPicksToDraft: 0
      },
      editError: null,
      deletingDraftId: null
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
    },
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
  }
};
</script>
