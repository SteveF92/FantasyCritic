<template>
  <div>
    <p v-if="!released">
      This is a list of unreleased games, which have not been cancelled, that have been in the system for the longest. "Dreams Dashed" is the total number of people who have had this game in a year
      that it did not release. "Peak Hype Year" is the year in which the most people had the game.
    </p>
    <p v-else>
      The counterpoint to the previous list, these games were in the system for a long time, but
      <em>did</em>
      eventually release. "Dreams Dashed" is the total number of people who have had this game in a year that it did not release, whereas "Dreams Realized" is the total number of players who had this
      game in the year it
      <em>did</em>
      release.
    </p>

    <b-form-checkbox v-if="!released" v-model="includeUnannouncedGames" class="unannounced-toggle">
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
        <template #cell(masterGame.addedTimestamp)="data">
          {{ formatDateIso(data.item.masterGame.addedTimestamp) }}
        </template>
      </b-table>
    </div>
  </div>
</template>

<script lang="ts">
import { defineComponent } from 'vue';
import { type BTableSortBy, type TableField } from 'bootstrap-vue-next';
import { DateTime } from 'luxon';

import { gameClient, type LongestTenuredGameViewModel, type MasterGameYearViewModel } from '@/api/clients';
import { formatMasterGameReleaseDate } from '@/globalFunctions';
import { getApiErrorMessage } from '@/utilities/apiError';
import { formatDateIso } from '@/utilities/formatters';
import MasterGamePopover from '@/components/masterGamePopover.vue';
import MasterGameTagBadge from '@/components/masterGameTagBadge.vue';

interface LongestTenuredGamesPanelData {
  errorInfo: string;
  games: LongestTenuredGameViewModel[];
  isBusy: boolean;
  includeUnannouncedGames: boolean;
  sortBy: BTableSortBy[];
}

export default defineComponent({
  components: {
    MasterGamePopover,
    MasterGameTagBadge
  },
  props: {
    released: { type: Boolean, required: true }
  },
  data(): LongestTenuredGamesPanelData {
    return {
      errorInfo: '',
      games: [],
      isBusy: true,
      includeUnannouncedGames: false,
      sortBy: [{ key: 'daysSinceAddition', order: 'desc' }]
    };
  },
  computed: {
    filteredGames(): LongestTenuredGameViewModel[] {
      if (this.released || this.includeUnannouncedGames) {
        return this.games;
      }
      return this.games.filter((game) => !game.masterGame.tags.includes('UnannouncedGame'));
    },
    gameFields(): TableField<LongestTenuredGameViewModel>[] {
      const base: TableField<LongestTenuredGameViewModel>[] = [
        { key: 'masterGame.gameName', label: 'Game', sortable: true, thClass: 'bg-primary' },
        { key: 'masterGame.maximumReleaseDate', label: 'Release Date', sortable: true, thClass: 'bg-primary' },
        { key: 'masterGame.tags', label: 'Tags', thClass: ['bg-primary', 'lg-screen-minimum'], tdClass: 'lg-screen-minimum' },
        { key: 'masterGame.addedTimestamp', label: 'Date Added', sortable: true, thClass: ['bg-primary', 'md-screen-minimum'], tdClass: 'md-screen-minimum' },
        {
          key: 'daysSinceAddition',
          label: this.released ? 'Days Before Release' : 'Days Since Addition',
          sortable: true,
          thClass: 'bg-primary'
        },
        { key: 'dreamsDashed', label: 'Dreams Dashed', sortable: true, thClass: 'bg-primary' }
      ];

      if (this.released) {
        return base.concat([{ key: 'dreamsRealized', label: 'Dreams Realized', sortable: true, thClass: 'bg-primary' }]);
      }

      return base.concat([
        { key: 'yearOfPeakHype', label: 'Peak Hype Year', sortable: true, thClass: 'bg-primary' },
        { key: 'yearOfPeakHypeCount', label: 'Peak Hype Count', sortable: true, thClass: 'bg-primary' }
      ]);
    }
  },
  watch: {
    released() {
      this.fetchGames();
    }
  },
  created() {
    this.fetchGames();
  },
  methods: {
    formatDateIso,
    getReleaseDate(game: MasterGameYearViewModel): string {
      return formatMasterGameReleaseDate(game, false);
    },
    getDaysSinceAddition(masterGame: MasterGameYearViewModel): number {
      const added = DateTime.fromISO(masterGame.addedTimestamp).startOf('day');
      if (this.released) {
        //Release games must have a release date, so we can assert.
        const releaseDate = DateTime.fromISO(masterGame.releaseDate!).startOf('day');
        return Math.floor(releaseDate.diff(added, 'days').days);
      }
      const now = DateTime.local().startOf('day');
      return Math.floor(now.diff(added, 'days').days);
    },
    async fetchGames() {
      this.isBusy = true;
      try {
        this.errorInfo = '';
        const games = await gameClient.getLongestTenuredGames(this.released);
        this.games = games.map((game) => ({
          ...game,
          daysSinceAddition: this.getDaysSinceAddition(game.masterGame)
        }));
      } catch (error) {
        this.errorInfo = getApiErrorMessage(error, 'Failed to load longest tenured games.');
      }
      this.isBusy = false;
    }
  }
});
</script>

<style scoped>
.unannounced-toggle {
  margin-bottom: 1rem;
}

.spinner {
  display: flex;
  justify-content: space-around;
}
</style>
