<template>
  <div class="col-md-10 offset-md-1 col-sm-12">
    <div v-if="forbidden">
      <div class="alert alert-danger" role="alert">You do not have permission to view this league.</div>
    </div>

    <div v-if="league && !league.publicLeague && !(league.userIsInLeague || league.outstandingInvite)" class="alert alert-warning" role="info">You are viewing a private league.</div>

    <div v-if="leagueAllTimeStats">
      <h1>{{ league.leagueName }} All Time Stats</h1>
      <hr />
    </div>
  </div>
</template>
<script>
import axios from 'axios';

export default {
  props: {
    leagueid: { type: String, required: true }
  },
  data() {
    return {
      errorInfo: '',
      forbidden: false,
      leagueAllTimeStats: null
    };
  },
  computed: {
    league() {
      return this.leagueAllTimeStats?.league;
    }
  },
  watch: {
    $route(to, from) {
      if (to.path !== from.path) {
        this.initializePage();
      }
    }
  },
  async created() {
    await this.initializePage();
  },
  methods: {
    async initializePage() {
      try {
        const response = await axios.get('/api/League/GetLeagueAllTimeStats/' + this.leagueid);
        this.leagueAllTimeStats = response.data;
      } catch (error) {
        this.errorInfo = error.response.data;
      }
    }
  }
};
</script>
<style scoped></style>
