<template>
  <div>
    <div class="col-md-10 offset-md-1 col-sm-12">
      <div v-if="forbidden">
        <div class="alert alert-danger" role="alert">
          You do not have permission to view this league.
        </div>
      </div>
      <div v-if="league">
        <h1>League History: {{league.leagueName}} (Year {{year}})</h1>
        <hr />
        <div class="history-table">
          <b-table :sort-by.sync="sortBy"
                   :sort-desc.sync="sortDesc"
                   :items="leagueActions"
                   :fields="actionFields"
                   bordered
                   striped
                   responsive>
            <template v-slot:cell(timestamp)="data">
              {{data.item.timestamp | dateTime}}
            </template>
            <template v-slot:cell(managerAction)="data">
              {{data.item.managerAction | yesNo}}
            </template>
          </b-table>
        </div>
      </div>
    </div>
  </div>
</template>
<script>
import Vue from 'vue';
import axios from 'axios';

export default {
  data() {
    return {
      errorInfo: '',
      league: null,
      leagueActions: [],
      actionFields: [
        { key: 'publisherName', label: 'Name', sortable: true, thClass: 'bg-primary' },
        { key: 'timestamp', label: 'Timestamp', sortable: true, thClass: 'bg-primary' },
        { key: 'actionType', label: 'Action Type', thClass: 'bg-primary' },
        { key: 'description', label: 'Description', thClass: 'bg-primary' },
        { key: 'managerAction', label: 'Mananger Action?', thClass: 'bg-primary' },
      ],
      sortBy: 'timestamp',
      sortDesc: true,
      forbidden: false
    };
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
};
</script>
<style scoped>
  .history-table {
    margin-left: 15px;
    margin-right: 15px;
  }
</style>
