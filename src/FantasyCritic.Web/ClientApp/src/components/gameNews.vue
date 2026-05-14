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
      <b-table
        :sort-by.sync="sortBy"
        :sort-desc.sync="sortDesc"
        :items="gameNewsItems"
        :fields="gameNewsFields"
        bordered
        striped
        responsive
        small
        :per-page="perPage"
        :current-page="currentPage"
        class="mb-0">
        <template #cell(gameName)="data">
          <masterGamePopover :master-game="data.item.masterGame"></masterGamePopover>
        </template>
        <template #cell(maximumReleaseDate)="data">
          {{ getReleaseDate(data.item) }}
        </template>
        <template #cell(league)="data">
          <div class="league-pill-list">
            <router-link
              v-for="leaguePublisherSet in getSortedLeaguePublisherSets(data.item)"
              :key="leaguePublisherSet.LeagueID"
              :to="{ name: 'league', params: { leagueid: leaguePublisherSet.leagueID, year: leaguePublisherSet.year } }"
              class="league-pill-link">
              <span>{{ leaguePublisherSet.leagueName }}</span>
              <span v-if="mode === 'user' && isCounterPickPublisherSet(leaguePublisherSet)" class="league-pill-counter-pick">Counter Pick</span>
            </router-link>
          </div>
        </template>
        <template #cell(publisher)="data">
          <span v-if="!data.item.leaguePublisherSets[0].counterPickPublisherID">
            <router-link :to="{ name: 'publisher', params: { publisherid: data.item.leaguePublisherSets[0].publisherID } }">{{ data.item.leaguePublisherSets[0].publisherName }}</router-link>
          </span>
          <span v-else>
            <router-link :to="{ name: 'publisher', params: { publisherid: getPrimaryPublisherSet(data.item).publisherID } }">{{ getPrimaryPublisherSet(data.item).publisherName }}</router-link>
            - Counter Picked by:
            <router-link :to="{ name: 'publisher', params: { publisherid: getCounterPickPublisherID(data.item) } }">
              {{ getPrimaryPublisherSet(data.item).counterPickPublisherName }}
            </router-link>
          </span>
        </template>
      </b-table>
      <b-pagination v-model="currentPage" :total-rows="gameNewsItems.length" :per-page="perPage" aria-controls="my-table" align="right" size="sm" class="my-0 pagination-dark"></b-pagination>
    </div>
    <div v-else><h4>No Games Found</h4></div>
  </div>
</template>
<script>
import { DateTime } from 'luxon';
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
      currentPage: 1,
      perPage: 10,
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
    getSortedLeaguePublisherSets(game) {
      if (!game || !game.leaguePublisherSets || game.leaguePublisherSets.length === 0) {
        return [];
      }

      return [...game.leaguePublisherSets].sort((a, b) => {
        const aName = (a.leagueName || '').toLowerCase();
        const bName = (b.leagueName || '').toLowerCase();
        return aName.localeCompare(bName);
      });
    },
    isCounterPickPublisherSet(publisherSet) {
      if (!publisherSet) {
        return false;
      }

      return Boolean(publisherSet.counterPick || publisherSet.CounterPick || publisherSet.counterpick);
    },
    getPrimaryPublisherSet(game) {
      if (!game || !game.leaguePublisherSets || game.leaguePublisherSets.length === 0) {
        return {};
      }

      return game.leaguePublisherSets[0];
    },
    getCounterPickPublisherID(game) {
      const publisherSet = this.getPrimaryPublisherSet(game);
      return publisherSet.counterPickPublisherID || publisherSet.counterPickPublisherId || null;
    },
    getReleaseDate(game) {
      if (game.releaseDate) {
        return DateTime.fromISO(game.releaseDate).toFormat('yyyy-MM-dd');
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

.league-pill-list {
  display: flex;
  flex-wrap: wrap;
  gap: 0.35rem;
}

.league-pill-link {
  display: inline-flex;
  align-items: center;
  gap: 0.35rem;
  padding: 0.2rem 0.45rem;
  border-radius: 999px;
  border: 1px solid rgba(214, 153, 58, 0.45);
  background: rgba(214, 153, 58, 0.12);
  color: #f0f0f0;
  text-decoration: none;
  line-height: 1.2;
}

.league-pill-link:hover,
.league-pill-link:focus {
  border-color: rgba(232, 179, 102, 0.7);
  background: rgba(232, 179, 102, 0.2);
  color: #fff;
  text-decoration: none;
}

.league-pill-counter-pick {
  font-size: 0.7rem;
  font-weight: 600;
  padding: 0.05rem 0.35rem;
  border-radius: 999px;
  background: rgba(255, 193, 7, 0.25);
  border: 1px solid rgba(255, 193, 7, 0.55);
  color: #ffdf8a;
}

@media (max-width: 767.98px) {
  .league-pill-list {
    display: flex;
    flex-direction: column;
    gap: 0.25rem;
  }

  .league-pill-link {
    display: flex;
    flex-direction: column;
    align-items: flex-start;
    gap: 0.2rem;
    margin-bottom: 0;
    padding: 0.28rem 0.45rem;
    border: 1px solid rgba(214, 153, 58, 0.35);
    border-radius: 8px;
    background: rgba(214, 153, 58, 0.08);
    color: #f0f0f0;
    text-decoration: none;
    line-height: 1.25;
    word-break: normal;
    overflow-wrap: break-word;
  }

  .league-pill-link > span:first-child {
    text-decoration: underline;
    text-underline-offset: 2px;
  }

  .league-pill-link:hover,
  .league-pill-link:focus {
    border-color: rgba(232, 179, 102, 0.6);
    background: rgba(232, 179, 102, 0.16);
    color: #fff;
  }

  .league-pill-counter-pick {
    font-size: 0.62rem;
    padding: 0.02rem 0.28rem;
  }
}
</style>
