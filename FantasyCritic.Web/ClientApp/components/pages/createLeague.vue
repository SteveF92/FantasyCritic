<template>
    <div>
        <h2>Create a league</h2>
        <hr />
        <form v-if="possibleLeagueOptions" method="post" class="form-horizontal" role="form" v-on:submit.prevent="postRequest">
          <div class="alert alert-danger" v-if="errorInfo">An error has occurred.</div>
          <div class="form-group col-md-10">
            <label for="leagueName" class="control-label">League Name</label>
            <input v-model="selectedLeagueOptions.leagueName" id="leagueName" name="leagueName" type="text" class="form-control input" />
          </div>
          <hr />

          <div class="form-group col-md-10">
            <label for="pickupGames" class="control-label">Total Number of Games</label>
            <input v-model="selectedLeagueOptions.totalGames" id="totalGames" name="totalGames" type="text" class="form-control input" />
          </div>

          <div class="form-group col-md-10">
            <label for="draftGames" class="control-label">Number of Draft Games</label>
            <input v-model="selectedLeagueOptions.draftGames" id="draftGames" name="draftGames" type="text" class="form-control input" />
          </div>

          <div class="form-group col-md-10">
            <label for="counterPicks" class="control-label">Number of Counter Picks</label>
            <input v-model="selectedLeagueOptions.counterPicks" id="counterPicks" name="counterPicks" type="text" class="form-control input" />
          </div>
          <hr />

          <div class="form-group col-md-10">
            <label for="estimatedCriticScore" class="control-label">Estimated Game Score</label>
            <input v-model="selectedLeagueOptions.estimatedCriticScore" id="estimatedCriticScore" name="estimatedCriticScore" type="text" class="form-control input" />
          </div>
          <div class="form-group col-md-10">
            <label for="intialYear" class="control-label">Year to Play</label>
            <select class="form-control" v-model="selectedLeagueOptions.initialYear" id="initialYear">
              <option v-for="initialYear in possibleLeagueOptions.openYears" v-bind:value="initialYear">{{ initialYear }}</option>
            </select>
          </div>
          <hr />

          <div class="form-group col-md-10">
            <label for="draftSystem" class="control-label">Draft System</label>
            <select class="form-control" v-model="selectedLeagueOptions.draftSystem" id="draftSystem">
              <option v-for="draftSystem in possibleLeagueOptions.draftSystems" v-bind:value="draftSystem">{{ draftSystem }}</option>
            </select>
          </div>
          <div class="form-group col-md-10">
            <label for="pickupSystem" class="control-label">Pickup System</label>
            <select class="form-control" v-model="selectedLeagueOptions.pickupSystem" id="pickupSystem">
              <option v-for="pickupSystem in possibleLeagueOptions.pickupSystems" v-bind:value="pickupSystem">{{ pickupSystem }}</option>
            </select>
          </div>
          <div class="form-group col-md-10">
            <label for="scoringSystem" class="control-label">Scoring System</label>
            <select class="form-control" v-model="selectedLeagueOptions.scoringSystem" id="scoringSystem">
              <option v-for="scoringSystem in possibleLeagueOptions.scoringSystems" v-bind:value="scoringSystem">{{ scoringSystem }}</option>
            </select>
          </div>
          <div class="form-group col-md-10 eligibility-section">
            <label class="control-label eligibility-slider-label">Maximum Eligibility Level</label>
            <vue-slider v-model="selectedLeagueOptions.maximumEligibilityLevel" :min="minimumEligibilityLevel" :max="maximumEligibilityLevel"
                        piecewise piecewise-label :piecewise-style="piecewiseStyle">
            </vue-slider>
            <div>
              <h4>{{ selectedEligibilityLevel.name }}</h4>
              <p>{{ selectedEligibilityLevel.description }}</p>
              <p>Examples: </p>
              <ul>
                <li v-for="example in selectedEligibilityLevel.examples">{{example}}</li>
              </ul>
            </div>

            <div>
              <h4>Other Options</h4>
              <div>
                <b-form-checkbox id="yearly-checkbox"
                                 v-model="selectedLeagueOptions.allowYearlyInstallments">
                  <span class="checkbox-label">Allow Yearly Installments (IE Yearly Sports Franchises)</span>
                </b-form-checkbox>
              </div>
              <div>
                <b-form-checkbox id="early-access-checkbox"
                                 v-model="selectedLeagueOptions.allowEarlyAccess">
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
            possibleLeagueOptions: null,
            selectedLeagueOptions: {
                leagueName: "",
                totalGames: "",
                draftGames: "",
                counterPicks: "",
                estimatedCriticScore: "",
                initialYear: "",
                maximumEligibilityLevel: 0,
                allowYearlyInstallments: true,
                allowEarlyAccess: false,
                draftSystem: "",
                pickupSystem: "",
                scoringSystem: ""
          },
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
      minimumEligibilityLevel() {
        return 0;
      },
      maximumEligibilityLevel() {
        if (!this.possibleLeagueOptions.eligibilityLevels) {
          return 0;
        }
        let maxEligibilityLevel = _.maxBy(this.possibleLeagueOptions.eligibilityLevels, 'level');
        return maxEligibilityLevel.level;
      },
      selectedEligibilityLevel() {
        let matchingLevel = _.filter(this.possibleLeagueOptions.eligibilityLevels, { 'level': this.selectedLeagueOptions.maximumEligibilityLevel });
        return matchingLevel[0];
      }
    },
    methods: {
        fetchLeagueOptions() {
            axios
                .get('/api/League/LeagueOptions')
                .then(response => {
                  this.possibleLeagueOptions = response.data;
                  this.selectedLeagueOptions.maximumEligibilityLevel = this.possibleLeagueOptions.defaultMaximumEligibilityLevel;
                })
                .catch(returnedError => (this.error = returnedError));
        },
        postRequest() {
            axios
                .post('/api/leagueManager/createLeague', this.selectedLeagueOptions)
                .then(this.responseHandler)
                .catch(this.catchHandler);
        },
        responseHandler(response) {
           this.$router.push({ name: "home" });
        },
        catchHandler(returnedError) {
          this.errorInfo = returnedError;
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
