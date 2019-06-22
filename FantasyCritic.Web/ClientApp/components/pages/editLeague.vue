<template>
  <div>
    <div class="col-md-10 offset-md-1 col-sm-12">
      <div v-if="selectedLeagueOptions && possibleLeagueOptions">
        <h1>Edit League: {{selectedLeagueOptions.leagueName}} (Year {{selectedLeagueOptions.year}})</h1>
        <hr />
        <form v-if="possibleLeagueOptions" method="post" class="form-horizontal" role="form" v-on:submit.prevent="postRequest">
          <div class="text-well col-md-12 col-lg-10 offset-lg-1">
            <div class="alert alert-danger" v-if="errorInfo">An error has occurred.</div>

            <div class="form-group">
              <label for="standardGames" class="control-label">Total Number of Games</label>
              <p>
                This is the total number of games that each player will have on their roster.
              </p>

              <input v-model="selectedLeagueOptions.standardGames" id="standardGames" name="standardGames" type="text" class="form-control input" />
            </div>

            <div class="form-group">
              <label for="gamesToDraft" class="control-label">Number of Games to Draft</label>
              <p>
                This is the number of games that will be chosen by each player at the draft.
                If this number is lower than the "Total Number of Games", the remainder will be
                <a href="/faq#bidding-system" target="_blank">
                  Pickup Games.
                </a>
              </p>

              <input v-model="selectedLeagueOptions.gamesToDraft" id="gamesToDraft" name="gamesToDraft" type="text" class="form-control input" />
            </div>

            <div class="form-group">
              <label for="counterPicks" class="control-label">Number of Counter Picks</label>
              <p>
                Counter picks are essentially bets against a game. For more details,
                <a href="/faq#scoring" target="_blank">
                  click here.
                </a>
              </p>

              <input v-model="selectedLeagueOptions.counterPicks" id="counterPicks" name="counterPicks" type="text" class="form-control input" />
            </div>
            <hr />

            <div class="form-group eligibility-section">
              <label class="control-label eligibility-slider-label">Maximum Eligibility Level</label>
              <vue-slider v-model="selectedLeagueOptions.maximumEligibilityLevel" :min="minimumEligibilityLevel" :max="maximumEligibilityLevel"
                          :marks="marks" :tooltip="'always'">
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
                  <b-form-checkbox id="yearly-checkbox"
                                   v-model="selectedLeagueOptions.allowYearlyInstallments">
                    <span class="checkbox-label">Allow Yearly Installments (IE Yearly Sports Franchises)</span>
                  </b-form-checkbox>
                </div>
                <div>
                  <b-form-checkbox id="early-access-checkbox"
                                   v-model="selectedLeagueOptions.allowEarlyAccess">
                    <span class="checkbox-label">Allow Early Access games</span>
                  </b-form-checkbox>
                </div>
                <div>
                  <b-form-checkbox id="freetoplay-checkbox"
                                   v-model="selectedLeagueOptions.allowFreeToPlay">
                    <span class="checkbox-label">Allow Free to Play games</span>
                  </b-form-checkbox>
                </div>
                <div>
                  <b-form-checkbox id="released-internationally-checkbox"
                                   v-model="selectedLeagueOptions.allowReleasedInternationally">
                    <span class="checkbox-label">Allow games already released internationally</span>
                  </b-form-checkbox>
                </div>
                <div>
                  <b-form-checkbox id="expansion-pack-checkbox"
                                   v-model="selectedLeagueOptions.allowExpansions">
                    <span class="checkbox-label">Allow expansion packs/DLC</span>
                  </b-form-checkbox>
                </div>
              </div>
            </div>

            <div class="form-group">
              <input type="submit" class="btn btn-primary col-10 offset-1" value="Edit League Settings" />
            </div>
          </div>
        </form>
      </div>
    </div>
  </div>
</template>
<script>
  import Vue from "vue";
  import axios from "axios";
  import vueSlider from 'vue-slider-component';
  import 'vue-slider-component/theme/antd.css'

  export default {
    data() {
      return {
        errorInfo: "",
        possibleLeagueOptions: null,
        selectedLeagueOptions: null
      }
    },
    props: ['leagueid', 'year'],
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
      },
      marks() {
        if (!this.possibleLeagueOptions.eligibilityLevels) {
          return [];
        }

        let levels =  this.possibleLeagueOptions.eligibilityLevels.map(function (v) {
          return v.level;
        });

        return levels;
      }

    },
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
            this.selectedLeagueOptions = response.data;
          })
          .catch(returnedError => (this.error = returnedError));
      },
      postRequest() {
        axios
          .post('/api/leagueManager/EditLeagueYearSettings', this.selectedLeagueOptions)
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

  label {
    font-size: 18px;
  }

</style>
<style>
  .vue-slider-piecewise-label {
    color: white !important;
  }
</style>
