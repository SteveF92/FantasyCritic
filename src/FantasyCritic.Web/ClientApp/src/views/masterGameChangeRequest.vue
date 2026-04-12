<template>
  <div class="col-md-10 offset-md-1 col-sm-12 offset-sm-0">
    <h1>Master Game Correction Request</h1>
    <hr />
    <div v-if="showSent" class="alert alert-success">Master Game request made.</div>
    <div v-if="showDeleted" class="alert alert-success">Master Game request was deleted.</div>
    <div v-if="errorInfo" class="alert alert-danger">An error has occurred with your request.</div>
    <div class="col-lg-10 col-md-12 offset-lg-1 offset-md-0">
      <div class="text-well">
        <p>If you see an issue with a game on the site, for example an incorrect release date, you can send me a note and I'll get it fixed.</p>
        <p>
          However, if this is a "debateable" correction, like whether or not a game is "Remake" or a "Partial Remake", please come to the "eligibility-debates" channel in our
          <a href="https://discord.gg/dNa7DD3">
            Discord
            <font-awesome-icon icon="external-link-alt" size="sm" />
          </a>
          so the community can discuss it.
        </p>
        <p v-show="masterGame">You can also use this form to let me know about a missing link to OpenCritic, so scores can populate, or GG|, so the game will have an image to represent it.</p>
      </div>
      <hr />
      <div v-if="masterGame && masterGame.numberOutstandingCorrections" class="alert alert-warning">
        There are {{ masterGame.numberOutstandingCorrections }} correction(s) currently submitted that I have not reviewed. You may not need to submit anything.
      </div>
      <p v-if="!masterGame">
        <strong>You can suggest a correction by clicking a link on a master game's page.</strong>
      </p>
      <div v-if="masterGame" class="row">
        <div class="col-lg-10 col-md-12 offset-lg-1 offset-md-0 text-well">
          <div class="game-header">
            <img v-if="coverArtLink" :src="coverArtLink" alt="Cover Art" class="cover-art" />
            <div class="game-info-section">
              <h2>{{ masterGame.gameName }}</h2>
              <div class="game-external-links">
                <a v-if="masterGame.openCriticID" :href="gameOpenCriticLink" target="_blank" class="game-link btn btn-sm btn-outline-info">
                  <font-awesome-icon icon="external-link-alt" size="xs" />
                  OpenCritic
                </a>
                <span v-else class="badge badge-secondary link-missing-badge">No OpenCritic link</span>
                <a v-if="gameGGLink" :href="gameGGLink" target="_blank" class="game-link btn btn-sm btn-outline-info">
                  <font-awesome-icon icon="external-link-alt" size="xs" />
                  GG|
                </a>
                <span v-else class="badge badge-secondary link-missing-badge">No GG| link</span>
              </div>
              <masterGameDetails :master-game="masterGame" :hide-notes="true"></masterGameDetails>
            </div>
          </div>

          <b-alert v-if="masterGame.notes" show variant="info" class="mt-3">
            <strong><font-awesome-icon icon="info-circle" /> Special Notes:</strong>
            {{ masterGame.notes }}
          </b-alert>

          <hr />

          <ValidationObserver v-slot="{ invalid }">
            <form @submit.prevent="sendMasterGameChangeRequestRequest">
              <div class="form-group">
                <label for="requestNote" class="control-label">What seems to be the issue?</label>
                <ValidationProvider v-slot="{ errors }" rules="required">
                  <input id="requestNote" v-model="requestNote" name="Request Note" type="text" class="form-control input" />
                  <span class="text-danger">{{ errors[0] }}</span>
                </ValidationProvider>
              </div>

              <div class="form-group">
                <label for="openCriticLink" class="control-label">Link to Open Critic Page (Optional)</label>
                <input id="openCriticLink" v-model="openCriticLink" name="openCriticLink" class="form-control input" />
              </div>

              <div class="form-group">
                <label for="ggLink" class="control-label">Link to GG| Page (Optional)</label>
                <input id="ggLink" v-model="ggLink" name="ggLink" class="form-control input" />
              </div>

              <b-alert variant="warning" :show="invalidOpenCriticRequest">
                If you are requesting an Open Critic link, please include the link here. It is possible that Open Critic may not have created a page yet if this game was recently released or is a
                lesser known title.
              </b-alert>

              <div class="form-group">
                <div class="right-button">
                  <input type="submit" class="btn btn-primary" value="Submit" :disabled="invalid || isBusy || invalidOpenCriticRequest" />
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
                <th scope="col">Answered By</th>
                <th scope="col">Response</th>
                <th scope="col">Response Time</th>
                <th scope="col"></th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="request in myRequests" :key="request.requestID">
                <td>
                  <span><masterGamePopover :master-game="request.masterGame"></masterGamePopover></span>
                </td>
                <td>
                  <span>{{ request.requestNote }}</span>
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
  </div>
