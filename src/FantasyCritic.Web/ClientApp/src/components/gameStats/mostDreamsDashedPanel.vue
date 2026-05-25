<template>
  <div>
    <p>Games that have dashed the most dreams — the total number of distinct players who had the game on a roster in a year it did not release.</p>

    <b-form-group label="Year" label-for="dreams-dashed-year" label-size="sm" class="dreams-dashed-year-selector">
      <b-form-select id="dreams-dashed-year" v-model="dreamsDashedYear" :options="dreamsDashedYearOptions" size="sm" />
    </b-form-group>

    <b-form-checkbox v-model="includeUnannouncedGames" class="unannounced-toggle">
      <span class="checkbox-label">Include unannounced games</span>
    </b-form-checkbox>

    <div v-if="showTable">
      <b-table small bordered striped responsive :items="filteredGames" :fields="gameFields" :sort-by.sync="sortBy" :sort-desc.sync="sortDesc">
        <template #cell(masterGame.gameName)="data">
          <masterGamePopover :master-game="data.item.masterGame"></masterGamePopover>
        </template>
        <template #cell(masterGame.maximumReleaseDate)="data">
          {{ getReleaseDate(data.item.masterGame) }}
        </template>
        <template #cell(masterGame.tags)="data">
          <span v-for="tag in data.item.masterGame.tags" :key="tag">
            <masterGameTagBadge :tag-name="tag" short></masterGameTagBadge>
          </span>
        </template>
      </b-table>
    </div>

    <div v-else class="spinner">
      <font-awesome-icon icon="circle-notch" size="5x" spin :style="{ color: '#D6993A' }" />
    </div>
  </div>
</template>

<script>
import axios from 'axios';
import BasicMixin from '@/mixins/basicMixin.js';
import { formatMasterGameReleaseDate } from '@/globalFunctions';
import MasterGamePopover from '@/components/masterGamePopover.vue';
import MasterGameTagBadge from '@/components/masterGameTagBadge.vue';

export default {
  components: {
    MasterGamePopover,
    MasterGameTagBadge
  },
  mixins: [BasicMixin],
  data() {
    return {
      games: null,
      isBusy: true,
      dreamsDashedYear: null,
      includeUnannouncedGames: false,
      sortBy: 'dreamsDashed',
      sortDesc: true,
      gameFields: [
        { key: 'masterGame.gameName', label: 'Game', sortable: true, thClass: 'bg-primary' },
        { key: 'masterGame.maximumReleaseDate', label: 'Release Date', sortable: true, thClass: 'bg-primary' },
        { key: 'masterGame.tags', label: 'Tags', thClass: ['bg-primary', 'lg-screen-minimum'], tdClass: 'lg-screen-minimum' },
        { key: 'dreamsDashed', label: 'Dreams Dashed', sortable: true, thClass: 'bg-primary' }
      ]
    };
  },
  computed: {
    dreamsDashedYearOptions() {
      return [
        { value: null, text: 'All Time' },
        ...this.supportedYears.filter((y) => y.finished).map((y) => ({ value: y.year, text: String(y.year) }))
      ];
    },
    showTable() {
      return this.games && !this.isBusy;
    },
    filteredGames() {
      if (!this.games) {
        return null;
      }
      if (this.includeUnannouncedGames) {
        return this.games;
      }
      return this.games.filter((game) => !game.masterGame.tags.includes('UnannouncedGame'));
    }
  },
  watch: {
    dreamsDashedYear() {
      this.fetchGames();
    }
  },
  created() {
    this.fetchGames();
  },
  methods: {
    getReleaseDate(game) {
      return formatMasterGameReleaseDate(game);
    },
    async fetchGames() {
      this.isBusy = true;
      const params = this.dreamsDashedYear != null ? { year: this.dreamsDashedYear } : {};
      const response = await axios.get('/api/game/GetMostDreamsDashedGames', { params });
      this.games = response.data;
      this.isBusy = false;
    }
  }
};
</script>

<style scoped>
.dreams-dashed-year-selector {
  margin-bottom: 1rem;
  max-width: 200px;
}

.unannounced-toggle {
  margin-bottom: 1rem;
}

.spinner {
  display: flex;
  justify-content: space-around;
}
</style>
