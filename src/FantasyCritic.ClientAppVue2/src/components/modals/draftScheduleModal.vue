<template>
  <b-modal id="draftScheduleModal" title="Draft Schedule" ok-only size="lg">
    <b-table :items="leagueYear.drafts" :fields="fields" striped bordered>
      <template #cell(playStatus)="data">
        <b-badge :variant="statusVariant(data.item)">{{ statusLabel(data.item) }}</b-badge>
      </template>
      <template #cell(scheduledDate)="data">
        {{ data.value || '—' }}
      </template>
      <template #cell(gamesAndCPKs)="data">{{ data.item.gamesToDraft }} / {{ data.item.counterPicksToDraft }}</template>
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
        { key: 'gamesAndCPKs', label: 'Games / CPKs' },
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
