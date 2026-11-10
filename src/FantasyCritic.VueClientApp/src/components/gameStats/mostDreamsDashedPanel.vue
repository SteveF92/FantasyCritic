<template>
  <div>
    <p>Games that have dashed the most dreams — the total number of distinct players who had the game on a roster in a year it did not release.</p>

    <b-form-group label="Year" label-for="dreams-dashed-year" label-size="sm" class="dreams-dashed-year-selector">
      <b-form-select id="dreams-dashed-year" v-model="dreamsDashedYear" :options="dreamsDashedYearOptions" size="sm" />
    </b-form-group>

    <b-form-checkbox v-model="includeUnannouncedGames" class="unannounced-toggle">
      <span class="checkbox-label">Include unannounced games</span>
    </b-form-checkbox>

    <b-alert v-if="errorInfo" variant="danger" :model-value="true">{{ errorInfo }}</b-alert>
    <div v-else-if="isBusy" class="spinner">
      <font-awesome-icon icon="circle-notch" size="5x" spin :style="{ color: '#D6993A' }" />
    </div>
    <div v-else>
      <b-table small bordered striped responsive :items="filteredGames" :fields="gameFields" v-model:sort-by="sortBy">
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
  </div>
</template>

<script lang="ts">
import { defineComponent } from 'vue';
import { mapState } from 'pinia';
import { type BTableSortBy, type TableField } from 'bootstrap-vue-next';

import { gameClient, type LongestTenuredGameViewModel, type MasterGameYearViewModel } from '@/api/clients';
import { formatMasterGameReleaseDate } from '@/globalFunctions';
import { useInterLeagueStore } from '@/store/interLeagueStore';
import { getApiErrorMessage } from '@/utilities/apiError';
import MasterGamePopover from '@/components/masterGamePopover.vue';
import MasterGameTagBadge from '@/components/masterGameTagBadge.vue';

interface DreamsDashedYearOption {
  value: number | null;
  text: string;
}

interface MostDreamsDashedPanelData {
  errorInfo: string;
  games: LongestTenuredGameViewModel[];
  isBusy: boolean;
  dreamsDashedYear: number | null;
  includeUnannouncedGames: boolean;
  sortBy: BTableSortBy[];
}

export default defineComponent({
  setup() {
    const gameFields: TableField<LongestTenuredGameViewModel>[] = [
      { key: 'masterGame.gameName', label: 'Game', sortable: true, thClass: 'bg-primary' },
      { key: 'masterGame.maximumReleaseDate', label: 'Release Date', sortable: true, thClass: 'bg-primary' },
      { key: 'masterGame.tags', label: 'Tags', thClass: ['bg-primary', 'lg-screen-minimum'], tdClass: 'lg-screen-minimum' },
      { key: 'dreamsDashed', label: 'Dreams Dashed', sortable: true, thClass: 'bg-primary' }
    ];

    return { gameFields };
  },
  components: {
    MasterGamePopover,
    MasterGameTagBadge
  },
  data(): MostDreamsDashedPanelData {
    return {
      errorInfo: '',
      games: [],
      isBusy: true,
      dreamsDashedYear: null,
      includeUnannouncedGames: false,
      sortBy: [{ key: 'dreamsDashed', order: 'desc' }]
    };
  },
  computed: {
    ...mapState(useInterLeagueStore, ['supportedYears']),
    dreamsDashedYearOptions(): DreamsDashedYearOption[] {
      return [{ value: null, text: 'All Time' }, ...this.supportedYears.filter((y) => y.finished).map((y) => ({ value: y.year, text: String(y.year) }))];
    },
    filteredGames(): LongestTenuredGameViewModel[] {
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
    getReleaseDate(game: MasterGameYearViewModel): string {
      return formatMasterGameReleaseDate(game);
    },
    async fetchGames() {
      this.isBusy = true;
      try {
        this.games = await gameClient.getMostDreamsDashedGames(this.dreamsDashedYear);
      } catch (error) {
        this.errorInfo = getApiErrorMessage(error, 'Failed to load most dreams dashed games.');
      }
      this.isBusy = false;
    }
  }
});
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