</template>
<script>
import axios from 'axios';
import MasterGamePopover from '@/components/masterGamePopover.vue';
import MasterGameDetails from '@/components/masterGameDetails.vue';
import GGMixin from '@/mixins/ggMixin.js';

export default {
  components: {
    MasterGamePopover,
    MasterGameDetails
  },
  mixins: [GGMixin],
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
        backgroundColor: '#ccc',
        visibility: 'visible',
        width: '12px',
        height: '20px'
      },
      isBusy: false
    };
  },
  async created() {
    let masterGameID = this.$route.query.mastergameid;
    if (masterGameID) {
      await this.fetchMasterGame(masterGameID);
    }

    await this.fetchMyRequests();
  },
  computed: {
    coverArtLink() {
      if (!this.masterGame) return null;
      return this.getGGCoverArtLinkForGame(this.masterGame, 150);
    },
    gameGGLink() {
      if (!this.masterGame) return null;
      return this.getGGLinkForGame(this.masterGame);
    },
    gameOpenCriticLink() {
      if (!this.masterGame || !this.masterGame.openCriticID) return null;
      return `https://opencritic.com/game/${this.masterGame.openCriticID}/${this.masterGame.openCriticSlug ?? 'b'}`;
    },
    invalidOpenCriticRequest() {
      if (!this.requestNote) {
        return false;
      }

      if (this.openCriticLink) {
        return false;
      }

      const searchStrings = ['Open Critic', 'OpenCritic', ' OC '];

      return searchStrings.some((x) => this.requestNote.toLocaleUpperCase().includes(x.toLocaleUpperCase()));
    }
  },
  methods: {
    async fetchMyRequests() {
      const response = await axios.get('/api/game/MyMasterGameChangeRequests');
      this.myRequests = response.data;
    },
    async sendMasterGameChangeRequestRequest() {
      if (!this.requestNote && this.openCriticLink) {
        this.requestNote = 'Link to OpenCritic';
      }
      let request = {
        masterGameID: this.masterGame.masterGameID,
        requestNote: this.requestNote,
        openCriticLink: this.openCriticLink,
        ggLink: this.ggLink
      };

      try {
        this.isBusy = true;
        await axios.post('/api/game/CreateMasterGameChangeRequest', request);
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
      this.requestNote = '';
      this.openCriticLink = '';
      this.ggLink = '';
    },
    async cancelRequest(request) {
      let model = {
        requestID: request.requestID
      };
      await axios.post('/api/game/DeleteMasterGameChangeRequest', model);
      this.showDeleted = true;
      await this.fetchMyRequests();
    },
    async dismissRequest(request) {
      let model = {
        requestID: request.requestID
      };
      await axios.post('/api/game/DismissMasterGameChangeRequest', model);
      await this.fetchMyRequests();
    },
    async fetchMasterGame(masterGameID) {
      try {
        const response = await axios.get('/api/game/MasterGame/' + masterGameID);
        this.masterGame = response.data;
      } catch (error) {
        this.error = error.response.data;
      }
    }
  }
};
</script>
<style scoped>
.select-cell {
  text-align: center;
}

label {
  font-size: 18px;
}

.game-header {
  display: flex;
  flex-direction: row;
  gap: 20px;
  align-items: center;
  flex-wrap: wrap;
}

.cover-art {
  border-radius: 6px;
  flex-shrink: 0;
  width: 150px;
  align-self: flex-start;
}

.game-info-section {
  flex: 1;
  min-width: 0;
}

.game-external-links {
  display: flex;
  flex-wrap: wrap;
  gap: 8px;
  margin-bottom: 12px;
  align-items: center;
}

.game-link {
  display: inline-flex;
  align-items: center;
  gap: 5px;
}

.link-missing-badge {
  font-size: 0.85em;
  padding: 4px 8px;
}
</style>
