<template>
  <div class="col-md-10 offset-md-1 col-sm-12">
    <h1>Longest Tenured Games</h1>
    <p>These are unreleased games that have been in the system the longest.</p>

    <b-form-checkbox v-model="includeUnannouncedGames" class="unannounced-toggle">
      <span class="checkbox-label">Include unannounced games</span>
    </b-form-checkbox>

    <b-table small bordered striped responsive :items="filteredGames" :fields="gameFields" :sort-by.sync="sortBy" :sort-desc.sync="sortDesc">
      <template #cell(gameName)="data">
        <masterGamePopover :master-game="data.item"></masterGamePopover>
      </template>
      <template #cell(maximumReleaseDate)="data">
        {{ getReleaseDate(data.item) }}
      </template>
      <template #cell(tags)="data">
        <span v-for="tag in data.item.tags" :key="tag">
          <masterGameTagBadge :tag-name="tag" short></masterGameTagBadge>
        </span>
      </template>
      <template #cell(addedTimestamp)="data">
        {{ data.item.addedTimestamp | date }}
      </template>
    </b-table>
  </div>
</template>

<script>
import axios from 'axios';
import { DateTime } from 'luxon';
import { formatMasterGameReleaseDate } from '@/globalFunctions';
import MasterGamePopover from '@/components/masterGamePopover.vue';
import MasterGameTagBadge from '@/components/masterGameTagBadge.vue';

export default {
  components: {
    MasterGamePopover,
    MasterGameTagBadge
  },
  data() {
    return {
      longestTenuredGames: null,
      includeUnannouncedGames: false,
      sortBy: 'daysSinceAddition',
      sortDesc: true,
      gameFields: [
        { key: 'gameName', label: 'Game', sortable: true, thClass: 'bg-primary' },
        { key: 'maximumReleaseDate', label: 'Release Date', sortable: true, thClass: 'bg-primary' },
        { key: 'tags', label: 'Tags', thClass: ['bg-primary', 'lg-screen-minimum'], tdClass: 'lg-screen-minimum' },
        { key: 'addedTimestamp', label: 'Date Added', sortable: true, thClass: ['bg-primary', 'md-screen-minimum'], tdClass: 'md-screen-minimum' },
        { key: 'daysSinceAddition', label: 'Days Since Addition', sortable: true, thClass: 'bg-primary' }
      ]
    };
  },
  computed: {
    filteredGames() {
      if (!this.longestTenuredGames) {
        return null;
      }

      if (this.includeUnannouncedGames) {
        return this.longestTenuredGames;
      }

      return this.longestTenuredGames.filter((game) => !game.tags.includes('UnannouncedGame'));
    }
  },
  methods: {
    getReleaseDate(game) {
      return formatMasterGameReleaseDate(game);
    }
  },
  async created() {
    const response = await axios.get('/api/game/GetLongestTenuredGames');
    const now = DateTime.local().startOf('day');
    this.longestTenuredGames = response.data.map((game) => {
      const added = DateTime.fromISO(game.addedTimestamp).startOf('day');
      return {
        ...game,
        daysSinceAddition: Math.floor(now.diff(added, 'days').days)
      };
    });
  }
};
</script>

<style scoped>
.unannounced-toggle {
  margin-bottom: 1rem;
}
</style>
