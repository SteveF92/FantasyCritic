<template>
  <div>
    <b-pagination v-model="currentPage" :total-rows="rows" :per-page="perPage" aria-controls="my-table"></b-pagination>

    <b-table :sort-by.sync="sortBy" :sort-desc.sync="sortDesc" :items="gameRows" :fields="gameFields" :per-page="perPage" :current-page="currentPage" bordered small responsive striped>
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
        {{ data.item.masterGame.dateAdjustedHypeFactor | score(1) }}
      </template>
      <template #cell(status)="data">
        <statusBadge :possible-master-game="data.item"></statusBadge>
      </template>
      <template #cell(select)="data">
        <b-button size="sm" variant="info" @click="selectGame(data.item.masterGame)">Select</b-button>
      </template>
    </b-table>
  </div>
</template>
<script>
import StatusBadge from '@/components/statusBadge';
import MasterGamePopover from '@/components/masterGamePopover';

export default {
  components: {
    StatusBadge,
    MasterGamePopover
  },
  props: {
    value: { type: Object, default: null },
    possibleGames: { type: Array, required: true }
  },
  data() {
    return {
      selectedMasterGame: null,
      lastPopoverShown: null,
      perPage: 10,
      currentPage: 1,
      gameFieldsInternal: [
        { key: 'masterGame.gameName', label: 'Name', sortable: true, thClass: 'bg-primary' },
        { key: 'masterGame.maximumReleaseDate', label: 'Release Date', sortable: true, thClass: 'bg-primary' },
        { key: 'masterGame.dateAdjustedHypeFactor', label: 'Hype Factor', sortable: true, thClass: ['bg-primary', 'lg-screen-minimum'], tdClass: 'lg-screen-minimum' },
        { key: 'status', label: 'Status', thClass: ['bg-primary', 'lg-screen-minimum'], tdClass: 'lg-screen-minimum' },
        { key: 'select', label: '', thClass: 'bg-primary' }
      ],
      sortBy: 'dateAdjustedHypeFactor',
      sortDesc: true
    };
  },
  computed: {
    rows() {
      return this.possibleGames.length;
    },
    gameRows() {
      let gameRows = this.possibleGames;
      if (!gameRows) {
        return [];
      }
      return gameRows;
    },
    gameFields() {
      return this.gameFieldsInternal;
    }
  },
  methods: {
    selectGame(masterGame) {
      this.selectedMasterGame = masterGame;
      this.$emit('input', this.selectedMasterGame);
    }
  }
};
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
