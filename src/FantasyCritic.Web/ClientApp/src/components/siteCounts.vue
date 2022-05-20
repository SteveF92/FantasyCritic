<template>
  <div>
    <div v-if="siteCounts" class="row counts-area bg-secondary">
      <div class="col-md-12 col-lg-10 offset-md-0 offset-lg-1">
        <div class="row">
          <div class="col-6 col-sm-3">
            <h3>{{ siteCounts.userCount | thousands }}</h3>
            <h4>Users Joined</h4>
          </div>
          <div class="col-6 col-sm-3">
            <h3>{{ siteCounts.leagueCount | thousands }}</h3>
            <h4>Leagues Created</h4>
          </div>
          <div class="col-6 col-sm-3">
            <h3>{{ siteCounts.masterGameCount | thousands }}</h3>
            <h4>Unique Games</h4>
          </div>
          <div class="col-6 col-sm-3">
            <h3>{{ siteCounts.publisherGameCount | thousands }}</h3>
            <h4>Games Drafted</h4>
          </div>
        </div>
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
      error: ''
    };
  },
  mounted() {
    this.fetchSiteCounts();
  },
  methods: {
    fetchSiteCounts() {
      axios
        .get('/api/general/sitecounts')
        .then((response) => {
          this.siteCounts = response.data;
        })
        .catch((returnedError) => (this.error = returnedError));
    }
  }
};
</script>

<style scoped>
.counts-area {
  text-align: center;
}
</style>
