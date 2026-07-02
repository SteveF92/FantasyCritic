<template>
  <div class="col-md-10 offset-md-1 col-sm-12">
    <div v-if="errorInfo" class="alert alert-danger">{{ errorInfo }}</div>
    <b-alert v-if="successMessage" variant="success" show dismissible @dismissed="successMessage = null">{{ successMessage }}</b-alert>

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
              <div>
                <strong>{{ draftDateLabel(draft) }}:</strong>
                <template v-if="draft.playStarted">
                  {{ draftStartedDisplay(draft) }}
                </template>
                <template v-else>
                  {{ draft.scheduledDate || '—' }}
                </template>
              </div>
              <div>
                <strong>Games to Draft:</strong>
                {{ draft.gamesToDraft }}
              </div>
              <div>
                <strong>Counter Picks to Draft:</strong>
                {{ draft.counterPicksToDraft }}
              </div>
              <div v-if="leagueYear.drafts.length > 1 && draft.counterPicksToDraft > 0">
                <strong>Counter Picks From This Draft Only:</strong>
                {{ draft.counterPicksMustBeFromThisDraft ? 'Yes' : 'No' }}
              </div>
              <div class="mt-2">
                <b-button v-if="editingDraftId === null" size="sm" variant="info" @click="startEdit(draft)">Edit</b-button>
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
                  <p>
                    Are you sure you want to delete
                    <strong>{{ draft.name }}</strong>
                    ? This cannot be undone.
                  </p>
                  <b-button size="sm" variant="danger" @click="submitDelete(draft)">Confirm Delete</b-button>
                  <b-button size="sm" variant="secondary" class="ml-2" @click="cancelDelete()">Cancel</b-button>
                </div>
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
                <input v-model.number="editForm.gamesToDraft" type="number" class="form-control" :disabled="draft.playStatus !== 'NotStartedDraft'" />
                <small v-if="draft.playStatus !== 'NotStartedDraft'" class="text-muted">Cannot change after draft has started.</small>
              </div>
              <div class="form-group">
                <label>Counter Picks to Draft</label>
                <input v-model.number="editForm.counterPicksToDraft" type="number" class="form-control" :disabled="draft.playStatus !== 'NotStartedDraft'" />
              </div>
              <div v-if="leagueYear.drafts.length > 1 && editForm.counterPicksToDraft > 0" class="form-group">
                <b-form-checkbox v-model="editForm.counterPicksMustBeFromThisDraft" :disabled="draft.playStatus !== 'NotStartedDraft'">
                  <span class="checkbox-label">Lorem ipsum dolor sit amet</span>
                  <p>Consectetur adipiscing elit, sed do eiusmod tempor incididunt.</p>
                </b-form-checkbox>
              </div>
              <b-button size="sm" variant="primary" @click="submitEdit(draft)">Save</b-button>
              <b-button size="sm" variant="secondary" class="ml-2" @click="cancelEdit()">Cancel</b-button>
            </div>
          </b-card>
        </div>

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
            <div v-if="newDraft.counterPicksToDraft > 0" class="form-group">
              <b-form-checkbox v-model="newDraft.counterPicksMustBeFromThisDraft">
                <span class="checkbox-label">Lorem ipsum dolor sit amet</span>
                <p>Consectetur adipiscing elit, sed do eiusmod tempor incididunt.</p>
              </b-form-checkbox>
            </div>
            <div class="form-group">
              <label>Additional Standard Games</label>
              <b-alert show variant="info">
                If you want, you can expand the total roster size to make room for this draft's picks. If you already have all the slots you want set up, you can leave this at 0.
              </b-alert>
              <input v-model.number="newDraft.additionalStandardGames" type="number" min="0" class="form-control" />
            </div>
            <div class="form-group">
              <label>Additional Counter Picks</label>
              <input v-model.number="newDraft.additionalCounterPicks" type="number" min="0" class="form-control" />
            </div>
            <div class="form-group">
              <label>New Special Slots (optional)</label>
              <specialGameSlotSelector v-model="newDraft.newSpecialGameSlots"></specialGameSlotSelector>
              <b-alert show variant="warning" v-if="specialSlotsExceedStandardGames" class="mt-2">
                You are adding {{ newSpecialSlotCount }} special slot{{ newSpecialSlotCount !== 1 ? 's' : '' }} but only {{ newDraft.additionalStandardGames }} additional standard game{{
                  newDraft.additionalStandardGames !== 1 ? 's' : ''
                }}. You must add at least as many 'Additional Standard Games' as new special slots, otherwise the new special slots would convert existing standard slots into special slots.
              </b-alert>
            </div>
            <b-button variant="success" :disabled="!newDraft.name || specialSlotsExceedStandardGames" @click="submitNewDraft()">Add Draft</b-button>
          </b-card>
        </div>
        <div v-else class="alert alert-info">You cannot add a draft while one is in progress.</div>
      </template>
    </div>

    <div v-else class="text-center mt-4">
      <b-spinner></b-spinner>
    </div>
  </div>
