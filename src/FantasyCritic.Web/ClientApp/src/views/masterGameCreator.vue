<template>
  <div class="col-md-10 offset-md-1 col-sm-12">
    <div>
      <h1>Create Master Game</h1>
      <b-button variant="info" :to="{ name: 'activeMasterGameRequests' }">View master game requests</b-button>
      <b-button variant="info" :to="{ name: 'adminConsole' }">Admin Console</b-button>
    </div>
    <hr />
    <div v-if="createdGame" class="alert alert-success">
      Master Game created: {{ createdGame.masterGameID }}
      <b-button v-clipboard:copy="createdGame.masterGameID" variant="info" size="sm">Copy</b-button>
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
    <div v-if="requestNote" class="row">
      <div class="col-lg-10 col-md-12 offset-lg-1 text-well">
        <h2>Request Note</h2>
        <p>{{ requestNote }}</p>
      </div>
      <hr />
    </div>
    <div v-if="request?.leagueOptionsForRequestingPlayer?.length > 0" class="row">
      <div class="col-lg-10 col-md-12 offset-lg-1 text-well">
        <h3>League Options for Requesting Player</h3>
        <ul>
          <li v-for="(leagueOptions, index) in request.leagueOptionsForRequestingPlayer" :key="`${leagueOptions.leagueID}-${leagueOptions.year}`">
            League {{ index + 1 }}
            <router-link :to="{ name: 'league', params: { leagueid: leagueOptions.leagueID, year: leagueOptions.year } }" class="league-link">(Link)</router-link>
            <span v-b-modal="`leagueOptionsModal_${leagueOptions.leagueID}-${leagueOptions.year}`" class="fake-link action">(Settings)</span>
            <leagueOptionsModal :league-year-options="leagueOptions" :supported-year="supportedYears.find((x) => x.year === leagueOptions.year)"></leagueOptionsModal>
          </li>
        </ul>
      </div>
      <hr />
    </div>
    <div class="row">
      <div class="col-lg-10 col-md-12 offset-lg-1 text-well">
        <ValidationObserver v-slot="{ invalid }">
          <form @submit.prevent="createMasterGame">
            <div class="form-group">
              <label for="gameName" class="control-label">Game Name</label>
              <ValidationProvider v-slot="{ errors }" rules="required">
                <input id="gameName" v-model="gameName" name="Game Name" type="text" class="form-control input" />
                <span class="text-danger">{{ errors[0] }}</span>
              </ValidationProvider>
            </div>

            <div class="form-group">
              <label for="releaseDate" class="control-label">Release Date</label>
              <b-form-datepicker v-model="releaseDate" class="form-control"></b-form-datepicker>
            </div>

            <b-button variant="info" size="sm" @click="propagateDate">Propagate Date</b-button>
            <b-button variant="info" size="sm" @click="parseEstimatedReleaseDate">Parse Estimated</b-button>
            <b-button variant="warning" size="sm" @click="clearDates">Clear Dates</b-button>

            <div class="form-group">
              <label for="estimatedReleaseDate" class="control-label">Estimated Release Date</label>
              <input id="estimatedReleaseDate" v-model="estimatedReleaseDate" name="estimatedReleaseDate" class="form-control input" />
            </div>
            <div class="form-group">
              <label for="minimumReleaseDate" class="control-label">Minimum Release Date</label>
              <b-form-datepicker v-model="minimumReleaseDate" class="form-control"></b-form-datepicker>
            </div>
            <div class="form-group">
              <label for="maximumReleaseDate" class="control-label">Maximum Release Date</label>
              <b-form-datepicker v-model="maximumReleaseDate" class="form-control"></b-form-datepicker>
            </div>
            <div class="form-group">
              <label for="earlyAccessReleaseDate" class="control-label">Early Access Release Date</label>
              <b-form-datepicker v-model="earlyAccessReleaseDate" class="form-control"></b-form-datepicker>
            </div>
            <div class="form-group">
              <label for="internationalReleaseDate" class="control-label">International Release Date</label>
              <b-form-datepicker v-model="internationalReleaseDate" class="form-control"></b-form-datepicker>
            </div>

            <div class="form-group">
              <label for="openCriticID" class="control-label">Open Critic ID</label>
              <input id="openCriticID" v-model="openCriticID" name="openCriticID" class="form-control input" />
            </div>

            <div class="form-group">
              <label for="ggToken" class="control-label">GG| Token</label>
              <input id="ggToken" v-model="ggToken" name="ggToken" class="form-control input" />
            </div>

            <h3>Tags</h3>
            <masterGameTagSelector v-model="tags" include-system></masterGameTagSelector>

            <div class="form-group">
              <label for="notes" class="control-label">Other Notes</label>
              <input id="notes" v-model="notes" name="notes" class="form-control input" />
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
</template>
<script>
import axios from 'axios';

import MasterGameTagSelector from '@/components/masterGameTagSelector.vue';
import LeagueOptionsModal from '@/components/modals/leagueOptionsModal.vue';
import BasicMixin from '@/mixins/basicMixin.js';

export default {
  components: {
    MasterGameTagSelector,
    LeagueOptionsModal
  },
  mixins: [BasicMixin],
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
  computed: {
    openCriticLink() {
      return `https://opencritic.com/game/${this.openCriticID}/a`;
    },
    steamLink() {
      return 'https://store.steampowered.com/app/' + this.steamID;
    },
    ggLink() {
      return `https://ggapp.io/games/${this.ggToken}/a`;
    }
  },
  async created() {
    await this.fetchRequest();
    await this.parseEstimatedReleaseDate();
  },
  methods: {
    async createMasterGame() {
      let tagNames = this.tags.map((x) => x.name);

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

      try {
        const response = await axios.post('/api/factChecker/CreateMasterGame', request);
        this.createdGame = response.data;
        window.scroll({
          top: 0,
          left: 0,
          behavior: 'smooth'
        });
      } catch (error) {
        this.errorInfo = error;
      }
    },
    async parseEstimatedReleaseDate() {
      try {
        if (this.releaseDate || !this.estimatedReleaseDate) {
          return;
        }
        const response = await axios.get('/api/factChecker/ParseEstimatedDate?estimatedReleaseDate=' + this.estimatedReleaseDate);
        this.minimumReleaseDate = response.data.minimumReleaseDate;
        this.maximumReleaseDate = response.data.maximumReleaseDate;
      } catch (error) {
        this.errorInfo = error;
      }
    },
    async fetchRequest() {
      let requestID = this.$route.query.requestID;
      if (!requestID) {
        return;
      }

      try {
        const response = await axios.get('/api/factChecker/GetMasterGameRequest?requestID=' + requestID);
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
      } catch (error) {
        this.errorInfo = error;
      }
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

.league-link {
  margin-right: 5px;
}
</style>
<style>
.vue-slider-piecewise-label {
  color: white !important;
}
</style>
