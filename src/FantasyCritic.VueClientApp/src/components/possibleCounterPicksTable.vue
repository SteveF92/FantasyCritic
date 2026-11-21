<template>
  <div>
    <b-pagination v-model="currentPage" :total-rows="possibleGames.length" :per-page="perPage" aria-controls="counter-pick-table"></b-pagination>

    <b-table v-model:sort-by="sortBy" :items="possibleGames" :fields="gameFields" :per-page="perPage" :current-page="currentPage" bordered small responsive striped>
      <template #cell(gameName)="data">
        <span v-if="data.item.masterGame">
          <masterGamePopover :master-game="data.item.masterGame"></masterGamePopover>
        </span>
        <span v-else>{{ data.item.gameName }}</span>
      </template>
      <template #cell(masterGame.maximumReleaseDate)="data">
        <div v-if="data.item.masterGame" :class="{ 'text-danger': data.item.released }" class="release-date">
          <span>{{ data.item.masterGame.estimatedReleaseDate }}</span>
          <span v-if="data.item.released" class="release-date-qualifier">(Released)</span>
        </div>
        <div v-else>
          <span v-if="data.item.estimatedReleaseDate">{{ data.item.estimatedReleaseDate }}</span>
          <span v-else>TBD</span>
        </div>
      </template>
      <template #cell(masterGame.dateAdjustedHypeFactor)="data">
        <span v-if="data.item.masterGame">
          {{ formatScore(data.item.masterGame.dateAdjustedHypeFactor, 1) }}
        </span>
        <span v-else>N/A</span>
      </template>
      <template #cell(publisherName)="data">
        {{ data.item.publisherName }}
      </template>
      <template #cell(select)="data">
        <b-button size="sm" variant="info" @click="selectGame(data.item)">Select</b-button>
      </template>
    </b-table>
  </div>
</template>

<script lang="ts">
import { defineComponent, type PropType } from 'vue';
import type { BTableSortBy, TableField } from 'bootstrap-vue-next';

import type { PublisherGameViewModel } from '@/api/clients';
import { numericComparer } from '@/globalFunctions';
import { formatScore } from '@/utilities/formatters';
import MasterGamePopover from '@/components/masterGamePopover.vue';

interface PossibleCounterPicksTableData {
  perPage: number;
  currentPage: number;
  sortBy: BTableSortBy[];
}

export default defineComponent({
  setup() {
    const gameFields: TableField<PublisherGameViewModel>[] = [
      { key: 'gameName', label: 'Name', sortable: true, thClass: 'bg-primary' },
      { key: 'masterGame.maximumReleaseDate', label: 'Release Date', sortable: true, thClass: 'bg-primary' },
      { key: 'masterGame.dateAdjustedHypeFactor', label: 'Hype Factor', sortable: true, thClass: ['bg-primary', 'lg-screen-minimum'], tdClass: 'lg-screen-minimum', sortCompare: numericComparer },
      { key: 'publisherName', label: 'Owned By', sortable: true, thClass: ['bg-primary', 'lg-screen-minimum'], tdClass: 'lg-screen-minimum' },
      { key: 'select', label: '', thClass: 'bg-primary' }
    ];

    return { gameFields };
  },
  components: {
    MasterGamePopover
  },
  props: {
    modelValue: { type: Object as PropType<PublisherGameViewModel>, default: null },
    possibleGames: { type: Array as PropType<PublisherGameViewModel[]>, required: true }
  },
  emits: {
    'update:modelValue': (_value: PublisherGameViewModel) => true
  },
  data(): PossibleCounterPicksTableData {
    return {
      perPage: 10,
      currentPage: 1,
      sortBy: [{ key: 'gameName', order: 'asc' }]
    };
  },
  methods: {
    formatScore,
    selectGame(publisherGame: PublisherGameViewModel): void {
      this.$emit('update:modelValue', publisherGame);
    }
  }
});
</script>

<style scoped>
.release-date {
  font-weight: bold;
}

.release-date-qualifier {
  margin-left: 5px;
}

@media only screen and (max-width: 450px) {
  .lg-screen-minimum {
    display: none;
  }
}
</style>
