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
        <div v-if="readyToSetupLeagueYear">
          <hr />
          <leagueYearSettings :year="initialYear" :possibleLeagueOptions="possibleLeagueOptions" v-model="leagueYearSettings"></leagueYearSettings>
        </div>
      </div>
    </div>
  </div>
</template>
<script>
  import Vue from "vue";
  import axios from "axios";
  import vueSlider from 'vue-slider-component';
  import Popper from 'vue-popperjs';
  import 'vue-slider-component/theme/antd.css'
  import LeagueYearSettings from "components/modules/leagueYearSettings";

  export default {
    data() {
        return {
          errorInfo: "",
          possibleLeagueOptions: null,
          leagueName: "",
          initialYear: "",
          leagueYearSettings: null
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
          standardGames: this.standardGames,
          gamesToDraft: this.gamesToDraft,
          counterPicks: this.counterPicks,
          initialYear: this.initialYear,
          maximumEligibilityLevel: this.maximumEligibilityLevel,
          allowYearlyInstallments: this.allowYearlyInstallments,
          allowEarlyAccess: this.allowEarlyAccess,
          allowFreeToPlay: this.allowFreeToPlay,
          allowReleasedInternationally: this.allowReleasedInternationally,
          allowExpansions: this.allowExpansions,
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
