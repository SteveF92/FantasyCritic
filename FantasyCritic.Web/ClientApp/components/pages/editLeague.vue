<template>
  <div>
    <div class="col-md-10 offset-md-1 col-sm-12">
      <h1>Edit League Year Settings</h1>
      <hr />
      <div class="alert alert-danger" v-show="errorInfo">
        <h2>Error!</h2>
        <p>{{errorInfo}}</p>
      </div>

      <div v-if="possibleLeagueOptions && leagueYearSettings">
        <div class="text-well">
          <leagueYearSettings :year="year" :possibleLeagueOptions="possibleLeagueOptions" :editMode="true" v-model="leagueYearSettings"></leagueYearSettings>
        </div>


        <div class="alert alert-warning disclaimer" v-show="!leagueYearIsValid">
          Some of your settings are invalid.
        </div>

        <div class="form-group">
          <b-button class="col-10 offset-1" variant="primary" v-on:click="postRequest" :disabled="!leagueYearIsValid">Confirm Settings</b-button>
        </div>
      </div>
    </div>
  </div>
</template>
<script>
  import axios from "axios";
  import LeagueYearSettings from "components/modules/leagueYearSettings";

  export default {
    data() {
      return {
        errorInfo: "",
        possibleLeagueOptions: null,
        leagueYearSettings: null
      }
    },
    components: {
      LeagueYearSettings
    },
    computed: {
      leagueYearIsValid() {
        let valid = this.leagueYearSettings &&
          this.leagueYearSettings.standardGames >= 1 && this.leagueYearSettings.standardGames <= 50 &&
          this.leagueYearSettings.gamesToDraft >= 1 && this.leagueYearSettings.gamesToDraft <= 50 &&
          this.leagueYearSettings.counterPicks >= 0 && this.leagueYearSettings.counterPicks <= 20;
        return valid;
      }
    },
    props: ['leagueid', 'year'],
    methods: {
      fetchLeagueOptions() {
        axios
          .get('/api/League/LeagueOptions')
          .then(response => {
            this.possibleLeagueOptions = response.data;
          })
          .catch(returnedError => (this.error = returnedError));
      },
      fetchCurrentLeagueYearOptions() {
        axios
          .get('/api/League/GetLeagueYearOptions?leagueID=' + this.leagueid + '&year=' + this.year)
          .then(response => {
            this.leagueYearSettings = response.data;
          })
          .catch(returnedError => (this.error = returnedError));
      },
      postRequest() {
        axios
          .post('/api/leagueManager/EditLeagueYearSettings', this.leagueYearSettings)
          .then(this.responseHandler)
          .catch(this.catchHandler);
      },
      responseHandler(response) {
        this.$router.push({ name: 'league', params: { leagueid: this.leagueid, year: this.year } });
      },
      catchHandler(returnedError) {
        this.errorInfo = returnedError;
      }
    },
    mounted() {
      this.fetchLeagueOptions();
      this.fetchCurrentLeagueYearOptions();
    }
  }
</script>
