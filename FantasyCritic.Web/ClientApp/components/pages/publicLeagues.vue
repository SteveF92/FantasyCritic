<template>
  <div>
    <div class="row league-header">
      <h1 class="header">Public Leagues</h1>
      <div class="year-selector">
        <b-form-select v-model="selectedYear" :options="supportedYears" v-on:change="fetchPublicLeaguesForYear" />
      </div>
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
          { key: 'gameName', label: 'Name', sortable: true, thClass:'bg-primary' },
          { key: 'releaseDate', label: 'Release Date', sortable: true, thClass: 'bg-primary' },
          { key: 'isReleased', label: 'Released?', sortable: true, thClass: 'bg-primary' },
          { key: 'criticScore', label: 'Critic Score', thClass: 'bg-primary' },
          { key: 'hypeFactor', label: 'Hype Factor', sortable: true, thClass: 'bg-primary' },
          { key: 'percentStandardGame', label: '% Picked', sortable: true, thClass: 'bg-primary' },
          { key: 'percentCounterPick', label: '% Counter Picked', sortable: true, thClass: 'bg-primary' },
          { key: 'averageDraftPosition', label: 'Avg. Draft Position', sortable: true, thClass: 'bg-primary' },
          { key: 'eligibilityLevel', label: 'Eligibility Level', sortable: true, thClass: 'bg-primary' }
        ],
        sortBy: 'gameName',
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
