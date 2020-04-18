<template>
  <div>
    <div class="col-md-10 offset-md-1 col-sm-12 offset-sm-0">
      <h1>Master Game Request</h1>
      <hr />
      <div v-if="showSent" class="alert alert-success">Master Game request made.</div>
      <div v-if="showDeleted" class="alert alert-success">Master Game request was deleted.</div>
      <div v-if="errorInfo" class="alert alert-danger">An error has occurred with your request.</div>
      <div class="col-lg-10 col-md-12 offset-lg-1 offset-md-0">
        <div v-if="myRequests.length !== 0">
          <div class="row">
            <h3>My Current Requests</h3>
          </div>
          <div class="row">
            <table class="table table-sm table-responsive-sm table-bordered table-striped">
              <thead>
                <tr class="bg-primary">
                  <th scope="col" class="game-column">Game Name</th>
                  <th scope="col">Response</th>
                  <th scope="col">Response Time</th>
                  <th scope="col"></th>
                </tr>
              </thead>
              <tbody>
                <tr v-for="request in myRequests">
                  <td>
                    <span v-if="request.masterGame"><masterGamePopover :masterGame="request.masterGame"></masterGamePopover></span>
                    <span v-show="!request.masterGame"> {{request.gameName}} </span>
                  </td>
                  <td>
                    <span v-show="request.responseNote"> {{request.responseNote}} </span>
                    <span v-show="!request.responseNote">&lt;Pending&gt;</span>
                  </td>
                  <td>
                    <span v-show="request.responseTimestamp"> {{request.responseTimestamp | dateTime}} </span>
                    <span v-show="!request.responseTimestamp">&lt;Pending&gt;</span>
                  </td>
                  <td class="select-cell">
                    <span v-show="request.answered"><b-button variant="info" size="sm" v-on:click="dismissRequest(request)">Dismiss Request</b-button></span>
                    <span v-show="!request.answered"><b-button variant="danger" size="sm" v-on:click="cancelRequest(request)">Cancel Request</b-button></span>
                  </td>
                </tr>
              </tbody>
            </table>
          </div>
        </div>
        <div class="row">
          <div class="col-lg-10 col-md-12 offset-lg-1 offset-md-0 text-well">
            <p>
              <strong>
                If there's a game you want to see added to the site, you can fill out this form and I'll look into adding the game.
                You can check back on this page to see the status of previous requests, as well.
              </strong>
            </p>
            <ValidationObserver v-slot="{ invalid }">
              <form v-on:submit.prevent="sendMasterGameRequestRequest">
                <div class="form-group">
                  <label for="gameName" class="control-label">Game Name</label>
                  <ValidationProvider rules="required" v-slot="{ errors }" name="Game Name">
                    <input v-model="gameName" id="gameName" name="gameName" type="text" class="form-control input" />
                    <span class="text-danger">{{ errors[0] }}</span>
                  </ValidationProvider>
                </div>

                <div class="form-group">
                  <b-form-checkbox v-model="hasReleaseDate">
                    <span class="checkbox-label">Does this game have a confirmed release date?</span>
                  </b-form-checkbox>
                  <div v-if="hasReleaseDate">
                    <label for="releaseDate" class="control-label">Release Date</label>
                    <VueDatePicker v-model="releaseDate" name="Release Date" color="#D6993A" fullscreen-mobile no-input />
                  </div>
                  <div v-if="!hasReleaseDate">
                    <label for="estimatedReleaseDate" class="control-label">Estimated Release Date</label>
                    <input v-model="estimatedReleaseDate" id="estimatedReleaseDate" name="estimatedReleaseDate" class="form-control input" />
                  </div>
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
                </div>

                <div class="form-group">
                  <b-form-checkbox v-model="yearlyInstallment">
                    <span class="checkbox-label">Is this game a yearly installment?</span>
                    <p>Check this for games like yearly sports titles.</p>
                  </b-form-checkbox>
                </div>
                <div class="form-group">
                  <b-form-checkbox v-model="earlyAccess">
                    <span class="checkbox-label">Is this game currently in early access?</span>
                    <p>Games that are already playable in early access are only selectable in some leagues.</p>
                  </b-form-checkbox>
                </div>
                <div class="form-group">
                  <b-form-checkbox v-model="freeToPlay">
                    <span class="checkbox-label">Is this game free to play?</span>
                    <p>Check this for free to play games.</p>
                  </b-form-checkbox>
                </div>
                <div class="form-group">
                  <b-form-checkbox v-model="releasedInternationally">
                    <span class="checkbox-label">Has this game already been released in a non-English speaking region?</span>
                    <p>Games that are already playable in other regions are only selectable in some leagues.</p>
                  </b-form-checkbox>
                </div>
                <div class="form-group">
                  <b-form-checkbox v-model="expansionPack">
                    <span class="checkbox-label">Is this an expansion pack or DLC?</span>
                    <p>Expansion packs are only selectable in some leagues.</p>
                  </b-form-checkbox>
                </div>
                <div class="form-group">
                  <b-form-checkbox v-model="unannouncedGame">
                    <span class="checkbox-label">Is this unannounced?</span>
                    <p>If the game is only a rumor right now, check this box.</p>
                  </b-form-checkbox>
                </div>

                <div class="form-group">
                  <label for="requestNote" class="control-label">Any other notes?</label>
                  <input v-model="requestNote" id="requestNote" name="requestNote" class="form-control input" />
                </div>

                <div class="form-group">
                  <div class="right-button">
                    <input type="submit" class="btn btn-primary" value="Submit" :disabled="invalid" />
                  </div>
                </div>
              </form>
            </ValidationObserver>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>
<script>
  import axios from 'axios';
  import vueSlider from 'vue-slider-component';
  import Popper from 'vue-popperjs';
  import MasterGamePopover from "components/modules/masterGamePopover";
  import 'vue-slider-component/theme/antd.css'

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
        releaseDate: new Date(),
        estimatedReleaseDate: "",
        yearlyInstallment: false,
        earlyAccess: false,
        freeToPlay: false,
        releasedInternationally: false,
        expansionPack: false,
        unannouncedGame: false,
        eligibilityLevel: 0,
        possibleEligibilityLevels: null,
        hasReleaseDate: false
      }
    },
    components: {
      MasterGamePopover,
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
      marks() {
        if (!this.possibleEligibilityLevels) {
          return [];
        }

        let levels =  this.possibleEligibilityLevels.map(function (v) {
          return v.level;
        });

        return levels;
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
          eligibilityLevel: this.eligibilityLevel,
          yearlyInstallment: this.yearlyInstallment,
          earlyAccess: this.earlyAccess,
          freeToPlay: this.freeToPlay,
          releasedInternationally: this.releasedInternationally,
          expansionPack: this.expansionPack,
          unannouncedGame: this.unannouncedGame
        };

        if (this.hasReleaseDate) {
          request.releaseDate = this.releaseDate;
        }

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
        this.releaseDate = "";
        this.estimatedReleaseDate = "";
        this.eligibilitySettings.yearlyInstallment = false;
        this.eligibilitySettings.earlyAccess = false;
        this.eligibilitySettings.freeToPlay = false;
        this.eligibilitySettings.releasedInternationally = false;
        this.eligibilitySettings.expansionPack = false;
        this.eligibilitySettings.unannouncedGame = false;
        this.eligibilitySettings.eligibilityLevel = 0;
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
      },
      dismissRequest(request) {
        let model = {
          requestID: request.requestID
        };
        axios
          .post('/api/game/DismissMasterGameRequest', model)
          .then(response => {
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
