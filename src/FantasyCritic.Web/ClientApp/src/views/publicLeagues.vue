<template>
  <div v-if="interLeagueDataLoaded">
    <div class="col-md-10 offset-md-1 col-sm-12">
      <div class="row league-header">
        <h1 class="header">Public Leagues</h1>
        <div class="year-selector">
          <b-form-select v-model="selectedYear" :options="supportedYears" @change="fetchPublicLeaguesForYear" />
        </div>
      </div>
      <div v-if="publicLeagues && publicLeagues.length > 0" class="row">
        <b-form-group>
          <label>Search by League Name</label>
          <b-input-group size="sm">
            <b-form-input id="filter-input" v-model="filter" type="search" placeholder="Type to Search"></b-form-input>
          </b-input-group>
        </b-form-group>

        <b-table
          :sort-by.sync="sortBy"
          :sort-desc.sync="sortDesc"
          :items="publicLeagues"
          :fields="leagueFields"
          bordered
          striped
          responsive
          :per-page="perPage"
          :current-page="currentPage"
          :filter="filter"
          :filter-included-fields="filterOn"
          @filtered="onFiltered">
          <template #cell(leagueName)="data">
            <router-link :to="{ name: 'league', params: { leagueid: data.item.leagueID, year: selectedYear } }">{{ data.item.leagueName }}</router-link>
          </template>
        </b-table>
        <b-pagination v-model="currentPage" class="pagination-dark" :total-rows="rows" :per-page="perPage" aria-controls="my-table"></b-pagination>
      </div>
    </div>
  </div>
</template>

<script>
import axios from 'axios';
import _ from 'lodash';

export default {
  data() {
    return {
      perPage: 10,
      currentPage: 1,
      filter: null,
      filterOn: ['leagueName'],
      selectedYear: null,
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
  computed: {
    rows() {
      return this.publicLeagues.length;
    }
  },
  async mounted() {
    this.selectedYear = this.supportedYears.filter((x) => x.openForPlay)[0].year;
    await this.fetchPublicLeaguesForYear(this.selectedYear);
  },
  methods: {
    onFiltered(filteredItems) {
      // Trigger pagination to update the number of buttons/pages due to filtering
      this.totalRows = filteredItems.length;
      this.currentPage = 1;
    },
    async fetchPublicLeaguesForYear(year) {
      const response = await axios.get('/api/league/PublicLeagues/' + year);
      this.publicLeagues = response.data;
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
