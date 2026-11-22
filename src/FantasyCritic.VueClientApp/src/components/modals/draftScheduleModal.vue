<template>
  <b-modal id="draft-schedule-modal" title="Draft Schedule" ok-only size="lg">
    <b-table :items="requiredLeagueYear.drafts" :fields="fields" striped bordered>
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

<script lang="ts">
import { defineComponent } from 'vue';
import { mapState } from 'pinia';
import type { ColorVariant, TableField } from 'bootstrap-vue-next';

import type { LeagueDraftViewModel } from '@/api/clients';
import { useLeagueStore } from '@/store/leagueStore';

const statusLabels: Record<string, string> = {
  NotStartedDraft: 'Not Started',
  Drafting: 'In Progress',
  DraftPaused: 'Paused',
  DraftFinal: 'Completed'
};

const statusVariants: Record<string, ColorVariant> = {
  NotStartedDraft: 'secondary',
  Drafting: 'success',
  DraftPaused: 'warning',
  DraftFinal: 'info'
};

export default defineComponent({
  setup() {
    const fields: TableField<LeagueDraftViewModel>[] = [
      { key: 'draftNumber', label: '#' },
      { key: 'name', label: 'Name' },
      { key: 'scheduledDate', label: 'Scheduled Date' },
      { key: 'gamesAndCPKs', label: 'Games / CPKs' },
      { key: 'playStatus', label: 'Status' }
    ];

    return { fields };
  },
  computed: {
    ...mapState(useLeagueStore, ['requiredLeagueYear'])
  },
  methods: {
    statusLabel(draft: LeagueDraftViewModel): string {
      return statusLabels[draft.playStatus] ?? draft.playStatus;
    },
    statusVariant(draft: LeagueDraftViewModel): ColorVariant {
      return statusVariants[draft.playStatus] ?? 'secondary';
    }
  }
});
</script>
