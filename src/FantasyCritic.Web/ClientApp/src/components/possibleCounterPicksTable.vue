<template>
  <div>
    <b-pagination v-model="currentPage" :total-rows="rows" :per-page="perPage" aria-controls="counter-pick-table"></b-pagination>

    <b-table :sort-by.sync="sortBy" :sort-desc.sync="sortDesc" :items="gameRows" :fields="gameFields" :per-page="perPage" :current-page="currentPage" bordered small responsive striped>
      <template #cell(gameName)="data">
        <span v-if="data.item.masterGame">
          <masterGamePopover :master-game="data.item.masterGame"></masterGamePopover>
        </span>
        <span v-else>{{ data.item.gameName }}</span>
      </template>
      <template #cell(releaseDate)="data">
        <div v-if="data.item.masterGame" :class="{ 'text-danger': data.item.released }" class="release-date">
          <span>{{ data.item.masterGame.estimatedReleaseDate }}</span>
          <span v-if="data.item.released" class="release-date-qualifier">(Released)</span>
        </div>
        <div v-else>
          <span v-if="data.item.estimatedReleaseDate">{{ data.item.estimatedReleaseDate }}</span>
          <span v-else>TBD</span>
        </div>
      </template>
      <template #cell(hypeFactor)="data">
        <span v-if="data.item.masterGame">
          {{ data.item.masterGame.dateAdjustedHypeFactor | score(1) }}
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
<script>
import MasterGamePopover from '@/components/masterGamePopover.vue';

export default {
  components: {
    MasterGamePopover
  },
  props: {
    value: { type: Object, default: null },
    possibleGames: { type: Array, required: true }
  },
  data() {
    return {
      selectedPublisherGame: null,
      perPage: 10,
      currentPage: 1,
      gameFieldsInternal: [
        { key: 'gameName', label: 'Name', sortable: true, thClass: 'bg-primary' },
        { key: 'releaseDate', label: 'Release Date', sortable: true, thClass: 'bg-primary' },
        { key: 'hypeFactor', label: 'Hype Factor', sortable: true, thClass: ['bg-primary', 'lg-screen-minimum'], tdClass: 'lg-screen-minimum' },
        { key: 'publisherName', label: 'Owned By', sortable: true, thClass: ['bg-primary', 'lg-screen-minimum'], tdClass: 'lg-screen-minimum' },
        { key: 'select', label: '', thClass: 'bg-primary' }
      ],
      sortBy: 'gameName',
      sortDesc: false
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
    selectGame(publisherGame) {
      this.selectedPublisherGame = publisherGame;
      this.$emit('input', this.selectedPublisherGame);
    }
  }
};
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
