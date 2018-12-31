<template>
  <div>
    <h1>Create a league</h1>
    <hr />
    <div class="alert alert-danger" v-show="errorInfo">
      <h2>Error!</h2>
      <p>{{errorInfo}}</p>
    </div>
    <form v-if="possibleLeagueOptions" method="post" class="form-horizontal" role="form" v-on:submit.prevent="postRequest">
      <div class="form-group col-md-10">
        <label for="leagueName" class="control-label">League Name</label>
        <input v-model="leagueName" v-validate="'required'" id="leagueName" name="leagueName" type="text" class="form-control input" />
        <span class="text-danger">{{ errors.first('leagueName') }}</span>
      </div>
      <hr />
      <div class="form-group col-md-10">
        <label for="intendedNumberOfPlayers" class="control-label">How many players do you think will be in this league?</label>
        <input v-model="intendedNumberOfPlayers" v-validate="'required|min_value:2|max_value:14'" id="intendedNumberOfPlayers" name="intendedNumberOfPlayers" type="text" class="form-control input" />
        <span class="text-danger">{{ errors.first('intendedNumberOfPlayers') }}</span>
      </div>

      <div v-if="readyToChooseNumbers()">
        <label>Based on your number of players, we recommend the following settings. However, you are free to change this.</label>

        <div class="form-group col-md-10">
          <label for="pickupGames" class="control-label">Total Number of Games</label>
          <input v-model="standardGames" v-validate="'required|min_value:1|max_value:30'" id="standardGames" name="standardGames" type="text" class="form-control input" />
          <span class="text-danger">{{ errors.first('standardGames') }}</span>
        </div>

        <div class="form-group col-md-10">
          <label for="gamesToDraft" class="control-label">Number of Games to Draft</label>
          <input v-model="gamesToDraft" v-validate="'required|min_value:1|max_value:30'" id="gamesToDraft" name="gamesToDraft" type="text" class="form-control input" />
          <span class="text-danger">{{ errors.first('gamesToDraft') }}</span>
        </div>

        <div class="form-group col-md-10">
          <label for="counterPicks" class="control-label">Number of Counter Picks</label>
          <input v-model="counterPicks" v-validate="'required|max_value:5'" id="counterPicks" name="counterPicks" type="text" class="form-control input" />
          <span class="text-danger">{{ errors.first('counterPicks') }}</span>
        </div>
        <hr />

        <div class="form-group col-md-10">
          <label for="intialYear" class="control-label">Year to Play</label>
          <select class="form-control" v-model="initialYear" id="initialYear">
            <option v-for="initialYear in possibleLeagueOptions.openYears" v-bind:value="initialYear">{{ initialYear }}</option>
          </select>
          <span class="text-danger">{{ errors.first('initialYear') }}</span>
        </div>
        <hr />
      </div>
      <div v-if="readyToChooseLevels()">
        <div class="form-group col-md-10 eligibility-section">
          <label class="control-label eligibility-slider-label">Maximum Eligibility Level</label>
          <vue-slider v-model="maximumEligibilityLevel" :min="minimumPossibleEligibilityLevel" :max="maximumPossibleEligibilityLevel"
                      piecewise piecewise-label :piecewise-style="piecewiseStyle">
          </vue-slider>
          <div>
            <h3>{{ selectedEligibilityLevel.name }}</h3>
            <p>{{ selectedEligibilityLevel.description }}</p>
            <p>Examples: </p>
            <ul>
              <li v-for="example in selectedEligibilityLevel.examples">{{example}}</li>
            </ul>
          </div>

          <div>
            <h3>Other Options</h3>
            <div>
              <b-form-checkbox id="yearly-checkbox" v-model="allowYearlyInstallments">
                <span class="checkbox-label">Allow Yearly Installments (IE Yearly Sports Franchises)</span>
              </b-form-checkbox>
            </div>
            <div>
              <b-form-checkbox id="early-access-checkbox" v-model="allowEarlyAccess">
                <span class="checkbox-label">Allow Early Access Games</span>
              </b-form-checkbox>
            </div>
          </div>
        </div>
        <div class="form-group">
          <div class="col-md-offset-2 col-md-10">
            <input type="submit" class="btn btn-primary" value="Create League" />
          </div>
        </div>
      </div>
    </form>
  </div>
</template>
<script>
import Vue from "vue";
import axios from "axios";
import vueSlider from 'vue-slider-component';

export default {
    data() {
        return {
          errorInfo: "",
          intendedNumberOfPlayers: "",
          leagueName: "",
          standardGames: "",
          gamesToDraft: "",
          counterPicks: "",
          initialYear: "",
          maximumEligibilityLevel: 0,
          allowYearlyInstallments: true,
          allowEarlyAccess: false,
          possibleLeagueOptions: null,
          piecewiseStyle: {
            "backgroundColor": "#ccc",
            "visibility": "visible",
            "width": "12px",
            "height": "20px"
          }
        }
    },
    components: {
      vueSlider
    },
    computed: {
      formIsValid() {
        return !Object.keys(this.fields).some(key => this.fields[key].invalid);
      },
      minimumPossibleEligibilityLevel() {
        return 0;
      },
      maximumPossibleEligibilityLevel() {
        if (!this.possibleLeagueOptions.eligibilityLevels) {
          return 0;
        }
        let maxEligibilityLevel = _.maxBy(this.possibleLeagueOptions.eligibilityLevels, 'level');
        return maxEligibilityLevel.level;
      },
      selectedEligibilityLevel() {
        let matchingLevel = _.filter(this.possibleLeagueOptions.eligibilityLevels, { 'level': this.maximumEligibilityLevel });
        return matchingLevel[0];
      }
    },
    methods: {
        readyToChooseNumbers() {
          let leagueNameValid = this.fields['leagueName'] && this.fields['leagueName'].valid;
          let intendedNumberOfPlayersValid = this.fields['intendedNumberOfPlayers'] && this.fields['intendedNumberOfPlayers'].valid;
          return leagueNameValid && intendedNumberOfPlayersValid;
        },
        readyToChooseLevels() {
          let standardGamesValid = this.fields['standardGames'] && this.fields['standardGames'].valid;
          let gamesToDraftValid = this.fields['gamesToDraft'] && this.fields['gamesToDraft'].valid;
          let counterPicksValid = this.fields['counterPicks'] && this.fields['counterPicks'].valid;
          return standardGamesValid && gamesToDraftValid && counterPicksValid && this.initialYear;
        },
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
    watch: {
      intendedNumberOfPlayers: function (val) {
        let recommendedNumberOfGames = 54;
        this.standardGames = Math.floor(recommendedNumberOfGames / val);
        this.gamesToDraft = Math.floor(this.standardGames * (2 / 3));
        this.counterPicks = Math.floor(this.gamesToDraft / 6);
        if (this.counterPicks === 0) {
          this.counterPicks = 1;
        }
      }
    },
    mounted() {
        this.fetchLeagueOptions();
    }
}
</script>
<style scoped>
.eligibility-slider-label {
  margin-bottom: 40px;
}
.eligibility-section {
  margin-bottom: 10px;
}
  .checkbox-label {
    padding-left: 25px;
  }
</style>
