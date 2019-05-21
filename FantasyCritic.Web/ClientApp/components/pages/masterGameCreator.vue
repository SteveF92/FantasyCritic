<template>
  <div>
    <h1>Create Master Game</h1>
    <div v-if="createdGame" class="alert alert-success">Master Game created: {{createdGame.masterGameID}}</div>
    <div v-if="errorInfo" class="alert alert-danger">An error has occurred with your request.</div>
    <div v-if="openCriticID || steamID">
      <h2>Links</h2>
      <ul>
        <li>
          <a v-if="openCriticID" :href="openCriticLink" target="_blank">OpenCritic Link <font-awesome-icon icon="external-link-alt" /></a>
          <span v-else>No OpenCritic Link</span>
        </li>
        <li>
          <a v-if="steamID" :href="steamLink" target="_blank">Steam Link <font-awesome-icon icon="external-link-alt" /></a>
          <span v-else>No Steam Link</span>
        </li>
      </ul>
    </div>
    <div class="row" v-if="requestNote">
      <div class="col-xl-8 col-lg-10 col-md-12 text-well">
        <h2>Request Note</h2>
        <p>{{requestNote}}</p>
      </div>
    </div>
    <hr />
    <div class="row">
      <div class="col-xl-8 col-lg-10 col-md-12 text-well">
        <form v-on:submit.prevent="createMasterGame">
          <div class="form-group">
            <label for="gameName" class="control-label">Game Name</label>
            <input v-model="gameName" v-validate="'required'" id="gameName" name="gameName" class="form-control input" />
            <span class="text-danger">{{ errors.first('gameName') }}</span>
          </div>

          <div class="form-group">
            <label for="estimatedReleaseDate" class="control-label">Estimated Release Date</label>
            <input v-model="estimatedReleaseDate" id="estimatedReleaseDate" name="estimatedReleaseDate" class="form-control input" />
          </div>
          <div class="form-group">
            <label for="releaseDate" class="control-label">Release Date</label>
            <input v-model="releaseDate" id="releaseDate" name="releaseDate" class="form-control input" />
          </div>

          <div class="form-group">
            <label for="openCriticID" class="control-label">Open Critic ID</label>
            <input v-model="openCriticID" id="openCriticID" name="openCriticID" class="form-control input" />
          </div>

          <div class="form-group eligibility-section" v-if="possibleEligibilityLevels">
            <label class="control-label eligibility-slider-label">Eligibility Level</label>
            <p class="eligibility-explanation">
              Eligibility levels are designed to prevent people from taking "uninteresting" games. While I will make the final decision on how a game should be classified, I'm interested in your opinion.
            </p>
            <vue-slider v-model="eligibilityLevel" :min="minimumPossibleEligibilityLevel" :max="maximumPossibleEligibilityLevel"
                        piecewise piecewise-label :piecewise-style="piecewiseStyle">
            </vue-slider>
            <div class="eligibility-description" v-if="selectedEligibilityLevel">
              <h3>{{ selectedEligibilityLevel.name }}</h3>
              <p>{{ selectedEligibilityLevel.description }}</p>
              <p>Examples: </p>
              <ul>
                <li v-for="example in selectedEligibilityLevel.examples">{{example}}</li>
              </ul>
            </div>
          </div>

          <div class="form-group">
            <b-form-checkbox v-model="yearlyInstallment">
              <span class="checkbox-label">Yearly installment?</span>
            </b-form-checkbox>
          </div>
          <div class="form-group">
            <b-form-checkbox v-model="earlyAccess">
              <span class="checkbox-label">Early access?</span>
            </b-form-checkbox>
          </div>
          <div class="form-group">
            <b-form-checkbox v-model="freeToPlay">
              <span class="checkbox-label">Free to Play?</span>
            </b-form-checkbox>
          </div>
          <div class="form-group">
            <b-form-checkbox v-model="releasedInternationally">
              <span class="checkbox-label">Released Internationally?</span>
            </b-form-checkbox>
          </div>
          <div class="form-group">
            <b-form-checkbox v-model="expansionPack">
              <span class="checkbox-label">Expansion Pack?</span>
            </b-form-checkbox>
          </div>

          <div class="form-group">
            <div class="col-md-offset-2 col-md-4">
              <input type="submit" class="btn btn-primary" value="Submit" />
            </div>
          </div>
        </form>
      </div>
    </div>
  </div>
