<template>
  <div>
    <h1>Active Master Game Change Requests</h1>
    <div v-if="showResponded" class="alert alert-success">Responded to request.</div>

    <div v-if="activeRequests && activeRequests.length === 0" class="alert alert-info">No active requests.</div>

    <div class="col-xl-8 col-lg-10 col-md-12" v-if="activeRequests && activeRequests.length !== 0">
      <div class="row">
        <table class="table table-sm table-responsive-sm table-bordered table-striped">
          <thead>
            <tr class="bg-primary">
              <th scope="col" class="game-column">Game Name</th>
              <th scope="col">User Name</th>
              <th scope="col">Note</th>
              <th scope="col">OpenCritic ID</th>
              <th scope="col"></th>
              <th scope="col"></th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="request in activeRequests">
              <td>{{request.masterGame.gameName}}</td>
              <td>{{request.requesterDisplayName}}</td>
              <td>{{request.requestNote}}</td>
              <td>
                  <a v-if="request.openCriticID" :href="openCriticLink(request.openCriticID)" target="_blank"><strong>OpenCritic Link <font-awesome-icon icon="external-link-alt" /></strong></a>
              </td>
              <td class="select-cell">
                <b-button variant="danger" size="sm" v-on:click="createResponse(request)">Respond</b-button>
              </td>
              <td class="select-cell">
                <b-button variant="danger" size="sm">Link to OpenCritic</b-button>
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
        responseNote: ""
      }
    },
    computed: {

    },
    methods: {
      fetchMyRequests() {
        axios
          .get('/api/admin/ActiveMasterGameChangeRequests')
          .then(response => {
            this.activeRequests = response.data;
          })
          .catch(response => {

          });
      },
      createResponse(request) {
        this.requestSelected = request;
      },
      respondToRequest() {
        let request = {
          requestID: this.requestSelected.requestID,
          responseNote: this.responseNote
        };
        axios
          .post('/api/admin/CompleteMasterGameChangeRequest', request)
          .then(response => {
            this.showResponded = true;
          })
          .catch(error => {
            this.errorInfo = error.response;
          });
      },
      openCriticLink(openCriticID) {
        return "https://opencritic.com/game/" + openCriticID + "/a";
      },
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
