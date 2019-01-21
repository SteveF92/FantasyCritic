<template>
  <div>
    <div v-if="league">
      <h1>League History: {{league.leagueName}} (Year {{year}})</h1>
      <hr />
      <b-table :sort-by.sync="sortBy"
               :sort-desc.sync="sortDesc"
               :items="leagueActions"
               :fields="actionFields"
               bordered
               striped
               responsive>
        <template slot="timestamp" slot-scope="data">
          {{data.item.timestamp | date}}
        </template>
        <template slot="managerAction" slot-scope="data">
          {{data.item.managerAction | yesNo}}
        </template>
      </b-table>
    </div>
  </div>
</template>
<script>
import Vue from "vue";
import axios from "axios";

export default {
    data() {
      return {
        errorInfo: "",
        league: null,
        leagueActions: [],
        actionFields: [
          { key: 'publisherName', label: 'Name', sortable: true, thClass: 'bg-primary' },
          { key: 'timestamp', label: 'Timestamp', sortable: true, thClass: 'bg-primary' },
          { key: 'actionType', label: 'Action Type', thClass: 'bg-primary' },
          { key: 'description', label: 'Description', thClass: 'bg-primary' },
          { key: 'managerAction', label: 'Mananger Action?', thClass: 'bg-primary' },
        ],
        sortBy: 'gameName',
        sortDesc: true
      }
    },
    props: ['leagueid', 'year'],
    methods: {
      fetchLeagueActions() {
        axios
          .get('/api/League/GetLeagueActions?leagueID=' + this.leagueid + '&year=' + this.year)
          .then(response => {
            this.leagueActions = response.data;
          })
          .catch(returnedError => (this.error = returnedError));
      },
      fetchLeague() {
        axios
          .get('/api/League/GetLeague/' + this.leagueid)
          .then(response => {
            this.league = response.data;
          })
          .catch(returnedError => {
            this.error = returnedError;
            this.forbidden = (returnedError.response.status === 403);
          });
      },
    },
    mounted() {
      this.fetchLeague();
      this.fetchLeagueActions();
    }
}
</script>
