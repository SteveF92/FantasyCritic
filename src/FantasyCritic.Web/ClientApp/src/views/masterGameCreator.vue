<template>
  <div>
    <div class="col-md-10 offset-md-1 col-sm-12">
      <div>
        <h1>Create Master Game</h1>
        <b-button variant="info" :to="{ name: 'activeMasterGameRequests' }">View master game requests</b-button>
        <b-button variant="info" :to="{ name: 'adminConsole' }">Admin Console</b-button>
      </div>
      <hr />
      <div v-if="createdGame" class="alert alert-success">
        Master Game created: {{ createdGame.masterGameID }}
        <b-button variant="info" size="sm" v-clipboard:copy="createdGame.masterGameID">Copy</b-button>
        <b-button class="warning" :to="{ name: 'masterGameEditor', params: { mastergameid: createdGame.masterGameID } }">Edit Master Game</b-button>
      </div>
      <div v-if="errorInfo" class="alert alert-danger">An error has occurred with your request.</div>
      <div v-if="openCriticID || steamID || ggToken">
        <h2>Links</h2>
        <ul>
          <li>
            <a v-if="openCriticID" :href="openCriticLink" target="_blank">
              OpenCritic Link
              <font-awesome-icon icon="external-link-alt" />
            </a>
            <span v-else>No OpenCritic Link</span>
          </li>
          <li>
            <a v-if="steamID" :href="steamLink" target="_blank">
              Steam Link
              <font-awesome-icon icon="external-link-alt" />
            </a>
            <span v-else>No Steam Link</span>
          </li>
          <li>
            <a v-if="ggToken" :href="ggLink" target="_blank">
              GG| Link
              <font-awesome-icon icon="external-link-alt" />
            </a>
            <span v-else>No GG| Link</span>
          </li>
        </ul>
        <hr />
      </div>
      <div class="row" v-if="requestNote">
        <div class="col-lg-10 col-md-12 offset-lg-1 text-well">
          <h2>Request Note</h2>
          <p>{{ requestNote }}</p>
        </div>
        <hr />
      </div>
      <div class="row">
        <div class="col-lg-10 col-md-12 offset-lg-1 text-well">
          <ValidationObserver v-slot="{ invalid }">
            <form v-on:submit.prevent="createMasterGame">
              <div class="form-group">
                <label for="gameName" class="control-label">Game Name</label>
                <ValidationProvider rules="required" v-slot="{ errors }">
                  <input v-model="gameName" id="gameName" name="Game Name" type="text" class="form-control input" />
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

              <div class="form-group">
                <label for="ggToken" class="control-label">GG| Token</label>
                <input v-model="ggToken" id="ggToken" name="ggToken" class="form-control input" />
              </div>

              <h3>Tags</h3>
              <masterGameTagSelector v-model="tags" :includeSystem="true"></masterGameTagSelector>

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
</template>
<script>
import axios from 'axios';
import MasterGameTagSelector from '@/components/masterGameTagSelector';

export default {
  data() {
    return {
      createdGame: null,
      errorInfo: '',
      requestNote: '',
      steamID: null,
      openCriticID: null,
      ggToken: null,
      gameName: '',
      estimatedReleaseDate: '',
      minimumReleaseDate: null,
      maximumReleaseDate: null,
      earlyAccessReleaseDate: null,
      internationalReleaseDate: null,
      releaseDate: null,
      notes: '',
      request: null,
      tags: []
    };
  },
  components: {
    MasterGameTagSelector
  },
  computed: {
    openCriticLink() {
      return 'https://opencritic.com/game/' + this.openCriticID + '/a';
    },
    steamLink() {
      return 'https://store.steampowered.com/app/' + this.steamID;
    },
    ggLink() {
      return `https://ggapp.io/games/${this.ggToken}`;
    }
  },
  methods: {
    createMasterGame() {
      let tagNames = _.map(this.tags, 'name');

      let request = {
        gameName: this.gameName,
        estimatedReleaseDate: this.estimatedReleaseDate,
        minimumReleaseDate: this.minimumReleaseDate,
        maximumReleaseDate: this.maximumReleaseDate,
        earlyAccessReleaseDate: this.earlyAccessReleaseDate,
        internationalReleaseDate: this.internationalReleaseDate,
        releaseDate: this.releaseDate,
        openCriticID: this.openCriticID,
        ggToken: this.ggToken,
        tags: tagNames,
        notes: this.notes
      };
      axios
        .post('/api/admin/CreateMasterGame', request)
        .then((response) => {
          this.createdGame = response.data;
          window.scroll({
            top: 0,
            left: 0,
            behavior: 'smooth'
          });
          this.clearData();
        })
        .catch((error) => {
          this.errorInfo = error.response;
        });
    },
    async parseEstimatedReleaseDate() {
      await axios
        .get('/api/admin/ParseEstimatedDate?estimatedReleaseDate=' + this.estimatedReleaseDate)
        .then((response) => {
          this.minimumReleaseDate = response.data.minimumReleaseDate;
          this.maximumReleaseDate = response.data.maximumReleaseDate;
        })
        .catch((returnedError) => (this.error = returnedError));
    },
    async fetchRequest() {
      let requestID = this.$route.query.requestID;
      if (!requestID) {
        return;
      }
      await axios
        .get('/api/admin/GetMasterGameRequest?requestID=' + requestID)
        .then((response) => {
          this.request = response.data;
          this.gameName = this.request.gameName;
          this.estimatedReleaseDate = this.request.estimatedReleaseDate;
          if (this.request.releaseDate !== undefined) {
            this.releaseDate = this.request.releaseDate;
            this.minimumReleaseDate = this.request.releaseDate;
            this.maximumReleaseDate = this.request.releaseDate;
          }
          this.steamID = this.request.steamID;
          this.openCriticID = this.request.openCriticID;
          this.ggToken = this.request.ggToken;
          this.requestNote = this.request.requestNote;
        })
        .catch((returnedError) => (this.error = returnedError));
    },
    propagateDate() {
      this.maximumReleaseDate = this.releaseDate;
      this.minimumReleaseDate = this.releaseDate;
      this.estimatedReleaseDate = this.releaseDate;
    },
    clearDates() {
      this.releaseDate = null;
      this.minimumReleaseDate = null;
      this.maximumReleaseDate = null;
      this.estimatedReleaseDate = null;
      this.internationalReleaseDate = null;
      this.earlyAccessReleaseDate = null;
    }
  },
  async mounted() {
    await this.fetchRequest();
    this.parseEstimatedReleaseDate();
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
