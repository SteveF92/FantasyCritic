<template>
  <div class="col-md-10 offset-md-1 col-sm-12 offset-sm-0">
    <h1>Master Game Request</h1>
    <hr />
    <div v-if="showSent" class="alert alert-success">Master Game request made.</div>
    <div v-if="showDeleted" class="alert alert-success">Master Game request was deleted.</div>
    <div v-if="errorInfo" class="alert alert-danger">An error has occurred with your request.</div>
    <div class="col-lg-10 col-md-12 offset-lg-1 offset-md-0 text-well">
      <p>
        <strong>
          If there's a game you want to see added to the site, you can fill out this form and I'll look into adding the game. You can check back on this page to see the status of previous requests, as
          well.
        </strong>
      </p>

      <br />

      <label>Please describe why you are requesting this game:</label>
      <b-form-checkbox v-model="wantToPickup">
        <span class="checkbox-label">I want to draft or bid on this game.</span>
      </b-form-checkbox>
      <b-form-checkbox v-model="nearCertainInterested">
        <span class="checkbox-label">I'm certain someone else wants to draft or bid on this game.</span>
      </b-form-checkbox>
      <br />
      <div class="alert alert-info">
        The reason I ask this question is because having an obscure game on the list that is not drafted by anyone doesn't do much for the site, other than requiring effort to make sure the game's
        info is up to date (for example, release date changes). Please don't request a game "just for the sake of having it on the site".
      </div>

      <ValidationObserver v-if="validReason" v-slot="{ invalid }">
        <hr />
        <form @submit.prevent="sendMasterGameRequestRequest">
          <div class="form-group">
            <label for="gameName" class="control-label">Game Name</label>
            <ValidationProvider v-slot="{ errors }" rules="required">
              <input id="gameName" v-model="gameName" name="Game Name" type="text" class="form-control input" />
              <span class="text-danger">{{ errors[0] }}</span>
            </ValidationProvider>
          </div>

          <div class="form-group">
            <b-form-checkbox v-model="hasReleaseDate">
              <span class="checkbox-label">Does this game have a confirmed release date?</span>
            </b-form-checkbox>
            <div v-if="hasReleaseDate">
              <label for="releaseDate" class="control-label">Release Date</label>
              <b-form-datepicker v-model="releaseDate" class="form-control"></b-form-datepicker>
            </div>
            <div v-if="!hasReleaseDate">
              <label for="estimatedReleaseDate" class="control-label">Estimated Release Date</label>
              <input id="estimatedReleaseDate" v-model="estimatedReleaseDate" name="estimatedReleaseDate" class="form-control input" />
            </div>
          </div>
          <div class="form-group">
            <label for="steamLink" class="control-label">Link to Steam Page (Optional)</label>
            <input id="steamLink" v-model="steamLink" name="steamLink" class="form-control input" />
          </div>
          <div class="form-group">
            <label for="openCriticLink" class="control-label">Link to Open Critic Page (Optional)</label>
            <input id="openCriticLink" v-model="openCriticLink" name="openCriticLink" class="form-control input" />
          </div>
          <div class="form-group">
            <label for="ggLink" class="control-label">Link to GG| Page (Optional)</label>
            <input id="ggLink" v-model="ggLink" name="ggLink" class="form-control input" />
          </div>

          <div class="form-group">
            <label for="requestNote" class="control-label">Any other notes?</label>
            <div class="alert alert-info">
              In particular, please indicate if this game needs any special tags. Such as:
              <ul>
                <li>Is this game currently in early access or planned for early access?</li>
                <li>Is this game the start of a new franchise or part of an existing one?</li>
                <li>Is this game a remake, remaster, or something like that?</li>
              </ul>
            </div>
            <input id="requestNote" v-model="requestNote" name="requestNote" class="form-control input" />
          </div>

          <div v-if="showUnannouncedWarning" class="alert alert-warning">
            <p>It looks like you are requesting an unannounced or rumored game. While there are many of these games on the site, we are trying to scale back on adding them.</p>
            <p>
              If you are requesting this in order to draft it, we ask that you would instead use the "Unlisted Game" feature, which will allow you to draft a game not on the Master Game list. The game
              can be linked to a master game later if it does in fact get announced.
            </p>
            <p>
              If you are requesting this in order to bid on it, then it will need to be added to the master games list. Please use the 'notes' field above to provide an explanation of your situation
              and as much detail on this predicted game as you can.
            </p>
            <b-form-checkbox v-model="unannouncedCheckbox">
              <span class="checkbox-label">I have read and understand this message.</span>
            </b-form-checkbox>
          </div>

          <div class="form-group">
            <div class="right-button">
              <input type="submit" class="btn btn-primary" value="Submit" :disabled="invalid || !validDate || !validUnannounced || isBusy" />
            </div>
          </div>
        </form>
      </ValidationObserver>
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
              <th scope="col">Answered By</th>
              <th scope="col">Response</th>
              <th scope="col">Response Time</th>
              <th scope="col"></th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="request in myRequests" :key="request.requestID">
              <td>
                <span v-if="request.masterGame"><masterGamePopover :master-game="request.masterGame"></masterGamePopover></span>
                <span v-else>{{ request.gameName }}</span>
              </td>
              <td>
                <span v-if="request.responseUser">{{ request.responseUser.displayName }}</span>
                <span v-else>&lt;Pending&gt;</span>
              </td>
              <td>
                <span v-if="request.responseNote">{{ request.responseNote }}</span>
                <span v-else>&lt;Pending&gt;</span>
              </td>
              <td>
                <span v-if="request.responseTimestamp">{{ request.responseTimestamp | dateTime }}</span>
                <span v-else>&lt;Pending&gt;</span>
              </td>
              <td class="select-cell">
                <span v-if="request.answered"><b-button variant="info" size="sm" @click="dismissRequest(request)">Dismiss Request</b-button></span>
                <span v-else><b-button variant="danger" size="sm" @click="cancelRequest(request)">Cancel Request</b-button></span>
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
import MasterGamePopover from '@/components/masterGamePopover.vue';

