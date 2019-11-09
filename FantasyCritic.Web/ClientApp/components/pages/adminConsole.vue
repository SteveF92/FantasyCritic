<template>
  <div>
    <div class="col-md-10 offset-md-1 col-sm-12">
      <h1>Admin Console</h1>
      <div class="alert alert-danger" v-show="errorInfo">{{errorInfo}}</div>
      <div class="alert alert-info" v-show="isBusy">Request is processing...</div>
      <div class="alert alert-success" v-show="jobSuccess">'{{jobSuccess}}' sucessfully run.</div>

      <h2>Requests</h2>
      <div>
        <b-button variant="info" :to="{ name: 'activeMasterGameRequests' }">View master game requests</b-button>
        <b-button variant="info" :to="{ name: 'activeMasterGameChangeRequests' }">View master game change requests</b-button>
        <b-button variant="info" :to="{ name: 'masterGameCreator' }">Add new master game</b-button>
      </div>

      <h2>Data Actions</h2>
      <div>
        <b-button variant="info" v-on:click="fullRefresh">Full Refresh</b-button>
        <b-button variant="info" v-on:click="refreshCriticScores">Refresh Critic Scores</b-button>
        <b-button variant="info" v-on:click="updateFantasyPoints">Update Fantasy Points</b-button>
        <b-button variant="info" v-on:click="refreshCaches">Refresh Caches</b-button>
      </div>

      <h2>Bids</h2>
      <div>
        <b-button variant="info" :to="{ name: 'currentActionedGames' }">Current Actioned Games</b-button>
        <b-button variant="warning" v-on:click="turnOnBidProcessing">Turn on bid processing mode</b-button>
        <b-button variant="info" v-on:click="turnOffBidProcessing">Turn off bid processing mode</b-button>
        <b-button variant="danger" v-on:click="processBids">Process Bids</b-button>
      </div>

      <h2>Database</h2>
      <div>
        <b-button variant="info" v-on:click="getRecentDatabaseSnapshots">Get Recent Database Snapshots</b-button>
        <b-button variant="warning" v-on:click="snapshotDatabase">Snapshot Database</b-button>
      </div>
      <b-table v-if="recentSnapshots" :items="recentSnapshots" striped bordered></b-table>
    </div>
  </div>
</template>
<script>
  import axios from 'axios';

  export default {
    data() {
      return {
        isBusy: false,
        errorInfo: "",
        jobSuccess: "",
        recentSnapshots: null
      }
    },
    computed: {

    },
    methods: {
      fullRefresh() {
        this.isBusy = true;
        axios
          .post('/api/admin/FullDataRefresh')
          .then(response => {
            this.isBusy = false;
            this.jobSuccess = "Full Data Refresh";
          })
          .catch(returnedError => {
            this.isBusy = false;
            this.errorInfo = returnedError.response.data;
          });
      },
      refreshCriticScores() {
        this.isBusy = true;
        axios
          .post('/api/admin/RefreshCriticInfo')
          .then(response => {
            this.isBusy = false;
            this.jobSuccess = "Refresh Critic Scores";
          })
          .catch(returnedError => {
            this.isBusy = false;
            this.errorInfo = returnedError.response.data;
          });
      },
      updateFantasyPoints() {
        this.isBusy = true;
        axios
          .post('/api/admin/updateFantasyPoints')
          .then(response => {
            this.isBusy = false;
            this.jobSuccess = "Update Fantasy Points";
          })
          .catch(returnedError => {
            this.isBusy = false;
            this.errorInfo = returnedError.response.data;
          });
      },
      processBids() {
        this.isBusy = true;
        axios
          .post('/api/admin/ProcessPickups')
          .then(response => {
            this.isBusy = false;
            this.jobSuccess = "Process Bids";
          })
          .catch(returnedError => {
            this.isBusy = false;
            this.errorInfo = returnedError.response.data;
          });
      },
      turnOnBidProcessing() {
        this.isBusy = true;
        axios
          .post('/api/admin/TurnOnBidProcessingMode')
          .then(response => {
            this.isBusy = false;
            this.jobSuccess = "Bid Processing Mode ON";
          })
          .catch(returnedError => {
            this.isBusy = false;
            this.errorInfo = returnedError.response.data;
          });
      },
      turnOffBidProcessing() {
        this.isBusy = true;
        axios
          .post('/api/admin/TurnOffBidProcessingMode')
          .then(response => {
            this.isBusy = false;
            this.jobSuccess = "Bid Processing Mode OFF";
          })
          .catch(returnedError => {
            this.isBusy = false;
            this.errorInfo = returnedError.response.data;
          });
      },
      refreshCaches() {
        this.isBusy = true;
        axios
          .post('/api/admin/refreshCaches')
          .then(response => {
            this.isBusy = false;
            this.jobSuccess = "Refresh Caches";
          })
          .catch(returnedError => {
            this.isBusy = false;
            this.errorInfo = returnedError.response.data;
          });
      },
      snapshotDatabase() {
        this.isBusy = true;
        axios
          .post('/api/admin/snapshotDatabase')
          .then(response => {
            this.isBusy = false;
            this.jobSuccess = "Database snapshot started";
          })
          .catch(returnedError => {
            this.isBusy = false;
            this.errorInfo = returnedError.response.data;
          });
      },
      getRecentDatabaseSnapshots() {
        this.isBusy = true;
        axios
          .get('/api/admin/GetRecentDatabaseSnapshots')
          .then(response => {
            this.recentSnapshots = response.data;
            this.isBusy = false;
            this.jobSuccess = "Getting snapshots";
          })
          .catch(returnedError => {
            this.isBusy = false;
            this.errorInfo = returnedError.response.data;
          });
      },
    }
  }
</script>
