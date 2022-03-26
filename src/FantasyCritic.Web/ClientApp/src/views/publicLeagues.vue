<template>
  <div>
    <div class="col-md-10 offset-md-1 col-sm-12">
      <div class="row league-header">
        <h1 class="header">Public Leagues</h1>
        <div class="year-selector">
          <b-form-select v-model="selectedYear" :options="supportedYears" @change="fetchPublicLeaguesForYear" />
        </div>
      </div>
      <div v-if="publicLeagues && publicLeagues.length > 0" class="row">
        <b-table :sort-by.sync="sortBy" :sort-desc.sync="sortDesc" :items="publicLeagues" :fields="leagueFields" bordered striped responsive>
          <template #cell(leagueName)="data">
            <router-link :to="{ name: 'league', params: { leagueid: data.item.leagueID, year: selectedYear } }">{{ data.item.leagueName }}</router-link>
          </template>
        </b-table>
      </div>
    </div>
  </div>
</template>

<script>
import axios from 'axios';

export default {
  data() {
    return {
      selectedYear: null,
      supportedYears: [],
      publicLeagues: [],
      leagueFields: [
        { key: 'leagueName', label: 'Name', sortable: true, thClass: 'bg-primary' },
        { key: 'numberOfFollowers', label: 'Number of Followers', sortable: true, thClass: 'bg-primary' },
        { key: 'playStatus', label: 'Play Status', sortable: true, thClass: 'bg-primary' }
      ],
      sortBy: 'numberOfFollowers',
      sortDesc: true
    };
  },
  mounted() {
    this.fetchSupportedYears();
  },
  methods: {
    fetchSupportedYears() {
      axios
        .get('/api/game/SupportedYears')
        .then((response) => {
          let supportedYears = response.data;
          let openYears = _.filter(supportedYears, { openForPlay: true });
          let finishedYears = _.filter(supportedYears, { finished: true });
          this.supportedYears = openYears.concat(finishedYears).map(function (v) {
            return v.year;
          });
          this.selectedYear = this.supportedYears[0];
          this.fetchPublicLeaguesForYear(this.selectedYear);
        })
        .catch(() => {});
    },
    fetchPublicLeaguesForYear(year) {
      axios
        .get('/api/league/PublicLeagues/' + year)
        .then((response) => {
          this.publicLeagues = response.data;
        })
        .catch(() => {});
    }
  }
};
</script>
<style scoped>
.header {
  max-width: 80%;
}
.year-selector {
  position: absolute;
  right: 0px;
}
</style>
