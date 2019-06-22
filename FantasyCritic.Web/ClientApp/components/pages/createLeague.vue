<template>
  <div>
    <div class="col-md-10 offset-md-1 col-sm-12">
      <h1>Create a league</h1>
      <hr />
      <div class="alert alert-danger" v-show="errorInfo">
        <h2>Error!</h2>
        <p>{{errorInfo}}</p>
      </div>

      <form v-if="possibleLeagueOptions" method="post" class="form-horizontal" role="form" v-on:submit.prevent="postRequest">
        <div class="col-md-12 col-lg-10 offset-lg-1">
          <div class="text-well ">
            <h2>Basic Settings</h2>
            <div class="form-group">
              <label for="leagueName" class="control-label">League Name</label>
              <input v-model="leagueName" v-validate="'required'" id="leagueName" name="leagueName" type="text" class="form-control input" />
              <span class="text-danger">{{ errors.first('leagueName') }}</span>
            </div>
            <hr />
            <div class="form-group">
              <label for="intendedNumberOfPlayers" class="control-label">How many players do you think will be in this league?</label>
              <input v-model="intendedNumberOfPlayers" v-validate="'required|min_value:2|max_value:14'" id="intendedNumberOfPlayers" name="intendedNumberOfPlayers" type="text" class="form-control input" />
              <span class="text-danger">{{ errors.first('intendedNumberOfPlayers') }}</span>
              <p>You aren't locked into this number of people. This is just to recommend how many games to have per person.</p>
            </div>

            <div v-if="readyToChooseNumbers()">
              <hr />
              <label>Based on your number of players, we recommend the following settings. However, you are free to change this.</label>

              <div class="form-group">
                <label for="standardGames" class="control-label">Total Number of Games</label>
                <p>
                  This is the total number of games that each player will have on their roster.
                </p>

                <input v-model="standardGames" v-validate="'required|min_value:1|max_value:30'" id="standardGames" name="standardGames" type="text" class="form-control input" />
                <span class="text-danger">{{ errors.first('standardGames') }}</span>
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
                <input v-model="gamesToDraft" v-validate="'required|min_value:1|max_value:30'" id="gamesToDraft" name="gamesToDraft" type="text" class="form-control input" />
                <span class="text-danger">{{ errors.first('gamesToDraft') }}</span>
              </div>

              <div class="form-group">
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
              <hr />
            </div>
          </div>

          <hr />

          <div v-if="readyToChooseLevels()">
            <div class="text-well">
              <h2>Eligibility Settings</h2>
              <div class="alert alert-info">
                These options let you choose what games are available in your league. These settings can be overriden on a game by game basis, and I reccomend you lean towards being more restrictive,
                and allow specific exemptions if your entire league decides on one. The default options are the recommended settings.
              </div>

              <div class="form-group eligibility-section">
                <label class="control-label eligibility-slider-label">Maximum Eligibility Level</label>
                <p class="eligibility-explanation">
                  Eligibility levels are designed to prevent people from taking "uninteresting" games. Every game on the site is assigned a 'level' to seperate 'new games' from remakes, remasters, ports, and everything
                  in between. Setting this to a low number means being more restrictive, setting it to a higher number means being more lenient. I reccommend setting '2'. For more details,
                  <a href="/faq#eligibility" target="_blank">
                    click here.
                  </a>
                </p>
                <div class="alert alert-warning" v-show="maximumEligibilityLevel === 5">
                  I really don't recommend using setting '5'. Games that fall under this category often don't even get re-reviewed when they are released on their new platforms. Again, I recomend that you
                  be restrictive here and allow exemptions if need be.
                </div>
                <vue-slider v-model="maximumEligibilityLevel" :min="minimumPossibleEligibilityLevel" :max="maximumPossibleEligibilityLevel"
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
                <br />

                <div>
                  <div>
                    <b-form-checkbox v-model="allowYearlyInstallments">
                      <span class="checkbox-label">Allow Yearly Installments (IE Yearly Sports Franchises)</span>
                      <p>These are often pretty safe bets, so they may not be the most interesting choices.</p>
                    </b-form-checkbox>
                  </div>
                  <div>
                    <b-form-checkbox v-model="allowEarlyAccess">
                      <span class="checkbox-label">Allow Early Access Games</span>
                      <p>
                        If this is left unchecked, a game that is already in early access will not be selectable, since it is already playable.
                        Games that are planned for early access that are not yet playable are always selectable.
                      </p>
                    </b-form-checkbox>
                  </div>
                  <div>
                    <b-form-checkbox v-model="allowFreeToPlay">
                      <span class="checkbox-label">Allow Free to Play Games</span>
                      <p>These are often hard to review and may not get a score.</p>
                    </b-form-checkbox>
                  </div>
                  <div>
                    <b-form-checkbox v-model="allowReleasedInternationally">
                      <span class="checkbox-label">Allow games already released in other regions</span>
                      <p>
                        If this is left unchecked, a game that has already been released in another region will not be selectable.
                        For example, a game that came out in Japan in 2018 and is getting an English release in 2019.
                      </p>
                    </b-form-checkbox>
                  </div>
                  <div>
                    <b-form-checkbox v-model="allowExpansions">
                      <span class="checkbox-label">Allow expansion packs/DLC</span>
                      <p>
                        If this is left unchecked, expansion packs and DLC will not be selectable. There's a lot of grey zone with these games and I recommend using the override system to allow
                        specific games, rather than the whole category.
                      </p>
                    </b-form-checkbox>
                  </div>
                </div>
              </div>
            </div>

            <hr />

            <div>
              <div>
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
              </div>
              <div class="form-group">
                <input type="submit" class="btn btn-primary col-10 offset-1" value="Create League" />
              </div>
              <div class="alert alert-info disclaimer">
                Reminder: All of these settings can always be changed later.
              </div>
            </div>
          </div>
        </div>
      </form>
    </div>
  </div>
