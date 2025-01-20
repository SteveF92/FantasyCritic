<template>
  <div class="col-md-10 offset-md-1 col-sm-12">
    <div v-if="forbidden">
      <div class="alert alert-danger" role="alert">You do not have permission to view this league.</div>
    </div>

    <div v-if="league && !league.publicLeague && !(league.userIsInLeague || league.outstandingInvite)" class="alert alert-warning" role="info">You are viewing a private league.</div>

    <div v-if="leagueAllTimeStats">
      <h1>{{ league.leagueName }} All Time Stats</h1>
      <hr />
      <div v-if="league.years.length > 2">
        <h2>Player Stats</h2>
        <b-table :sort-by.sync="playerSortBy" :sort-desc.sync="playerSortDesc" :items="leagueAllTimeStats.playerAllTimeStats" :fields="playerStatFields" bordered small responsive striped>
          <template #cell(playerName)="data">
            <div>{{ data.item.playerName }}</div>
            <div class="won-years">
              <span v-for="year in data.item.yearsWon" :key="year" class="badge badge-success">
                <font-awesome-icon icon="crown" class="year-winner-crown" />
                {{ year }}
              </span>
            </div>
          </template>
          <template #cell(totalFantasyPoints)="data">
            {{ data.item.totalFantasyPoints | score(2) }}
          </template>
          <template #cell(averageFinishRanking)="data">
            {{ data.item.averageFinishRanking | score(2) }}
          </template>
          <template #cell(averageFantasyPoints)="data">
            {{ data.item.averageFantasyPoints | score(2) }}
          </template>
          <template #cell(averageCriticScore)="data">
            {{ data.item.averageCriticScore | score(2) }}
          </template>
        </b-table>

        <div class="hall-of-fame">
          <div class="hall-of-fame-lists">
            <hallOfFameSection v-for="hallOfFameList in leagueAllTimeStats.hallOfFameGameLists" :key="hallOfFameList.name" :hallOfFameList="hallOfFameList"></hallOfFameSection>
          </div>
        </div>
        <h2>Individual Year Stats</h2>
        <b-table :sort-by.sync="individualSortBy" :sort-desc.sync="individualSortDesc" :items="leagueAllTimeStats.publishers" :fields="publisherFields" bordered small responsive striped>
          <template #cell(publisherName)="data">
            <router-link :to="{ name: 'publisher', params: { publisherid: data.item.publisherID } }">
              {{ data.item.publisherName }}
            </router-link>
          </template>
          <template #cell(year)="data">
            <router-link :to="{ name: 'league', params: { leagueid: league.leagueID, year: data.item.year } }">
              {{ data.item.year }}
            </router-link>
          </template>
          <template #cell(ranking)="data">
            <span class="standings-position">{{ ordinal_suffix_of(data.item.ranking) }}</span>
          </template>
          <template #cell(totalFantasyPoints)="data">
            {{ data.item.totalFantasyPoints | score(2) }}
          </template>
          <template #cell(averageCriticScore)="data">
            {{ data.item.averageCriticScore | score(2) }}
          </template>
        </b-table>
      </div>
      <div v-else>
        <b-alert variant="warning" show>This page is not available until you have completed at least two years of playing!</b-alert>
      </div>
    </div>
  </div>
</template>
<script>
import axios from 'axios';
import { ordinal_suffix_of } from '@/globalFunctions';
import HallOfFameSection from '@/components/hallOfFameSection.vue';

export default {
  components: { HallOfFameSection },
  props: {
    leagueid: { type: String, required: true }
  },
  data() {
    return {
      errorInfo: '',
      forbidden: false,
      leagueAllTimeStats: null,
      playerStatFieldsInternal: [
        { key: 'playerName', label: 'User', thClass: 'bg-primary', sortable: true },
        { key: 'yearsPlayedIn', label: 'Years Played In', thClass: 'bg-primary', sortable: true },
        { key: 'yearsWon.length', label: 'Years Won', thClass: 'bg-primary', sortable: true },
        { key: 'totalFantasyPoints', label: 'Points', thClass: 'bg-primary', sortable: true },
        { key: 'gamesReleased', label: 'Games Released', thClass: 'bg-primary', sortable: true },
        { key: 'averageFinishRanking', label: 'Average Rank', thClass: 'bg-primary', sortable: true },
        { key: 'averageFantasyPoints', label: 'Average Fantasy Points', thClass: 'bg-primary', sortable: true },
        { key: 'averageCriticScore', label: 'Average Critic Score', thClass: 'bg-primary', sortable: true }
      ],
      playerSortBy: 'totalFantasyPoints',
      playerSortDesc: true,
      publisherFields: [
        { key: 'playerName', label: 'User', thClass: 'bg-primary', sortable: true },
        { key: 'year', label: 'Year', thClass: 'bg-primary', sortable: true },
        { key: 'publisherName', label: 'Publisher', thClass: 'bg-primary' },
        { key: 'ranking', label: 'Rank', thClass: 'bg-primary', sortable: true },
        { key: 'totalFantasyPoints', label: 'Points', thClass: 'bg-primary', sortable: true },
        { key: 'gamesReleased', label: 'Games Released', thClass: 'bg-primary', sortable: true },
        { key: 'averageCriticScore', label: 'Average Critic Score', thClass: 'bg-primary', sortable: true }
      ],
      individualSortBy: 'totalFantasyPoints',
      individualSortDesc: true
    };
  },
  computed: {
    league() {
      return this.leagueAllTimeStats?.league;
    },
    playerStatFields() {
      const yearsPlayedIn = this.leagueAllTimeStats.playerAllTimeStats.map((x) => x.yearsPlayedIn);
      const allSameYearsPlayed = yearsPlayedIn.every((x) => x === yearsPlayedIn[0]);
      if (!allSameYearsPlayed) {
        return this.playerStatFieldsInternal;
      }

      return this.playerStatFieldsInternal.slice(0, 1).concat(this.playerStatFieldsInternal.slice(2));
    }
  },
  watch: {
    $route(to, from) {
      if (to.path !== from.path) {
        this.initializePage();
      }
    }
  },
  async created() {
    await this.initializePage();
  },
  methods: {
    ordinal_suffix_of,
    async initializePage() {
      try {
        const response = await axios.get('/api/League/GetLeagueAllTimeStats/' + this.leagueid);
        this.leagueAllTimeStats = response.data;
      } catch (error) {
        this.errorInfo = error.response.data;
      }
    }
  }
};
</script>
<style scoped>
.won-years span {
  margin-right: 2px;
}

.year-winner-crown {
  color: #d6993a;
}

.hall-of-fame-lists {
  display: flex;
  flex-direction: column;
  gap: 50px;
  margin-bottom: 50px;
}
</style>
