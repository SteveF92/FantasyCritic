<template>
  <div>
    <h1>Master Game Request</h1>
    <div v-if="showSent" class="alert alert-success">Master Game request made.</div>
    <div v-if="showDeleted" class="alert alert-success">Master Game request was deleted.</div>
    <div v-if="errorInfo" class="alert alert-danger">An error has occurred with your request.</div>
    <div class="col-xl-8 col-lg-10 col-md-12" v-if="myRequests.length !== 0">
      <div class="row">
        <h3>My Current Requests</h3>
      </div>
      <div class="row">
        <table class="table table-sm table-responsive-sm table-bordered table-striped">
          <thead>
            <tr class="bg-primary">
              <th scope="col" class="game-column">Game Name</th>
              <th scope="col"></th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="request in myRequests">
              <td>{{request.gameName}}</td>
              <td class="select-cell">
                <b-button variant="danger" size="sm" v-on:click="cancelRequest(request)">Cancel Request</b-button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>
    <div class="row">
      <div class="col-xl-8 col-lg-10 col-md-12 text-well">
        <p>
          <strong>
            If there's a game you want to see added to the site, you can fill out this form and I'll look into adding the game.
            You can check back on this page to see the status of previous requests, as well.
          </strong>
        </p>
        <form v-on:submit.prevent="sendMasterGameRequestRequest">
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
            <label for="steamLink" class="control-label">Link to Steam Page (Optional)</label>
            <input v-model="steamLink" id="steamLink" name="steamLink" class="form-control input" />
          </div>
          <div class="form-group">
            <label for="openCriticLink" class="control-label">Link to Open Critic Page (Optional)</label>
            <input v-model="openCriticLink" id="openCriticLink" name="openCriticLink" class="form-control input" />
          </div>

          <div class="form-group eligibility-section" v-if="possibleEligibilityLevels">
            <label class="control-label eligibility-slider-label">Eligibility Level</label>
            <p class="eligibility-explanation">
              Eligibility levels are designed to prevent people from taking "uninteresting" games. While I will make the final decision on how a game should be classified, I'm interested in your opinion.
            </p>
            <vue-slider v-model="eligibilityLevel" :min="minimumPossibleEligibilityLevel" :max="maximumPossibleEligibilityLevel"
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
          </div>

          <div class="form-group">
            <b-form-checkbox v-model="yearlyInstallment">
              <span class="checkbox-label">Is this game a yearly installment?</span>
              <p>Check this for games like yearly sports titles.</p>
            </b-form-checkbox>
          </div>
          <div class="form-group">
            <b-form-checkbox v-model="earlyAccess">
              <span class="checkbox-label">Is this game currenly in early access?</span>
              <p>Games that are already playable in early access are only selectable in some leagues.</p>
            </b-form-checkbox>
          </div>

          <div class="form-group">
            <label for="requestNote" class="control-label">Any other notes?</label>
            <input v-model="requestNote" id="requestNote" name="requestNote" class="form-control input" />
          </div>

          <div class="form-group">
            <div class="col-md-offset-2 col-md-4">
              <input type="submit" class="btn btn-primary" value="Submit" :disabled="!formIsValid" />
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
        myRequests: [],
        showSent: false,
        showDeleted: false,
        errorInfo: "",
        gameName: "",
        requestNote: "",
        steamLink: "",
        openCriticLink: "",
        estimatedReleaseDate: "",
        yearlyInstallment: false,
        earlyAccess: false,
        eligibilityLevel: 0,
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
      formIsValid() {
        return !Object.keys(this.veeFields).some(key => this.veeFields[key].invalid);
      },
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
      }
    },
    methods: {
      fetchMyRequests() {
        axios
          .get('/api/game/MyMasterGameRequests')
          .then(response => {
            this.myRequests = response.data;
          })
          .catch(response => {

          });
      },
      fetchEligibilityLevels() {
        axios
          .get('/api/Game/EligibilityLevels')
          .then(response => {
            this.possibleEligibilityLevels = response.data;
          })
          .catch(returnedError => (this.error = returnedError));
      },
      sendMasterGameRequestRequest() {
        let request = {
          gameName: this.gameName,
          requestNote: this.requestNote,
          steamLink: this.steamLink,
          openCriticLink: this.openCriticLink,
          estimatedReleaseDate: this.estimatedReleaseDate,
          yearlyInstallment: this.yearlyInstallment,
          earlyAccess: this.earlyAccess,
          eligibilityLevel: this.eligibilityLevel

        };
        axios
          .post('/api/game/CreateMasterGameRequest', request)
          .then(response => {
            this.showSent = true;
            window.scroll({
              top: 0,
              left: 0,
              behavior: 'smooth'
            });
            this.clearData();
            this.fetchMyRequests();
          })
          .catch(error => {
            this.errorInfo = error.response;
          });
      },
      clearData() {
        this.gameName = "";
        this.requestNote = "";
        this.steamLink = "";
        this.openCriticLink = "";
        this.estimatedReleaseDate = "";
        this.yearlyInstallment = false;
        this.earlyAccess = false;
        this.eligibilityLevel = 0;
        this.$validator.reset();
      },
      cancelRequest(request) {
        let model = {
          requestID: request.requestID
        };
        axios
          .post('/api/game/DeleteMasterGameRequest', model)
          .then(response => {
            this.showDeleted = true;
            this.fetchMyRequests();
          })
          .catch(response => {

          });
      }
    },
    mounted() {
      this.fetchEligibilityLevels();
      this.fetchMyRequests();
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
