<template>
  <div class="col-md-10 offset-md-1 col-sm-12">
    <div v-if="forbidden">
      <div class="alert alert-danger" role="alert">You do not have permission to view this league.</div>
    </div>

    <div v-if="league && !league.publicLeague && !(league.userIsInLeague || league.outstandingInvite)" class="alert alert-warning" role="info">You are viewing a private league.</div>

    <div v-if="leagueAllTimeStats">
      <h1>{{ league.leagueName }} All Time Stats</h1>
      <hr />
      <h2>Individual Year Stats</h2>
      <b-table :sort-by.sync="sortBy" :sort-desc.sync="sortDesc" :items="leagueAllTimeStats.publishers" :fields="publisherFields" bordered small responsive striped>
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
  </div>
</template>
<script>
import axios from 'axios';
import { ordinal_suffix_of } from '@/globalFunctions';

export default {
  props: {
    leagueid: { type: String, required: true }
  },
  data() {
    return {
      errorInfo: '',
      forbidden: false,
      leagueAllTimeStats: null,
      publisherFields: [
        { key: 'playerName', label: 'User', thClass: 'bg-primary', sortable: true },
        { key: 'year', label: 'Year', thClass: 'bg-primary', sortable: true },
        { key: 'publisherName', label: 'Publisher', thClass: 'bg-primary' },
        { key: 'ranking', label: 'Rank', thClass: 'bg-primary', sortable: true },
        { key: 'totalFantasyPoints', label: 'Points', thClass: 'bg-primary', sortable: true },
        { key: 'gamesReleased', label: 'Games Released', thClass: 'bg-primary', sortable: true },
        { key: 'averageCriticScore', label: 'Average Critic Score', thClass: 'bg-primary', sortable: true }
      ],
      sortBy: 'totalFantasyPoints',
      sortDesc: true
    };
  },
  computed: {
    league() {
      return this.leagueAllTimeStats?.league;
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
<style scoped></style>
