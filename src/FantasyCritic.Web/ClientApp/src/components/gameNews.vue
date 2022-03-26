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
      <b-table :sort-by.sync="sortBy" :sort-desc.sync="sortDesc" :items="gameNewsItems" :fields="gameNewsFields" bordered striped responsive small>
        <template #cell(gameName)="data">
          <masterGamePopover :master-game="data.item.masterGame"></masterGamePopover>
        </template>
        <template #cell(maximumReleaseDate)="data">
          {{ getReleaseDate(data.item) }}
        </template>
        <template #cell(league)="data">
          <router-link v-if="data.item.leagueID" :to="{ name: 'league', params: { leagueid: data.item.leagueID, year: data.item.year } }">{{ data.item.leagueName }}</router-link>
          <span v-else>{{ data.item.leagueName }}</span>
        </template>
        <template #cell(publisher)="data">
          <span v-if="!data.item.counterPickPublisherID">
            <router-link :to="{ name: 'publisher', params: { publisherid: data.item.publisherID } }">{{ data.item.publisherName }}</router-link>
          </span>
          <span v-else>
            <router-link :to="{ name: 'publisher', params: { publisherid: data.item.publisherID } }">{{ data.item.publisherName }}</router-link>
            - Counter Picked by:
            <router-link :to="{ name: 'publisher', params: { publisherid: data.item.counterPickPublisherID } }">{{ data.item.counterPickPublisherName }}</router-link>
          </span>
        </template>
      </b-table>
    </div>
  </div>
</template>
<script>
import moment from 'moment';
import MasterGamePopover from '@/components/masterGamePopover';
import { ToggleButton } from 'vue-js-toggle-button';

export default {
  components: {
    MasterGamePopover,
    ToggleButton
  },
  props: {
    gameNews: Object,
    mode: String
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
    isPlusUser() {
      return this.$store.getters.isPlusUser;
    },
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
</style>
