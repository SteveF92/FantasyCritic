<template>
  <div>
    <div class="row league-header">
      <h1 class="header">Public Leagues</h1>
      <div class="year-selector">
        <b-form-select v-model="selectedYear" :options="supportedYears" v-on:change="fetchPublicLeaguesForYear" />
      </div>
    </div>
    <div class="row leagues-table" v-if="publicLeagues && publicLeagues.length > 0">
      <b-table :sort-by.sync="sortBy"
               :sort-desc.sync="sortDesc"
               :items="publicLeagues"
               :fields="leagueFields"
               bordered
               striped
               responsive>
        <template slot="leagueName" slot-scope="data">
          <router-link :to="{ name: 'league', params: { leagueid: data.item.leagueID, year: selectedYear }}">{{data.item.leagueName}}</router-link>
        </template>
      </b-table>
    </div>
  </div>
</template>

<script>
  import Vue from 'vue';
  import axios from "axios";
  import moment from "moment";

  export default {
    data() {
      return {
        selectedYear: null,
        supportedYears: [],
        publicLeagues: [],
        leagueFields: [
          { key: 'leagueName', label: 'Name', sortable: true, thClass: 'bg-primary' },
          { key: 'numberOfFollowers', label: 'Number of Followers', sortable: true, thClass: 'bg-primary' },
          { key: 'playStatus', label: 'Play Status', sortable: true, thClass:'bg-primary' },
        ],
        sortBy: 'numberOfFollowers',
        sortDesc: true
      }
    },
    methods: {
      fetchSupportedYears() {
        axios
          .get('/api/game/SupportedYears')
          .then(response => {
            this.supportedYears = response.data;
            this.selectedYear = this.supportedYears[0];
            this.fetchPublicLeaguesForYear(this.selectedYear);
          })
          .catch(response => {

          });
      },
      fetchPublicLeaguesForYear(year) {
        axios
          .get('/api/league/PublicLeagues/' + year)
          .then(response => {
            this.publicLeagues = response.data;
          })
          .catch(response => {

          });
      },
    },
    mounted() {
      this.fetchSupportedYears();
    }
  }
</script>
<style scoped>
  .header {
    max-width: 80%;
  }
  .year-selector {
    position: absolute;
    right: 0px;
  }

  .leagues-table {
    margin-left: 15px;
    margin-right: 15px;
  }
</style>
