<template>
  <div>
    <div class="col-md-10 offset-md-1 col-sm-12">
      <div>
        <h1>Edit Master Game</h1>
        <b-button variant="info" :to="{ name: 'activeMasterGameChangeRequests' }">View master change game requests</b-button>
        <b-button variant="info" :to="{ name: 'adminConsole' }">Admin Console</b-button>
      </div>
      <hr />
      <div v-if="masterGame">
        <h2>{{masterGame.gameName}}</h2>
        <div class="row" v-if="changeRequest">
          <div class="text-well">
            <h2>Request Note</h2>
            <p>{{changeRequest.requestNote}}</p>
          </div>
        </div>
        <hr />

        <div class="row">
          <div class="col-lg-10 col-md-12 offset-lg-1 text-well">
            <ValidationObserver v-slot="{ invalid }">
              <form v-on:submit.prevent="createMasterGame">
                <div class="form-group">
                  <label for="gameName" class="control-label">Game Name</label>
                  <ValidationProvider rules="required" v-slot="{ errors }">
                    <input v-model="masterGame.gameName" id="gameName" name="Game Name" type="text" class="form-control input" />
                    <span class="text-danger">{{ errors[0] }}</span>
                  </ValidationProvider>
                </div>

                <div class="form-group">
                  <label for="releaseDate" class="control-label">Release Date</label>
                  <flat-pickr v-model="releaseDate" class="form-control"></flat-pickr>
                </div>

                <b-button variant="info" size="sm" v-on:click="propagateDate">Propagate Date</b-button>
                <b-button variant="info" size="sm" v-on:click="parseEstimatedReleaseDate">Parse Estimated</b-button>
                <b-button variant="warning" size="sm" v-on:click="clearDates">Clear Dates</b-button>

                <div class="form-group">
                  <label for="estimatedReleaseDate" class="control-label">Estimated Release Date</label>
                  <input v-model="estimatedReleaseDate" id="estimatedReleaseDate" name="estimatedReleaseDate" class="form-control input" />
                </div>
                <div class="form-group">
                  <label for="minimumReleaseDate" class="control-label">Minimum Release Date</label>
                  <flat-pickr v-model="minimumReleaseDate" class="form-control"></flat-pickr>
                </div>
                <div class="form-group">
                  <label for="maximumReleaseDate" class="control-label">Maximum Release Date</label>
                  <flat-pickr v-model="maximumReleaseDate" class="form-control"></flat-pickr>
                </div>
                <div class="form-group">
                  <label for="earlyAccessReleaseDate" class="control-label">Early Access Release Date</label>
                  <flat-pickr v-model="earlyAccessReleaseDate" class="form-control"></flat-pickr>
                </div>
                <div class="form-group">
                  <label for="internationalReleaseDate" class="control-label">International Release Date</label>
                  <flat-pickr v-model="internationalReleaseDate" class="form-control"></flat-pickr>
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
                              :marks="marks" :tooltip="'always'">
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
                  <b-form-checkbox v-model="unannouncedGame">
                    <span class="checkbox-label">Unannounced?</span>
                  </b-form-checkbox>
                </div>

                <h3>Tags</h3>
                <masterGameTagSelector v-model="tags"></masterGameTagSelector>

                <div class="form-group">
                  <label for="notes" class="control-label">Other Notes</label>
                  <input v-model="notes" id="notes" name="notes" class="form-control input" />
                </div>

                <div class="form-group">
                  <div class="col-md-offset-2 col-md-4">
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
  import moment from 'moment';
  import 'vue-slider-component/theme/antd.css';
  import MasterGameTagSelector from '@/components/modules/masterGameTagSelector';

  export default {
    props: ['mastergameid'],
    data() {
      return {
        masterGame: null,
        changeRequest: null
      };
    },
    components: {
      vueSlider,
      'popper': Popper,
      MasterGameTagSelector
    },
    methods: {
      fetchMasterGame() {
        axios
          .get('/api/game/MasterGame/' + this.mastergameid)
          .then(response => {
            this.masterGame = response.data;
          })
          .catch(returnedError => (this.error = returnedError));
      },
      fetchChangeRequest() {
        let changeRequestID = this.$route.query.changeRequestID;
        if (!changeRequestID) {
          return;
        }
        axios
          .get('/api/admin/GetMasterGameChangeRequest?changeRequestID=' + changeRequestID)
          .then(response => {
            this.changeRequest = response.data;
          })
          .catch(returnedError => (this.error = returnedError));
      }
    },
    mounted() {
      this.fetchMasterGame();
      this.fetchChangeRequest();
    },
    watch: {
      '$route'(to, from) {
        this.fetchMasterGame();
      }
    }
  };
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
    margin-bottom: 30px;
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
