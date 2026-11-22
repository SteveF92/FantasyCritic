<template>
  <div class="col-md-10 offset-md-1 col-sm-12">
    <div>
      <h1>Active Master Game Requests</h1>
      <b-button variant="info" :to="{ name: 'adminConsole' }">Admin Console</b-button>
    </div>
    <hr />
    <b-alert v-if="errorInfo" variant="danger" :model-value="true">{{ errorInfo }}</b-alert>
    <b-alert v-if="respondedTo" variant="success" :model-value="true">Responded to the request for {{ respondedTo }}.</b-alert>

    <div v-if="requestSelected">
      <h3>Respond to Request for {{ requestSelected.gameName }}</h3>
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
                <input type="submit" class="btn btn-primary" value="Submit" :disabled="isBusy || !responseNote.trim()" />
              </div>
            </div>
          </form>
        </div>
      </div>
      <hr />
    </div>

    <b-alert v-if="activeRequests && activeRequests.length === 0" variant="info" :model-value="true">No active requests.</b-alert>
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
<script lang="ts">
import { defineComponent } from 'vue';

import { factCheckerClient, type CompleteMasterGameRequestRequest, type MasterGameRequestViewModel } from '@/api/clients';
import { getApiErrorMessage } from '@/utilities/apiError';

interface ActiveMasterGameRequestsData {
  activeRequests: MasterGameRequestViewModel[] | null;
  isBusy: boolean;
  errorInfo: string;
  respondedTo: string;
  requestSelected: MasterGameRequestViewModel | null;
  masterGameID: string;
  responseNote: string;
}

export default defineComponent({
  data(): ActiveMasterGameRequestsData {
    return {
      activeRequests: null,
      isBusy: false,
      errorInfo: '',
      respondedTo: '',
      requestSelected: null,
      masterGameID: '',
      responseNote: ''
    };
  },
  async created() {
    try {
      await this.fetchRequests();
    } catch (error) {
      this.errorInfo = getApiErrorMessage(error, 'Could not load the requests.');
    }
  },
  methods: {
    async fetchRequests(): Promise<void> {
      this.activeRequests = await factCheckerClient.activeMasterGameRequests();
    },
    assignGame(request: MasterGameRequestViewModel): void {
      this.requestSelected = request;
      this.masterGameID = '';
      this.responseNote = 'Got that added, thanks!';
    },
    async respondToRequest(): Promise<void> {
      if (!this.requestSelected) {
        return;
      }

      const masterGameID = this.masterGameID.trim();
      const request: CompleteMasterGameRequestRequest = {
        requestID: this.requestSelected.requestID,
        responseNote: this.responseNote,
        masterGameID: masterGameID || undefined
      };

      this.isBusy = true;
      this.errorInfo = '';
      this.respondedTo = '';
      try {
        await factCheckerClient.completeMasterGameRequest(request);
        this.respondedTo = this.requestSelected.gameName;
        this.requestSelected = null;
        await this.fetchRequests();
      } catch (error) {
        this.errorInfo = getApiErrorMessage(error, 'Something went wrong when responding to the request.');
      } finally {
        this.isBusy = false;
      }
    }
  }
});
</script>
<style scoped>
.select-cell {
  text-align: center;
}
</style>
