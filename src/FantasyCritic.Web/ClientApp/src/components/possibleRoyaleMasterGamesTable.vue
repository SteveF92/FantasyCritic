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
        {{ data.item.masterGame.dateAdjustedHypeFactor | score(1) }}
      </template>
      <template #cell(status)="data">
        <statusBadge :possible-master-game="data.item"></statusBadge>
      </template>
      <template #cell(cost)="data">
        {{ data.item.cost | money }}
      </template>
      <template #cell(select)="data">
        <b-button size="sm" variant="info" @click="selectGame(data.item)">Select</b-button>
      </template>
    </b-table>
  </div>
</template>
<script>
import StatusBadge from '@/components/statusBadge.vue';
import MasterGamePopover from '@/components/masterGamePopover.vue';

export default {
  components: {
    StatusBadge,
    MasterGamePopover
  },
  props: {
    value: { type: Object, required: true },
    possibleGames: { type: Array, required: true }
  },
  data() {
    return {
      perPage: 10,
      currentPage: 1,
      selectedPossibleRoyaleGame: null,
      lastPopoverShown: null,
      gameFields: [
        { key: 'masterGame.gameName', label: 'Name', sortable: true, thClass: 'bg-primary' },
        { key: 'masterGame.maximumReleaseDate', label: 'Release Date', sortable: true, thClass: 'bg-primary' },
        { key: 'masterGame.dateAdjustedHypeFactor', label: 'Hype Factor', sortable: true, thClass: ['bg-primary', 'lg-screen-minimum'], tdClass: 'lg-screen-minimum' },
        { key: 'cost', label: 'Cost', sortable: true, thClass: 'bg-primary' },
        { key: 'status', label: 'Status', thClass: ['bg-primary', 'lg-screen-minimum'], tdClass: 'lg-screen-minimum' },
        { key: 'select', label: '', thClass: 'bg-primary' }
      ]
    };
  },
  computed: {
    rows() {
      return this.possibleGames.length;
    }
  },
  methods: {
    selectGame(possibleRoyaleGame) {
      this.selectedPossibleRoyaleGame = possibleRoyaleGame;
      this.$emit('input', this.selectedPossibleRoyaleGame);
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
