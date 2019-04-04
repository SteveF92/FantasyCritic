<template>
  <div>
    <h1>Active Master Game Requests</h1>
    <div v-if="showResponded" class="alert alert-success">Responded to request.</div>

    <div v-if="activeRequests && activeRequests.length === 0" class="alert alert-info">No active requests.</div>

    <div class="col-xl-8 col-lg-10 col-md-12" v-if="activeRequests && activeRequests.length !== 0">
      <div class="row">
        <table class="table table-sm table-responsive-sm table-bordered table-striped">
          <thead>
            <tr class="bg-primary">
              <th scope="col" class="game-column">Game Name</th>
              <th scope="col"></th>
              <th scope="col"></th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="request in activeRequests">
              <td>{{request.gameName}}</td>
              <td class="select-cell">
                <b-button variant="danger" size="sm" v-on:click="assignGame(request)">Assign Game</b-button>
              </td>
              <td class="select-cell">
                <b-button variant="danger" size="sm" v-on:click="createGame(request)">Create Game</b-button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
      <div v-if="requestSelected">
        <h3>Respond to Request</h3>
        <div class="row">
          <div class="col-xl-8 col-lg-10 col-md-12 text-well">
            <form v-on:submit.prevent="respondToRequest">
              <div class="form-group">
                <label for="masterGameID" class="control-label">Master Game ID</label>
                <input v-model="masterGameID" id="masterGameID" name="masterGameID" class="form-control input" />
              </div>
              <div class="form-group">
                <label for="responseNote" class="control-label">Response Note</label>
                <input v-model="responseNote" id="responseNote" name="responseNote" class="form-control input" />
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
        responseNote: ""
      }
    },
    computed: {

    },
    methods: {
      fetchMyRequests() {
        axios
          .get('/api/admin/ActiveMasterGameRequests')
          .then(response => {
            this.activeRequests = response.data;
          })
          .catch(response => {

          });
      },
      createGame(request) {
        let query = {
          gameName: request.gameName,
          estimatedReleaseDate: request.estimatedReleaseDate,
          steamID: request.steamID,
          openCriticID: request.openCriticID,
          eligibilityLevel: request.eligibilityLevel,
          yearlyInstallment: request.yearlyInstallment,
          earlyAccess: request.earlyAccess
        };
        this.$router.push({ name: 'masterGameCreator', query: query });
      },
      assignGame(request) {
        this.requestSelected = request;
      },
      respondToRequest() {
        let request = {
          requestID: this.requestSelected.requestID,
          responseNote: this.responseNote,
          masterGameID: this.masterGameID
        };
        axios
          .post('/api/admin/CompleteMasterGameRequest', request)
          .then(response => {
            this.showResponded = true;
          })
          .catch(error => {
            this.errorInfo = error.response;
          });
      }
    },
    mounted() {
      this.fetchMyRequests();
    }
  }
</script>
<style scoped>
  .select-cell {
    text-align: center;
  }
</style>
