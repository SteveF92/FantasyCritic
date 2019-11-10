<template>
  <div>
    <div class="col-md-10 offset-md-1 col-sm-12">
      <h1>Create a league</h1>
      <hr />
      <div class="alert alert-danger" v-show="errorInfo">
        <h2>Error!</h2>
        <p>{{errorInfo}}</p>
      </div>
      <div class="text-well" v-if="possibleLeagueOptions">
        <h2>Basic Settings</h2>
        <div class="form-group">
          <label for="leagueName" class="control-label">League Name</label>
          <input v-model="leagueName" v-validate="'required'" id="leagueName" name="leagueName" type="text" class="form-control input" />
          <span class="text-danger">{{ errors.first('leagueName') }}</span>
        </div>
        <hr />
        <div class="form-group">
          <label for="intialYear" class="control-label">Year to Play</label>
          <p>
            The best time to start a game is at the beginning of the year, the earlier the better. You are free to start playing as early as the Decemeber before the new year begins.
          </p>
          <select class="form-control" v-model="initialYear" id="initialYear">
            <option v-for="initialYear in possibleLeagueOptions.openYears" v-bind:value="initialYear">{{ initialYear }}</option>
          </select>
          <span class="text-danger">{{ errors.first('initialYear') }}</span>
        </div>
      </div>

      <div v-if="readyToSetupLeagueYear">
        <hr />
        <div class="text-well">
          <leagueYearSettings :year="initialYear" :possibleLeagueOptions="possibleLeagueOptions" v-model="leagueYearSettings"></leagueYearSettings>
        </div>
      </div>

      <div v-if="readyToSetupLeagueYear && leagueYearSettings">
        <hr />
        <div class="text-well">
          <h2>Other Options</h2>
          <div>
            <b-form-checkbox v-model="publicLeague">
              <span class="checkbox-label">Public League</span>
              <p>If checked, everyone will be able to see your league. Players still need to be invited to join. If unchecked, your league will only be viewable by its members.</p>
            </b-form-checkbox>
          </div>
          <div>
            <b-form-checkbox v-model="testLeague">
              <span class="checkbox-label">Test League</span>
              <p>If checked, this league won't affect the site's overall stats. Please check this if you are just testing out the site.</p>
            </b-form-checkbox>
          </div>
        </div>

        <hr />
        <div class="alert alert-info disclaimer">
          Reminder: All of these settings can always be changed later.
        </div>

        <div class="form-group">
          <b-button class="col-10 offset-1" variant="primary" v-on:click="postRequest">Create League</b-button>
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
        leagueName: "",
        initialYear: "",
        leagueYearSettings: null,
        publicLeague: true,
        testLeague: false,
      }
    },
    components: {
      LeagueYearSettings
    },
    computed: {
      readyToSetupLeagueYear() {
        let leagueNameValid = this.veeFields['leagueName'] && this.veeFields['leagueName'].valid;
        return leagueNameValid && this.initialYear;
      }
    },
    methods: {
      fetchLeagueOptions() {
        axios
          .get('/api/League/LeagueOptions')
          .then(response => {
            this.possibleLeagueOptions = response.data;
            this.maximumEligibilityLevel = this.possibleLeagueOptions.defaultMaximumEligibilityLevel;
          })
          .catch(returnedError => (this.error = returnedError));
      },
      postRequest() {
        let selectedLeagueOptions = {
          leagueName: this.leagueName,
          initialYear: this.initialYear,
          standardGames: this.leagueYearSettings.standardGames,
          gamesToDraft: this.leagueYearSettings.gamesToDraft,
          counterPicks: this.leagueYearSettings.counterPicks,
          maximumEligibilityLevel: this.leagueYearSettings.maximumEligibilityLevel,
          allowYearlyInstallments: this.leagueYearSettings.allowYearlyInstallments,
          allowEarlyAccess: this.leagueYearSettings.allowEarlyAccess,
          allowFreeToPlay: this.leagueYearSettings.allowFreeToPlay,
          allowReleasedInternationally: this.leagueYearSettings.allowReleasedInternationally,
          allowExpansions: this.leagueYearSettings.allowExpansions,
          publicLeague: this.publicLeague,
          testLeague: this.testLeague,
          draftSystem: "Flexible",
          pickupSystem: "Budget",
          scoringSystem: "Standard"
        };

        axios
          .post('/api/leagueManager/createLeague', selectedLeagueOptions)
          .then(response => {
            this.$router.push({ name: "home" });
          })
          .catch(error => {
            this.errorInfo = error.response.data;
            window.scrollTo(0, 0);
          });
      }
    },
    mounted() {
      this.fetchLeagueOptions();
    }
  }
</script>
<style scoped>
label {
  font-size: 18px;
}

.submit-button {
  text-align: right;
}
</style>
