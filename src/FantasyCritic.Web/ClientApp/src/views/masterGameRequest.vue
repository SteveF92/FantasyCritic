<template>
  <div>
    <div class="col-md-10 offset-md-1 col-sm-12 offset-sm-0">
      <h1>Master Game Request</h1>
      <hr />
      <div v-if="showSent" class="alert alert-success">Master Game request made.</div>
      <div v-if="showDeleted" class="alert alert-success">Master Game request was deleted.</div>
      <div v-if="errorInfo" class="alert alert-danger">An error has occurred with your request.</div>
      <div class="col-lg-10 col-md-12 offset-lg-1 offset-md-0 text-well">
        <p>
          <strong>
            If there's a game you want to see added to the site, you can fill out this form and I'll look into adding the game. You can check back on this page to see the status of previous requests,
            as well.
          </strong>
        </p>

        <br />

        <label>Please describe why you are requesting this game:</label>
        <b-form-checkbox v-model="wantToPickup">
          <span class="checkbox-label">I want to draft or bid on this game immediately.</span>
        </b-form-checkbox>
        <b-form-checkbox v-model="nearCertainInterested">
          <span class="checkbox-label">I'm essentially certain someone else wants to draft or bid on this game very soon.</span>
        </b-form-checkbox>
        <br />
        <div class="alert alert-info">
          The reason I ask this question is because having an obscure game on the list that is not drafted by anyone doesn't do much for the site, other than requiring effort to make sure the game's
          info is up to date (for example, release date changes). Please don't request a game "just for the sake of having it on the site".
        </div>

        <ValidationObserver v-slot="{ invalid }" v-if="validReason">
          <hr />
          <form v-on:submit.prevent="sendMasterGameRequestRequest">
            <div class="form-group">
              <label for="gameName" class="control-label">Game Name</label>
              <ValidationProvider rules="required" v-slot="{ errors }">
                <input v-model="gameName" id="gameName" name="Game Name" type="text" class="form-control input" />
                <span class="text-danger">{{ errors[0] }}</span>
              </ValidationProvider>
            </div>

            <div class="form-group">
              <b-form-checkbox v-model="hasReleaseDate">
                <span class="checkbox-label">Does this game have a confirmed release date?</span>
              </b-form-checkbox>
              <div v-if="hasReleaseDate">
                <label for="releaseDate" class="control-label">Release Date</label>
                <flat-pickr v-model="releaseDate" class="form-control"></flat-pickr>
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
            <div class="form-group">
              <label for="ggLink" class="control-label">Link to GG| Page (Optional)</label>
              <input v-model="ggLink" id="ggLink" name="ggLink" class="form-control input" />
            </div>

            <div class="form-group">
              <label for="requestNote" class="control-label">Any other notes?</label>
              <div class="alert alert-info">
                In particular, please indicate if this game needs any special tags. Such as:
                <ul>
                  <li>Is this game currently in early access?</li>
                  <li>Is this game the start of a new franchise or part of an existing one?</li>
                </ul>
              </div>
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
                <span v-show="!request.masterGame">{{ request.gameName }}</span>
              </td>
              <td>
                <span v-show="request.responseNote">{{ request.responseNote }}</span>
                <span v-show="!request.responseNote">&lt;Pending&gt;</span>
              </td>
              <td>
                <span v-show="request.responseTimestamp">{{ request.responseTimestamp | dateTime }}</span>
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
</template>
<script>
import axios from 'axios';
import MasterGamePopover from '@/components/masterGamePopover';

export default {
  data() {
    return {
      myRequests: [],
      showSent: false,
      showDeleted: false,
      errorInfo: '',
      gameName: '',
      requestNote: '',
      steamLink: '',
      openCriticLink: '',
      ggLink: '',
      releaseDate: new Date(),
      estimatedReleaseDate: '',
      hasReleaseDate: false,
      wantToPickup: false,
      nearCertainInterested: false
    };
  },
  components: {
    MasterGamePopover
  },
  computed: {
    validReason() {
      return this.wantToPickup || this.nearCertainInterested;
    }
  },
  methods: {
    fetchMyRequests() {
      axios
        .get('/api/game/MyMasterGameRequests')
        .then((response) => {
          this.myRequests = response.data;
        })
        .catch((response) => {});
    },
    sendMasterGameRequestRequest() {
      let request = {
        gameName: this.gameName,
        requestNote: this.requestNote,
        steamLink: this.steamLink,
        openCriticLink: this.openCriticLink,
        ggLink: this.ggLink,
        estimatedReleaseDate: this.estimatedReleaseDate
      };

      if (this.hasReleaseDate) {
        request.releaseDate = this.releaseDate;
      }

      axios
        .post('/api/game/CreateMasterGameRequest', request)
        .then((response) => {
          this.showSent = true;
          window.scroll({
            top: 0,
            left: 0,
            behavior: 'smooth'
          });
          this.clearData();
          this.fetchMyRequests();
        })
        .catch((error) => {
          this.errorInfo = error.response;
        });
    },
    clearData() {
      this.gameName = '';
      this.requestNote = '';
      this.steamLink = '';
      this.openCriticLink = '';
      this.ggLink = '';
      this.releaseDate = '';
      this.estimatedReleaseDate = '';
      this.$validator.reset();
    },
    cancelRequest(request) {
      let model = {
        requestID: request.requestID
      };
      axios
        .post('/api/game/DeleteMasterGameRequest', model)
        .then((response) => {
          this.showDeleted = true;
          this.fetchMyRequests();
        })
        .catch((response) => {});
    },
    dismissRequest(request) {
      let model = {
        requestID: request.requestID
      };
      axios
        .post('/api/game/DismissMasterGameRequest', model)
        .then((response) => {
          this.fetchMyRequests();
        })
        .catch((response) => {});
    }
  },
  mounted() {
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
