<template>
  <div>
    <span class="upcoming-header">
      <template v-if="mode === 'league'">
        <h2 v-show="!recentReleasesMode">Upcoming Releases</h2>
        <h2 v-show="recentReleasesMode">Recent Releases</h2>
      </template>
      <template v-if="mode === 'user'">
        <h3 v-show="!recentReleasesMode">My Upcoming Releases</h3>
        <h3 v-show="recentReleasesMode">My Recent Releases</h3>
      </template>
      <toggle-button
        v-if="isPlusUser"
        v-model="recentReleasesMode"
        class="toggle"
        :sync="true"
        :labels="{ checked: 'Recent', unchecked: 'Upcoming' }"
        :css-colors="true"
        :font-size="13"
        :width="100"
        :height="28" />
    </span>
    <div v-if="gameNewsItems && gameNewsItems.length > 0">
      <b-table v-model:sort-by="sortBy" v-model:sort-desc="sortDesc" :items="gameNewsItems" :fields="gameNewsFields" bordered striped responsive small>
        <template #cell(gameName)="data">
          <masterGamePopover :master-game="data.item.masterGame"></masterGamePopover>
        </template>
        <template #cell(maximumReleaseDate)="data">
          {{ getReleaseDate(data.item) }}
        </template>
        <template #cell(league)="data">
          <ul class="comma-list">
            <li v-for="leaguePublisherSet in data.item.leaguePublisherSets" :key="leaguePublisherSet.LeagueID" class="league-list">
              <router-link :to="{ name: 'league', params: { leagueid: leaguePublisherSet.leagueID, year: leaguePublisherSet.year } }">
                {{ leaguePublisherSet.leagueName }}
              </router-link>
            </li>
          </ul>
        </template>
        <template #cell(publisher)="data">
          <span v-if="!data.item.counterPickPublisherID">
            <router-link :to="{ name: 'publisher', params: { publisherid: data.item.leaguePublisherSets[0].publisherID } }">{{ data.item.leaguePublisherSets[0].publisherName }}</router-link>
          </span>
          <span v-else>
            <router-link :to="{ name: 'publisher', params: { publisherid: data.item.leaguePublisherSets[0].publisherID } }">{{ data.item.leaguePublisherSets[0].publisherName }}</router-link>
            - Counter Picked by:
            <router-link :to="{ name: 'publisher', params: { publisherid: data.item.leaguePublisherSets[0].counterPickPublisherID } }">
              {{ data.item.leaguePublisherSets[0].counterPickPublisherName }}
            </router-link>
          </span>
        </template>
      </b-table>
    </div>
  </div>
</template>
<script>
import moment from 'moment';
import MasterGamePopover from '@/components/masterGamePopover.vue';
import { ToggleButton } from 'vue-js-toggle-button';

export default {
  components: {
    MasterGamePopover,
    ToggleButton
  },
  props: {
    gameNews: { type: Object, required: true },
    mode: { type: String, required: true }
  },
  data() {
    return {
      recentReleasesMode: false,
      sortBy: 'maximumReleaseDate',
      sortDesc: false,
      baseGameNewsFields: [
        { key: 'gameName', label: 'Name', sortable: true, thClass: 'bg-primary' },
        { key: 'maximumReleaseDate', label: 'Release Date', sortable: true, thClass: 'bg-primary' }
      ],
      userGameNewsFields: [{ key: 'league', label: 'League', sortable: true, thClass: ['bg-primary'] }],
      leagueGameNewsFields: [{ key: 'publisher', label: 'Publisher', sortable: true, thClass: ['bg-primary'] }]
    };
  },
  computed: {
    gameNewsFields() {
      if (this.mode === 'user') {
        return this.baseGameNewsFields.concat(this.userGameNewsFields);
      } else if (this.mode === 'league') {
        return this.baseGameNewsFields.concat(this.leagueGameNewsFields);
      }

      return this.baseGameNewsFields;
    },
    gameNewsItems() {
      if (!this.gameNews) {
        return [];
      }
      if (this.recentReleasesMode) {
        return this.gameNews.recentGames;
      }

      return this.gameNews.upcomingGames;
    }
  },
  watch: {
    recentReleasesMode: function () {
      this.sortBy = 'maximumReleaseDate';
      this.sortDesc = this.recentReleasesMode;
    }
  },
  methods: {
    getReleaseDate(game) {
      if (game.releaseDate) {
        return moment(game.releaseDate).format('YYYY-MM-DD');
      }
      return game.estimatedReleaseDate + ' (Estimated)';
    }
  }
};
</script>
<style scoped>
.upcoming-header {
  display: flex;
  justify-content: space-between;
}

.comma-list {
  padding: 0;
  display: inline;
  list-style: none;
}

.comma-list li {
  display: inline;
}

.comma-list li + li:before {
  content: ', ';
}
</style>
