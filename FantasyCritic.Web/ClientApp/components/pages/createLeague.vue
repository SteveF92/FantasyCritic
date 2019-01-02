<template>
  <div>
    <h1>Create a league</h1>
    <hr />
    <div class="alert alert-danger" v-show="errorInfo">
      <h2>Error!</h2>
      <p>{{errorInfo}}</p>
    </div>
    <form v-if="possibleLeagueOptions" method="post" class="form-horizontal" role="form" v-on:submit.prevent="postRequest">
      <div class="form-group col-8">
        <label for="leagueName" class="control-label">League Name</label>
        <input v-model="leagueName" v-validate="'required'" id="leagueName" name="leagueName" type="text" class="form-control input" />
        <span class="text-danger">{{ errors.first('leagueName') }}</span>
      </div>
      <hr />
      <div class="form-group col-8">
        <label for="intendedNumberOfPlayers" class="control-label">How many players do you think will be in this league?</label>
        <input v-model="intendedNumberOfPlayers" v-validate="'required|min_value:2|max_value:14'" id="intendedNumberOfPlayers" name="intendedNumberOfPlayers" type="text" class="form-control input" />
        <span class="text-danger">{{ errors.first('intendedNumberOfPlayers') }}</span>
      </div>

      <div v-if="readyToChooseNumbers()">
        <label>Based on your number of players, we recommend the following settings. However, you are free to change this.</label>

        <div class="form-group col-8">
          <label for="pickupGames" class="control-label">Total Number of Games</label>
          <p>
            This is the total number of games that each player will have on their roster.
          </p>
         
          <input v-model="standardGames" v-validate="'required|min_value:1|max_value:30'" id="standardGames" name="standardGames" type="text" class="form-control input" />
          <span class="text-danger">{{ errors.first('standardGames') }}</span>
        </div>

        <div class="form-group col-8">
          <label for="gamesToDraft" class="control-label">Number of Games to Draft</label>
          <p>
            This is the number of games that will be chosen by each player at the draft.
            If this number is lower than the "Total Number of Games", the remainder will be
            <a href="/faq#bidding-system" target="_blank">
              Pickup Games.
            </a>
          </p>
          <input v-model="gamesToDraft" v-validate="'required|min_value:1|max_value:30'" id="gamesToDraft" name="gamesToDraft" type="text" class="form-control input" />
          <span class="text-danger">{{ errors.first('gamesToDraft') }}</span>
        </div>

        <div class="form-group col-8">
          <label for="counterPicks" class="control-label">Number of Counter Picks</label>
          <p>
            Counter picks are essentially bets against a game. For more details,
            <a href="/faq#scoring" target="_blank">
              click here.
            </a>
          </p>
          <input v-model="counterPicks" v-validate="'required|max_value:5'" id="counterPicks" name="counterPicks" type="text" class="form-control input" />
          <span class="text-danger">{{ errors.first('counterPicks') }}</span>
        </div>
        <hr />

        <div class="form-group col-8">
          <label for="intialYear" class="control-label">Year to Play</label>
          <p>
            The best time to start a game is at the beginning of the year, the earlier the better. You are free to start playing as early as the Decemeber before the new year begins.
          </p>
          <select class="form-control" v-model="initialYear" id="initialYear">
            <option v-for="initialYear in possibleLeagueOptions.openYears" v-bind:value="initialYear">{{ initialYear }}</option>
          </select>
          <span class="text-danger">{{ errors.first('initialYear') }}</span>
        </div>
        <hr />
      </div>
      <div v-if="readyToChooseLevels()">
        <div class="form-group col-10 eligibility-section">
          <label class="control-label eligibility-slider-label">Maximum Eligibility Level</label>
          <p class="eligibility-explanation">
            Eligibility levels are designed to prevent people from taking "uninteresting" games. Setting this to a low number means being more restrictive, setting it to a higher number means being
            more lenient. For more details, 
            <a href="/faq#eligibility" target="_blank">
              click here.
            </a>
          </p>
          <vue-slider v-model="maximumEligibilityLevel" :min="minimumPossibleEligibilityLevel" :max="maximumPossibleEligibilityLevel"
                      piecewise piecewise-label :piecewise-style="piecewiseStyle">
          </vue-slider>
          <div class="eligibility-description">
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
                <p>These are often pretty safe bets, but the scores vary just enough year to year for me to recommend that you leave this on.</p>
              </b-form-checkbox>
            </div>
            <div>
              <b-form-checkbox id="early-access-checkbox" v-model="allowEarlyAccess">
                <span class="checkbox-label">Allow Early Access Games</span>
                <p>If this is left unchecked, a game that is already in early access will not be selectable. Games that are planned for early access that are not yet playable are always selectable.</p>
              </b-form-checkbox>
            </div>
          </div>
        </div>
        <div class="form-group">
          <div class="col-md-offset-2 col-8">
            <input type="submit" class="btn btn-primary" value="Create League" />
          </div>
          <div class="alert alert-info disclaimer">
            Reminder: All of these settings can always be changed later.
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
  import Popper from 'vue-popperjs';

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
      vueSlider,
      'popper': Popper,
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
.eligibility-explanation {
  margin-bottom: 50px;
  max-width: 1300px;
}

.eligibility-section {
  margin-bottom: 10px;
}

.eligibility-description {
  margin-top: 25px;
}

.checkbox-label {
  padding-left: 25px;
}

.disclaimer {
  margin-top: 10px;
}
</style>
<style>
  .vue-slider-piecewise-label {
    color: white !important;
  }
</style>
