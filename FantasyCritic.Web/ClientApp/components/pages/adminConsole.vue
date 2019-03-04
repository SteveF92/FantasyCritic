<template>
  <div>
    <h1>Admin Console</h1>
    <div class="alert alert-danger" v-show="errorInfo">{{errorInfo}}</div>
    <div class="alert alert-info" v-show="isBusy">Request is processing...</div>
    <div class="alert alert-success" v-show="jobSuccess">{{jobSuccess}} sucessfully run.</div>
    <b-button variant="info" v-on:click="refreshCriticScores">Refresh Critic Scores</b-button>
  </div>
</template>
<script>
  import axios from 'axios';

  export default {
    data() {
      return {
        isBusy: false,
        errorInfo: "",
        jobSuccess: ""
      }
    },
    computed: {

    },
    methods: {
      refreshCriticScores() {
        this.isBusy = true;
        axios
          .post('/api/admin/RefreshCriticInfo')
          .then(response => {
            this.isBusy = false;
            this.jobSuccess = "Refresh Critic Scores";
          })
          .catch(returnedError => {
            this.errorInfo = returnedError.response.data;
          });
      }
    }
  }
</script>
