<template>
  <div>
    <b-pagination v-model="currentPage" :total-rows="rows" :per-page="perPage" aria-controls="my-table"></b-pagination>

    <b-table small bordered striped responsive :items="possibleGames" :fields="gameFields" :per-page="perPage" :current-page="currentPage">
      <template #cell(masterGame.gameName)="data">
        <masterGamePopover :master-game="data.item.masterGame"></masterGamePopover>
      </template>
      <template #cell(masterGame.maximumReleaseDate)="data">
        <div :class="{ 'text-danger': data.item.masterGame.isReleased }" class="release-date">
          <span>{{ data.item.masterGame.estimatedReleaseDate }}</span>
          <span v-if="data.item.masterGame.isReleased">(Released)</span>
        </div>
      </template>
      <template #cell(masterGame.dateAdjustedHypeFactor)="data">
        {{ formatScore(data.item.masterGame.dateAdjustedHypeFactor, 1) }}
      </template>
      <template #cell(status)="data">
        <statusBadge :possible-master-game="data.item" strip-that></statusBadge>
      </template>
      <template #cell(cost)="data">
        {{ formatCurrency(data.item.cost) }}
      </template>
      <template #cell(select)="data">
        <b-button size="sm" variant="info" @click="selectGame(data.item)">Select</b-button>
      </template>
    </b-table>
  </div>
</template>
<script lang="ts">
import { defineComponent, type PropType } from 'vue';
import type { TableField } from 'bootstrap-vue-next';

import type { PossibleRoyaleMasterGameViewModel } from '@/api/clients';
import { numericComparer } from '@/globalFunctions';
import { formatCurrency, formatScore } from '@/utilities/formatters';
import MasterGamePopover from '@/components/masterGamePopover.vue';
import StatusBadge from '@/components/statusBadge.vue';

interface PossibleRoyaleMasterGamesTableData {
  currentPage: number;
  selectedPossibleRoyaleGame: PossibleRoyaleMasterGameViewModel | null;
  lastPopoverShown: unknown;
}

export default defineComponent({
  setup() {
    const perPage: number = 10;
    const gameFields: TableField<PossibleRoyaleMasterGameViewModel>[] = [
      { key: 'masterGame.gameName', label: 'Name', sortable: true, thClass: 'bg-primary' },
      { key: 'masterGame.maximumReleaseDate', label: 'Release Date', sortable: true, thClass: 'bg-primary' },
      { key: 'masterGame.dateAdjustedHypeFactor', label: 'Hype Factor', sortable: true, thClass: ['bg-primary', 'lg-screen-minimum'], tdClass: 'lg-screen-minimum', sortCompare: numericComparer },
      { key: 'cost', label: 'Cost', sortable: true, thClass: 'bg-primary', sortCompare: numericComparer },
      { key: 'status', label: 'Status', thClass: ['bg-primary', 'lg-screen-minimum'], tdClass: 'lg-screen-minimum' },
      { key: 'select', label: '', thClass: 'bg-primary' }
    ];

    return { perPage, gameFields };
  },
  components: {
    StatusBadge,
    MasterGamePopover
  },
  props: {
    modelValue: { type: Object as PropType<PossibleRoyaleMasterGameViewModel | null>, default: null },
    possibleGames: { type: Array as PropType<PossibleRoyaleMasterGameViewModel[]>, required: true }
  },
  emits: {
    'update:modelValue': (_possibleRoyaleGame: PossibleRoyaleMasterGameViewModel | null) => true
  },
  data(): PossibleRoyaleMasterGamesTableData {
    return {
      currentPage: 1,
      selectedPossibleRoyaleGame: null,
      lastPopoverShown: null
    };
  },
  computed: {
    rows(): number {
      return this.possibleGames.length;
    }
  },
  methods: {
    formatCurrency,
    formatScore,
    selectGame(possibleRoyaleGame: PossibleRoyaleMasterGameViewModel): void {
      this.selectedPossibleRoyaleGame = possibleRoyaleGame;
      this.$emit('update:modelValue', this.selectedPossibleRoyaleGame);
    }
  }
});
</script>
<style scoped>
.fake-link {
  text-decoration: underline;
  cursor: pointer;
}

.release-date {
  font-weight: bold;
}

.select-cell {
  text-align: center;
}

@media only screen and (max-width: 450px) {
  .no-mobile {
    display: none;
  }
}
</style>