export default {
  components: {
    MasterGamePopover
  },
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
      nearCertainInterested: false,
      unannouncedCheckbox: false,
      isBusy: false
    };
  },
  computed: {
    validReason() {
      return this.wantToPickup || this.nearCertainInterested;
    },
    validDate() {
      return !!this.estimatedReleaseDate || !!this.releaseDate;
    },
    showUnannouncedWarning() {
      const fieldsToCheck = [this.gameName, this.estimatedReleaseDate, this.requestNote];
      const keywords = ['unannounced', 'rumor'];

      for (const variable of fieldsToCheck) {
        for (const word of keywords) {
          if (variable.toLowerCase().includes(word.toLowerCase())) {
            return true;
          }
        }
      }
      return false;
    },
    validUnannounced() {
      if (!this.showUnannouncedWarning) {
        return true;
      }
      if (!this.requestNote || this.requestNote.length < 20) {
        return false;
      }
      return this.unannouncedCheckbox;
    }
  },
  async created() {
    await this.fetchMyRequests();
  },
  methods: {
    async fetchMyRequests() {
      const response = await axios.get('/api/game/MyMasterGameRequests');
      this.myRequests = response.data;
    },
    async sendMasterGameRequestRequest() {
      let request = {
        gameName: this.gameName,
        requestNote: this.requestNote,
        steamLink: this.steamLink,
        openCriticLink: this.openCriticLink,
        ggLink: this.ggLink,
        estimatedReleaseDate: this.estimatedReleaseDate
      };

      if (!this.hasReleaseDate && this.estimatedReleaseDate) {
        request.estimatedReleaseDate = this.estimatedReleaseDate;
      }
      if (this.hasReleaseDate && this.releaseDate) {
        request.releaseDate = this.releaseDate;
        request.estimatedReleaseDate = this.releaseDate;
      }

      try {
        this.isBusy = true;
        await axios.post('/api/game/CreateMasterGameRequest', request);
        this.showSent = true;
        window.scroll({
          top: 0,
          left: 0,
          behavior: 'smooth'
        });
        this.clearData();
        await this.fetchMyRequests();
      } catch (error) {
        this.errorInfo = error.response.data;
      } finally {
        this.isBusy = false;
      }
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
    async cancelRequest(request) {
      let model = {
        requestID: request.requestID
      };
      await axios.post('/api/game/DeleteMasterGameRequest', model);
      this.showDeleted = true;
      await this.fetchMyRequests();
    },
    async dismissRequest(request) {
      let model = {
        requestID: request.requestID
      };
      await axios.post('/api/game/DismissMasterGameRequest', model);
      await this.fetchMyRequests();
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
