<template>
  <div>
    <div class="col-md-10 offset-md-1 col-sm-12 offset-sm-0">
      <h1>Master Game Correction Request</h1>
      <hr />
      <div v-if="showSent" class="alert alert-success">Master Game request made.</div>
      <div v-if="showDeleted" class="alert alert-success">Master Game request was deleted.</div>
      <div v-if="errorInfo" class="alert alert-danger">An error has occurred with your request.</div>
      <div class="col-lg-10 col-md-12 offset-lg-1 offset-md-0">
        <div class="text-well">
          <p>
            If you see an issue with a game on the site, for example an incorrect release date, you can send me a note and I'll get it fixed.
          </p>
          <p>
            However, if this is a "debateable" correction, like whether or not a game is "Remake" or a "Partial Remake", please come to the "eligibility-debates" channel in our
            <a href="https://discord.gg/dNa7DD3">Discord <font-awesome-icon icon="external-link-alt" size="sm" /></a> so the community can discuss it.
          </p>
          <p v-show="masterGame">
            You can also use this form to let me know about a missing link to OpenCritic, so scores can populate, or GG|, so the game will have an image to represent it.
          </p>
        </div>
        <hr />
        <div v-if="masterGame && masterGame.numberOutstandingCorrections" class="alert alert-warning">
          There are {{masterGame.numberOutstandingCorrections}} correction(s) currently submitted that I have not reviewed.
          You may not need to submit anything.
        </div>
        <p v-show="!masterGame">
          <strong>
            You can suggest a correction by clicking a link on a master game's page.
          </strong>
        </p>
        <div class="row" v-if="masterGame">
          <div class="col-lg-10 col-md-12 offset-lg-1 offset-md-0 text-well">
            <h2 v-if="masterGame">{{masterGame.gameName}}</h2>
            <masterGameDetails :masterGame="masterGame"></masterGameDetails>

            <ValidationObserver v-slot="{ invalid }">
              <form v-on:submit.prevent="sendMasterGameChangeRequestRequest">
                <div class="form-group">
                  <label for="requestNote" class="control-label">What seems to be the issue?</label>
                  <ValidationProvider rules="required" v-slot="{ errors }">
                    <input v-model="requestNote" id="requestNote" name="Request Note" type="text" class="form-control input" />
                    <span class="text-danger">{{ errors[0] }}</span>
                  </ValidationProvider>
                </div>

                <div class="form-group">
                  <label for="openCriticLink" class="control-label">Link to Open Critic Page (Optional)</label>
                  <input v-model="openCriticLink" id="openCriticLink" name="openCriticLink" class="form-control input" />
                </div>

                <div class="form-group">
                  <label for="ggLink" class="control-label">Link to GG| Page (Optional)</label>
                  <input v-model="ggLink" id="ggLink" name="ggLink" class="form-control input" />
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
        <div v-if="myRequests.length !== 0">
          <div class="row">
            <h3>My Current Requests</h3>
          </div>
          <div class="row">
            <table class="table table-sm table-responsive-sm table-bordered table-striped">
              <thead>
                <tr class="bg-primary">
                  <th scope="col" class="game-column">Game Name</th>
                  <th scope="col" class="game-column">Note</th>
                  <th scope="col">Response</th>
                  <th scope="col">Response Time</th>
                  <th scope="col"></th>
                </tr>
              </thead>
              <tbody>
                <tr v-for="request in myRequests">
                  <td>
                    <span><masterGamePopover :masterGame="request.masterGame"></masterGamePopover></span>
                  </td>
                  <td>
                    <span> {{request.requestNote}} </span>
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
      </div>
    </div>
  </div>
</template>
<script>
import axios from 'axios';
import vueSlider from 'vue-slider-component';
import Popper from 'vue-popperjs';
import MasterGamePopover from '@/components/modules/masterGamePopover';
import MasterGameDetails  from '@/components/modules/masterGameDetails';

export default {
  data() {
    return {
      myRequests: [],
      showSent: false,
      showDeleted: false,
      errorInfo: '',
      masterGame: null,
      requestNote: '',
      openCriticLink: '',
      ggLink: '',
      piecewiseStyle: {
        'backgroundColor': '#ccc',
        'visibility': 'visible',
        'width': '12px',
        'height': '20px'
      }
    };
  },
  components: {
    MasterGamePopover,
    MasterGameDetails,
    vueSlider,
    'popper': Popper,
  },
  methods: {
    fetchMyRequests() {
      axios
        .get('/api/game/MyMasterGameChangeRequests')
        .then(response => {
          this.myRequests = response.data;
        })
        .catch(response => {

        });
    },
    sendMasterGameChangeRequestRequest() {
      if (!this.requestNote && this.openCriticLink) {
        this.requestNote = 'Link to OpenCritic';
      }
      let request = {
        masterGameID: this.masterGame.masterGameID,
        requestNote: this.requestNote,
        openCriticLink: this.openCriticLink,
        ggLink: this.ggLink
      };
      axios
        .post('/api/game/CreateMasterGameChangeRequest', request)
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
      this.requestNote = '';
      this.openCriticLink = '';
      this.ggLink = '';
    },
    cancelRequest(request) {
      let model = {
        requestID: request.requestID
      };
      axios
        .post('/api/game/DeleteMasterGameChangeRequest', model)
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
        .post('/api/game/DismissMasterGameChangeRequest', model)
        .then(response => {
          this.fetchMyRequests();
        })
        .catch(response => {

        });
    },
    fetchMasterGame(masterGameID) {
      axios
        .get('/api/game/MasterGame/' + masterGameID)
        .then(response => {
          this.masterGame = response.data;
        })
        .catch(returnedError => (this.error = returnedError));
    }
  },
  mounted() {
    let masterGameID = this.$route.query.mastergameid;
    if (masterGameID) {
      this.fetchMasterGame(masterGameID);
    }

    this.fetchMyRequests();
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