</template>
<script>
  import Vue from "vue";
  import axios from "axios";
  import vueSlider from 'vue-slider-component';
  import Popper from 'vue-popperjs';
  import 'vue-slider-component/theme/antd.css'

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
          allowYearlyInstallments: false,
          allowEarlyAccess: false,
          allowFreeToPlay: false,
          allowReleasedInternationally: false,
          allowExpansions: false,
          publicLeague: true,
          testLeague: false,
          possibleLeagueOptions: null
        }
    },
    components: {
      vueSlider,
      'popper': Popper,
    },
    computed: {
      formIsValid() {
        return !Object.keys(this.veeFields).some(key => this.veeFields[key].invalid);
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
        readyToChooseNumbers() {
          let leagueNameValid = this.veeFields['leagueName'] && this.veeFields['leagueName'].valid;
          let intendedNumberOfPlayersValid = this.veeFields['intendedNumberOfPlayers'] && this.veeFields['intendedNumberOfPlayers'].valid;
          return leagueNameValid && intendedNumberOfPlayersValid;
        },
        readyToChooseLevels() {
          let standardGamesValid = this.veeFields['standardGames'] && this.veeFields['standardGames'].valid;
          let gamesToDraftValid = this.veeFields['gamesToDraft'] && this.veeFields['gamesToDraft'].valid;
          let counterPicksValid = this.veeFields['counterPicks'] && this.veeFields['counterPicks'].valid;
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
    watch: {
      intendedNumberOfPlayers: function (val) {
        let recommendedNumberOfGames = 72;
        this.standardGames = Math.floor(recommendedNumberOfGames / val);
        if (this.standardGames > 25) {
          this.standardGames = 25;
        }
        if (this.standardGames < 10) {
          this.standardGames = 10;
        }
        this.gamesToDraft = Math.floor(this.standardGames / 2);
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

label {
  font-size: 18px;
}

.submit-button {
  text-align: right;
}
</style>
<style>
  .vue-slider-piecewise-label {
    color: white !important;
  }
</style>