</template>
<script>
import axios from 'axios';
import { DateTime } from 'luxon';
import SpecialGameSlotSelector from '@/components/specialGameSlotSelector.vue';

export default {
  components: {
    SpecialGameSlotSelector
  },
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
        counterPicksToDraft: 0,
        counterPicksMustBeFromThisDraft: true
      },
      editError: null,
      deletingDraftId: null,
      deleteError: null,
      newDraft: {
        name: '',
        scheduledDate: null,
        gamesToDraft: 1,
        counterPicksToDraft: 0,
        counterPicksMustBeFromThisDraft: true,
        additionalStandardGames: 0,
        additionalCounterPicks: 0,
        newSpecialGameSlots: []
      },
      newDraftError: null,
      successMessage: null
    };
  },
  computed: {
    isManager() {
      return this.leagueYear?.league?.isManager ?? false;
    },
    activeDraft() {
      return this.leagueYear?.drafts?.find((d) => d.draftIsActive || d.draftIsPaused) ?? null;
    },
    newSpecialSlotCount() {
      return this.newDraft.newSpecialGameSlots?.length ?? 0;
    },
    specialSlotsExceedStandardGames() {
      return this.newSpecialSlotCount > this.newDraft.additionalStandardGames;
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
    draftDateLabel(draft) {
      return draft.playStarted ? 'Date Held' : 'Scheduled Date';
    },
    draftStartedDisplay(draft) {
      if (!draft.draftStartedTimestamp) {
        return '—';
      }
      return DateTime.fromISO(draft.draftStartedTimestamp).toFormat('MMMM dd, yyyy');
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
        counterPicksToDraft: draft.counterPicksToDraft,
        counterPicksMustBeFromThisDraft: draft.counterPicksMustBeFromThisDraft ?? true
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
        counterPicksToDraft: this.editForm.counterPicksToDraft,
        counterPicksMustBeFromThisDraft: this.editForm.counterPicksMustBeFromThisDraft
      };
      try {
        await axios.post('/api/leagueManager/EditLeagueDraft', model);
        this.editingDraftId = null;
        this.editError = null;
        await this.fetchLeagueYear();
      } catch (error) {
        this.editError = error.response?.data || 'An error occurred saving the draft.';
      }
    },
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
    },
    resetNewDraftForm() {
      this.newDraft = {
        name: '',
        scheduledDate: null,
        gamesToDraft: 1,
        counterPicksToDraft: 0,
        counterPicksMustBeFromThisDraft: true,
        additionalStandardGames: 0,
        additionalCounterPicks: 0,
        newSpecialGameSlots: []
      };
      this.newDraftError = null;
    },
    async submitNewDraft() {
      this.newDraftError = null;
      this.successMessage = null;
      const draftName = this.newDraft.name;
      const model = {
        leagueID: this.leagueid,
        year: this.year,
        name: draftName,
        scheduledDate: this.newDraft.scheduledDate || null,
        gamesToDraft: this.newDraft.gamesToDraft,
        counterPicksToDraft: this.newDraft.counterPicksToDraft,
        additionalStandardGames: this.newDraft.additionalStandardGames,
        additionalCounterPicks: this.newDraft.additionalCounterPicks,
        newSpecialGameSlots: this.newDraft.newSpecialGameSlots
      };
      if (this.newDraft.counterPicksToDraft > 0) {
        model.counterPicksMustBeFromThisDraft = this.newDraft.counterPicksMustBeFromThisDraft;
      }
      try {
        await axios.post('/api/leagueManager/CreateLeagueDraft', model);
        this.resetNewDraftForm();
        await this.fetchLeagueYear();
        this.successMessage = `Draft "${draftName}" was added.`;
        this.$nextTick(() => {
          window.scrollTo({ top: 0, behavior: 'smooth' });
        });
      } catch (error) {
        this.newDraftError = error.response?.data || 'An error occurred adding the draft.';
      }
    }
  }
};
</script>