</template>
<script>
  import axios from 'axios';
  import vueSlider from 'vue-slider-component';
  import Popper from 'vue-popperjs';

  export default {
    data() {
      return {
        createdGame: null,
        errorInfo: "",
        requestNote: "",
        steamID: null,
        openCriticID: null,
        gameName: "",
        estimatedReleaseDate: "",
        releaseDate: "",
        eligibilitySettings: {
            eligibilityLevel: 0,
            yearlyInstallment: false,
            earlyAccess: false,
            freeToPlay: false,
            releasedInternationally: false,
            expansionPack: false,
          },
        possibleEligibilityLevels: null,
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
      minimumPossibleEligibilityLevel() {
        return 0;
      },
      maximumPossibleEligibilityLevel() {
        if (!this.possibleEligibilityLevels) {
          return 0;
        }
        let maxEligibilityLevel = _.maxBy(this.possibleEligibilityLevels, 'level');
        return maxEligibilityLevel.level;
      },
      selectedEligibilityLevel() {
        let matchingLevel = _.filter(this.possibleEligibilityLevels, { 'level': this.eligibilityLevel });
        return matchingLevel[0];
      },
      openCriticLink() {
        return "https://opencritic.com/game/" + this.openCriticID + "/a";
      },
      steamLink() {
        return "https://store.steampowered.com/app/" + this.steamID;
      }
    },
    methods: {
      fetchEligibilityLevels() {
        axios
          .get('/api/Game/EligibilityLevels')
          .then(response => {
            this.possibleEligibilityLevels = response.data;
          })
          .catch(returnedError => (this.error = returnedError));
      },
      createMasterGame() {
        let request = {
          gameName: this.gameName,
          estimatedReleaseDate: this.estimatedReleaseDate,
          releaseDate: this.releaseDate,
          openCriticID: this.openCriticID,
          eligibilitySettings: {
            eligibilityLevel: this.eligibilityLevel,
            yearlyInstallment: this.yearlyInstallment,
            earlyAccess: this.earlyAccess,
            freeToPlay: this.freeToPlay,
            releasedInternationally: this.releasedInternationally,
            expansionPack: this.expansionPack,
          }
        };
        axios
          .post('/api/admin/CreateMasterGame', request)
          .then(response => {
            this.createdGame = response.data;
            window.scroll({
              top: 0,
              left: 0,
              behavior: 'smooth'
            });
            this.clearData();
          })
          .catch(error => {
            this.errorInfo = error.response;
          });
      },
      populateFieldsFromURL() {
        this.gameName = this.$route.query.gameName;
        this.estimatedReleaseDate = this.$route.query.estimatedReleaseDate;
        this.releaseDate = this.$route.query.releaseDate;
        this.steamID = this.$route.query.steamID;
        this.openCriticID = this.$route.query.openCriticID;
        this.eligibilitySettings.eligibilityLevel = this.$route.query.eligibilityLevel;
        this.eligibilitySettings.yearlyInstallment = this.$route.query.yearlyInstallment;
        this.eligibilitySettings.earlyAccess = this.$route.query.earlyAccess;
        this.eligibilitySettings.freeToPlay = this.$route.query.freeToPlay;
        this.eligibilitySettings.releasedInternationally = this.$route.query.releasedInternationally;
        this.eligibilitySettings.expansionPack = this.$route.query.expansionPack;
        this.requestNote = this.$route.query.requestNote;
      }
    },
    mounted() {
      this.fetchEligibilityLevels();
      this.populateFieldsFromURL();
    }
  }
</script>
<style scoped>
  .select-cell {
    text-align: center;
  }

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

  label {
    font-size: 18px;
  }

</style>
<style>
  .vue-slider-piecewise-label {
    color: white !important;
  }
</style>
