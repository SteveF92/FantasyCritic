<template>
  <div>
    <div v-if="league">
      <h1>League History: {{league.leagueName}} (Year {{year}})</h1>
      <hr />
      <table class="table table-sm table-responsive-sm table-bordered table-striped">
        <thead>
          <tr class="bg-primary">
            <th scope="col">Publisher</th>
            <th scope="col">Time</th>
            <th scope="col">Action Type</th>
            <th scope="col">Description</th>
            <th scope="col">Manager Action?</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="leagueAction in leagueActions">
            <td>{{leagueAction.publisherName}}</td>
            <td>{{leagueAction.timestamp | date}}</td>
            <td>{{leagueAction.actionType}}</td>
            <td>{{leagueAction.description}}</td>
            <td>{{leagueAction.managerAction | yesNo}}</td>
          </tr>
        </tbody>
      </table>
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
        leagueActions: []
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
