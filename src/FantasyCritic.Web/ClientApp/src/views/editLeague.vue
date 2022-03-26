<template>
  <div>
    <div class="col-md-10 offset-md-1 col-sm-12">
      <h1>Edit League Year Settings</h1>
      <hr />
      <div v-show="errorInfo" class="alert alert-danger">
        <h2>Error!</h2>
        <p>{{ errorInfo }}</p>
      </div>

      <div v-if="possibleLeagueOptions && leagueYearSettings && leagueYear">
        <div class="text-well league-options">
          <leagueYearSettings
            v-model="leagueYearSettings"
            :year="year"
            :possible-league-options="possibleLeagueOptions"
            :edit-mode="true"
            :current-number-of-players="activePlayersInLeague"
            :fresh-settings="freshSettings"></leagueYearSettings>
        </div>

        <div v-show="!leagueYearIsValid" class="alert alert-warning disclaimer">Some of your settings are invalid.</div>

        <div class="form-group">
          <b-button class="col-10 offset-1" variant="primary" :disabled="!leagueYearIsValid" @click="postRequest">Confirm Settings</b-button>
        </div>
      </div>
    </div>
  </div>
</template>
<script>
import axios from 'axios';
import LeagueYearSettings from '@/components/leagueYearSettings';

export default {
  components: {
    LeagueYearSettings
  },
  props: ['leagueid', 'year'],
  data() {
    return {
      errorInfo: '',
      possibleLeagueOptions: null,
      leagueYearSettings: null,
      leagueYear: null,
      freshSettings: false
    };
  },
  computed: {
    leagueYearIsValid() {
      let valid =
        this.leagueYearSettings &&
        this.leagueYearSettings.standardGames >= 1 &&
        this.leagueYearSettings.standardGames <= 50 &&
        this.leagueYearSettings.gamesToDraft >= 1 &&
        this.leagueYearSettings.gamesToDraft <= 50 &&
        this.leagueYearSettings.counterPicks >= 0 &&
        this.leagueYearSettings.counterPicks <= 20 &&
        this.leagueYearSettings.counterPicksToDraft >= 0 &&
        this.leagueYearSettings.counterPicksToDraft <= 20;
      return valid;
    },
    activePlayersInLeague() {
      if (!this.leagueYear || !this.leagueYear.players) {
        return null;
      }
      return this.leagueYear.players.length;
    }
  },
  mounted() {
    this.freshSettings = this.$route.query.freshSettings;
    this.fetchLeagueOptions();
    this.fetchCurrentLeagueYearOptions();
    this.fetchLeagueYear();
  },
  methods: {
    fetchLeagueOptions() {
      axios
        .get('/api/League/LeagueOptions')
        .then((response) => {
          this.possibleLeagueOptions = response.data;
        })
        .catch((returnedError) => (this.error = returnedError));
    },
    fetchLeagueYear() {
      axios
        .get('/api/League/GetLeagueYear?leagueID=' + this.leagueid + '&year=' + this.year)
        .then((response) => {
          this.leagueYear = response.data;
        })
        .catch((returnedError) => (this.error = returnedError));
    },
    fetchCurrentLeagueYearOptions() {
      axios
        .get('/api/League/GetLeagueYearOptions?leagueID=' + this.leagueid + '&year=' + this.year)
        .then((response) => {
          this.leagueYearSettings = response.data;
        })
        .catch((returnedError) => (this.error = returnedError));
    },
    postRequest() {
      axios.post('/api/leagueManager/EditLeagueYearSettings', this.leagueYearSettings).then(this.responseHandler).catch(this.catchHandler);
    },
    responseHandler() {
      this.$router.push({ name: 'league', params: { leagueid: this.leagueid, year: this.year } });
    },
    catchHandler(returnedError) {
      this.errorInfo = returnedError.response.data;
      window.scroll({
        top: 0,
        left: 0,
        behavior: 'smooth'
      });
    }
  }
};
</script>
<style scoped>
.league-options {
  margin-bottom: 10px;
}
</style>
