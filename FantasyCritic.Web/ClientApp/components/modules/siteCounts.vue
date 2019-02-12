<template>
  <div>
    <div v-if="siteCounts" class="row counts-area">
      <div class="col-3">
        <h3>{{siteCounts.userCount}}</h3>
        <h4>Users Joined</h4>
      </div>
      <div class="col-3">
        <h3>{{siteCounts.leagueCount}}</h3>
        <h4>Leagues Created</h4>
      </div>
      <div class="col-3">
        <h3>{{siteCounts.masterGameCount}}</h3>
        <h4>Unique Games</h4>
      </div>
      <div class="col-3">
        <h3>{{siteCounts.publisherGameCount}}</h3>
        <h4>Games drafted</h4>
      </div>
    </div>
  </div>
</template>

<script>
  import axios from 'axios';

  export default {
    data() {
      return {
        siteCounts: null,
        error: ""
      }
    },
    methods: {
      fetchSiteCounts() {
        axios
          .get('/api/general/sitecounts')
          .then(response => {
            this.siteCounts = response.data;
          })
          .catch(returnedError => (this.error = returnedError));
      }
    },
    mounted() {
      this.fetchSiteCounts();
    }
  }
</script>

<style scoped>
  .counts-area {
    background-color: #414141;
    text-align: center;
  }
</style>
