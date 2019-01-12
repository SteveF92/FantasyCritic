<template>
  <div>
    <div class="row league-header">
      <h1>Master Games List</h1>
      <div class="year-selector">
        <div>
          <b-form-select v-model="selectedYear" :options="supportedYears" v-on:change="fetchGamesForYear" />
        </div>
      </div>
    </div>
    <div class="row games-table" v-if="gamesForYear && gamesForYear.length > 0">
      <b-table :sort-by.sync="sortBy"
               :sort-desc.sync="sortDesc"
               :items="gamesForYear"
               :fields="gameFields"
               bordered
               striped
               responsive>
        <template slot="gameName" slot-scope="data">
          <masterGamePopover :masterGame="data.item"></masterGamePopover>
        </template>
        <template slot="releaseDate" slot-scope="data">
          {{getReleaseDate(data.item)}}
        </template>
        <template slot="isReleased" slot-scope="data">
          {{data.item.isReleased | yesNo}}
        </template>
      </b-table>
    </div>
  </div>
</template>

<script>
  import Vue from 'vue';
  import axios from "axios";
  import moment from "moment";
  import MasterGamePopover from "components/modules/masterGamePopover";

  export default {
    data() {
      return {
        selectedYear: null,
        supportedYears: [],
        gamesForYear: [],
        gameFields: [
          { key: 'gameName', label: 'Name', sortable: true, thClass:'bg-primary' },
          { key: 'releaseDate', label: 'Release Date', sortable: true, thClass: 'bg-primary' },
          { key: 'isReleased', label: 'Released?', sortable: true, thClass: 'bg-primary' },
          { key: 'criticScore', label: 'Critic Score', sortable: true, thClass: 'bg-primary' },
          { key: 'hypeFactor', label: 'Hype Factor', sortable: true, thClass: 'bg-primary' },
          { key: 'percentStandardGame', label: '% Picked', sortable: true, thClass: 'bg-primary' },
          { key: 'percentCounterPick', label: '% Counter Picked', sortable: true, thClass: 'bg-primary' },
          { key: 'averageDraftPosition', label: 'Avg. Draft Position', sortable: true, thClass: 'bg-primary' }
        ],
        sortBy: 'gameName',
        sortDesc: true
      }
    },
    components: {
      MasterGamePopover
    },
    methods: {
      fetchSupportedYears() {
        axios
          .get('/api/game/SupportedYears')
          .then(response => {
            this.supportedYears = response.data;
            this.selectedYear = this.supportedYears[0];
            this.fetchGamesForYear(this.selectedYear);
          })
          .catch(response => {

          });
      },
      fetchGamesForYear(year) {
        axios
          .get('/api/game/MasterGameYear/' + year)
          .then(response => {
            this.gamesForYear = response.data;
          })
          .catch(response => {

          });
      },
      getReleaseDate(game) {
        if (game.releaseDate) {
          return moment(game.releaseDate).format('MMMM Do, YYYY');
        }
        return game.estimatedReleaseDate + ' (Estimated)'
      },
    },
    mounted() {
      this.fetchSupportedYears();
    }
  }
</script>
<style scoped>
  .year-selector {
    position: absolute;
    right: 0px;
  }

  .year-selector div {
    float: left;
  }
  .games-table {
    margin-left: 15px;
    margin-right: 15px;
  }
</style>
