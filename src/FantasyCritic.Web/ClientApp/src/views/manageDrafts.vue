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
