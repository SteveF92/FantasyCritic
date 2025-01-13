<template>
  <div class="col-md-10 offset-md-1 col-sm-12">
    <div>
      <h1>Active Master Game Change Requests</h1>
      <b-button variant="info" :to="{ name: 'adminConsole' }">Admin Console</b-button>
    </div>
    <hr />
    <div v-if="showResponded" class="alert alert-success">Responded to request.</div>
    <div v-if="linkSuccessType" class="alert alert-success">Game has been linked to {{ linkSuccessType }}</div>

    <div v-if="activeRequests && activeRequests.length === 0" class="alert alert-info">No active requests.</div>

    <div v-if="activeRequests && activeRequests.length !== 0" class="row">
      <table class="table table-sm table-responsive-sm table-bordered table-striped">
        <thead>
          <tr class="bg-primary">
            <th scope="col" class="game-column">Game Name</th>
            <th scope="col">User Name</th>
            <th scope="col">Note</th>
            <th scope="col">OpenCritic ID</th>
            <th scope="col">GG Token</th>
            <th scope="col"></th>
            <th scope="col"></th>
            <th scope="col"></th>
            <th scope="col"></th>
            <th scope="col"></th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="request in activeRequests" :key="request.requestID">
            <td><masterGamePopover :master-game="request.masterGame"></masterGamePopover></td>
            <td>{{ request.requesterDisplayName }}</td>
            <td>{{ request.requestNote }}</td>
            <td>
              <a v-if="request.openCriticID" :href="openCriticLink(request.openCriticID)" target="_blank">
                <strong>
                  OpenCritic Link
                  <font-awesome-icon icon="external-link-alt" />
                </strong>
              </a>
            </td>
            <td>
              <a v-if="request.ggToken" :href="ggLink(request.ggToken)" target="_blank">
                <strong>
                  GG| Link
                  <font-awesome-icon icon="external-link-alt" />
                </strong>
              </a>
            </td>
            <td class="select-cell">
              <b-button variant="info" size="sm" @click="createResponse(request)">Respond</b-button>
            </td>
            <td class="select-cell">
              <b-button variant="info" :to="{ name: 'masterGameEditor', params: { mastergameid: request.masterGame.masterGameID }, query: { changeRequestID: request.requestID } }">Edit Game</b-button>
            </td>
            <td class="select-cell">
              <b-button variant="info" size="sm" @click="generateSQL(request)">Generate SQL</b-button>
            </td>
            <td class="select-cell">
              <b-button variant="danger" size="sm" @click="linkToOpenCritic(request)">Link OpenCritic</b-button>
            </td>
            <td class="select-cell">
              <b-button variant="danger" size="sm" @click="linkToGG(request)">Link GG|</b-button>
            </td>
          </tr>
        </tbody>
      </table>
    </div>

    <div v-if="generatedSQL">
      <h3>Generated SQL</h3>
      <div class="row">
        <div class="col-xl-8 col-lg-10 col-md-12 text-well">
          <div class="form-group">
            <label for="generated SQL" class="control-label">GeneratedSQL</label>
            <input id="generatedSQL" v-model="generatedSQL" name="generatedSQL" class="form-control input" />
          </div>
        </div>
      </div>
    </div>

    <div v-if="requestSelected">
      <h3>Respond to Request</h3>
      <div class="row">
        <div class="col-xl-8 col-lg-10 col-md-12 text-well">
          <form @submit.prevent="respondToRequest">
            <div class="form-group">
              <label for="responseNote" class="control-label">Response Note</label>
              <input id="responseNote" v-model="responseNote" name="responseNote" class="form-control input" />
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
      activeRequests: null,
      showResponded: false,
      linkSuccessType: null,
      requestSelected: null,
      responseNote: '',
      generatedSQL: ''
    };
  },
  mounted() {
    this.fetchMyRequests();
  },
  methods: {
    fetchMyRequests() {
      axios
        .get('/api/factChecker/ActiveMasterGameChangeRequests')
        .then((response) => {
          this.activeRequests = response.data;
        })
        .catch(() => {});
    },
    editGame(request) {
      let query = {
        changeRequestID: request.requestID
      };
      let params = {
        mastergameid: request.masterGame.masterGameID
      };
      this.$router.push({ name: 'masterGameEditor', params: params, query: query });
    },
    createResponse(request) {
      this.requestSelected = request;
      this.responseNote = 'Got that fixed, thanks!';
    },
    respondToRequest() {
      let request = {
        requestID: this.requestSelected.requestID,
        responseNote: this.responseNote
      };
      axios
        .post('/api/factChecker/CompleteMasterGameChangeRequest', request)
        .then(() => {
          this.showResponded = true;
        })
        .catch((error) => {
          this.errorInfo = error.response;
        });
    },
    openCriticLink(openCriticID) {
      return `https://opencritic.com/game/${openCriticID}/a`;
    },
    ggLink(ggToken) {
      return `https://ggapp.io/games/${ggToken}/a`;
    },
    linkToOpenCritic(request) {
      let linkRequest = {
        masterGameID: request.masterGame.masterGameID,
        openCriticID: request.openCriticID
      };

      axios
        .post('/api/factChecker/LinkGameToOpenCritic', linkRequest)
        .then(() => {
          this.linkSuccessType = 'Open Critic';
        })
        .catch((error) => {
          this.errorInfo = error.response;
        });
    },
    linkToGG(request) {
      let linkRequest = {
        masterGameID: request.masterGame.masterGameID,
        ggToken: request.ggToken
      };

      axios
        .post('/api/factChecker/LinkGameToGG', linkRequest)
        .then(() => {
          this.linkSuccessType = 'GG|';
        })
        .catch((error) => {
          this.errorInfo = error.response;
        });
    },
    generateSQL(request) {
      this.generatedSQL = "select * from tbl_mastergame where MasterGameID = '" + request.masterGame.masterGameID + "';";
    }
  }
};
</script>
<style scoped>
.select-cell {
  text-align: center;
}
</style>
