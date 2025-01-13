<template>
  <div class="col-md-10 offset-md-1 col-sm-12">
    <div>
      <h1>Active Master Game Requests</h1>
      <b-button variant="info" :to="{ name: 'adminConsole' }">Admin Console</b-button>
    </div>
    <hr />
    <div v-if="showResponded" class="alert alert-success">Responded to request.</div>

    <div v-if="requestSelected">
      <h3>Respond to Request</h3>
      <div class="row">
        <div class="col-xl-8 col-lg-10 col-md-12 text-well">
          <form @submit.prevent="respondToRequest">
            <div class="form-group">
              <label for="masterGameID" class="control-label">Master Game ID</label>
              <input id="masterGameID" v-model="masterGameID" name="masterGameID" class="form-control input" />
            </div>
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
      <hr />
    </div>

    <div v-if="activeRequests && activeRequests.length === 0" class="alert alert-info">No active requests.</div>
    <div v-if="activeRequests && activeRequests.length !== 0" class="row">
      <table class="table table-sm table-responsive-sm table-bordered table-striped">
        <thead>
          <tr class="bg-primary">
            <th scope="col" class="game-column">Game Name</th>
            <th scope="col">User Name</th>
            <th scope="col"></th>
            <th scope="col"></th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="request in activeRequests" :key="request.requestID">
            <td>{{ request.gameName }}</td>
            <td>{{ request.requesterDisplayName }}</td>
            <td class="select-cell">
              <b-button variant="info" size="sm" @click="assignGame(request)">Assign Game</b-button>
            </td>
            <td class="select-cell">
              <b-button variant="info" :to="{ name: 'masterGameCreator', query: { requestID: request.requestID } }">Create Game</b-button>
            </td>
          </tr>
        </tbody>
      </table>
    </div>
  </div>
</template>
<script>
import axios from 'axios';

export default {
  data() {
    return {
      activeRequests: null,
      showResponded: false,
      requestSelected: null,
      masterGameID: null,
      responseNote: ''
    };
  },
  created() {
    this.fetchMyRequests();
  },
  methods: {
    fetchMyRequests() {
      axios
        .get('/api/factChecker/ActiveMasterGameRequests')
        .then((response) => {
          this.activeRequests = response.data;
        })
        .catch(() => {});
    },
    assignGame(request) {
      this.requestSelected = request;
      this.responseNote = 'Got that added, thanks!';
    },
    respondToRequest() {
      let request = {
        requestID: this.requestSelected.requestID,
        responseNote: this.responseNote,
        masterGameID: this.masterGameID
      };
      axios
        .post('/api/factChecker/CompleteMasterGameRequest', request)
        .then(() => {
          this.showResponded = true;
        })
        .catch((error) => {
          this.errorInfo = error.response;
        });
    }
  }
};
</script>
<style scoped>
.select-cell {
  text-align: center;
}
</style>
