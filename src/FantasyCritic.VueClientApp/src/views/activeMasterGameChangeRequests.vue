<template>
  <div class="col-md-10 offset-md-1 col-sm-12">
    <div>
      <h1>Active Master Game Change Requests</h1>
      <b-button variant="info" :to="{ name: 'adminConsole' }">Admin Console</b-button>
    </div>
    <hr />
    <b-alert v-if="errorInfo" variant="danger" :model-value="true">{{ errorInfo }}</b-alert>
    <b-alert v-if="successMessage" variant="success" :model-value="true">{{ successMessage }}</b-alert>

    <b-alert v-if="activeRequests && activeRequests.length === 0" variant="info" :model-value="true">No active requests.</b-alert>

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
              <a v-if="request.openCriticID" :href="openCriticLink(request)" target="_blank">
                <strong>
                  OpenCritic Link
                  <font-awesome-icon icon="external-link-alt" />
                </strong>
              </a>
            </td>
            <td>
              <a v-if="request.ggToken" :href="getGGLinkForGame(request) ?? undefined" target="_blank">
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
              <b-button v-if="request.openCriticID" variant="danger" size="sm" :disabled="isBusy" @click="linkToOpenCritic(request.masterGame, request.openCriticID)">Link OpenCritic</b-button>
            </td>
            <td class="select-cell">
              <b-button v-if="request.ggToken" variant="danger" size="sm" :disabled="isBusy" @click="linkToGG(request.masterGame, request.ggToken)">Link GG|</b-button>
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
            <label for="generatedSQL" class="control-label">GeneratedSQL</label>
            <input id="generatedSQL" v-model="generatedSQL" name="generatedSQL" class="form-control input" />
          </div>
        </div>
      </div>
    </div>

    <div v-if="requestSelected">
      <h3>Respond to Request for {{ requestSelected.masterGame.gameName }}</h3>
      <div class="row">
        <div class="col-xl-8 col-lg-10 col-md-12 text-well">
          <form @submit.prevent="respondToRequest">
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
    </div>
  </div>
</template>
<script lang="ts">
import { defineComponent } from 'vue';

import {
  factCheckerClient,
  type CompleteMasterGameChangeRequestRequest,
  type LinkGameToGGRequest,
  type LinkGameToOpenCriticRequest,
  type MasterGameChangeRequestViewModel,
  type MasterGameViewModel
} from '@/api/clients';
import { getApiErrorMessage } from '@/utilities/apiError';
import { getGGLinkForGame } from '@/utilities/ggLinks';
import { openCriticLink } from '@/utilities/openCriticLinks';
import MasterGamePopover from '@/components/masterGamePopover.vue';

interface ActiveMasterGameChangeRequestsData {
  activeRequests: MasterGameChangeRequestViewModel[] | null;
  isBusy: boolean;
  errorInfo: string;
  successMessage: string;
  requestSelected: MasterGameChangeRequestViewModel | null;
  responseNote: string;
  generatedSQL: string;
}

export default defineComponent({
  components: {
    MasterGamePopover
  },
  data(): ActiveMasterGameChangeRequestsData {
    return {
      activeRequests: null,
      isBusy: false,
      errorInfo: '',
      successMessage: '',
      requestSelected: null,
      responseNote: '',
      generatedSQL: ''
    };
  },
  async created() {
    try {
      await this.fetchRequests();
    } catch (error) {
      this.errorInfo = getApiErrorMessage(error, 'Could not load the change requests.');
    }
  },
  methods: {
    openCriticLink,
    getGGLinkForGame,
    async fetchRequests(): Promise<void> {
      this.activeRequests = await factCheckerClient.activeMasterGameChangeRequests();
    },
    createResponse(request: MasterGameChangeRequestViewModel): void {
      this.requestSelected = request;
      this.responseNote = 'Got that fixed, thanks!';
    },
    //Runs one request, with its result in the banners at the top of the page.
    async runAction(successMessage: string, fallback: string, call: () => Promise<unknown>): Promise<void> {
      this.isBusy = true;
      this.errorInfo = '';
      this.successMessage = '';
      try {
        await call();
        this.successMessage = successMessage;
      } catch (error) {
        this.errorInfo = getApiErrorMessage(error, fallback);
      } finally {
        this.isBusy = false;
      }
    },
    async respondToRequest(): Promise<void> {
      const requestSelected = this.requestSelected;
      if (!requestSelected) {
        return;
      }

      const request: CompleteMasterGameChangeRequestRequest = {
        requestID: requestSelected.requestID,
        responseNote: this.responseNote
      };

      await this.runAction(`Responded to the request for ${requestSelected.masterGame.gameName}.`, 'Something went wrong when responding to the request.', async () => {
        await factCheckerClient.completeMasterGameChangeRequest(request);
        this.requestSelected = null;
        await this.fetchRequests();
      });
    },
    async linkToOpenCritic(masterGame: MasterGameViewModel, openCriticID: number): Promise<void> {
      const request: LinkGameToOpenCriticRequest = {
        masterGameID: masterGame.masterGameID,
        openCriticID
      };

      await this.runAction(`${masterGame.gameName} has been linked to OpenCritic.`, 'Something went wrong when linking the game to OpenCritic.', () => factCheckerClient.linkGameToOpenCritic(request));
    },
    async linkToGG(masterGame: MasterGameViewModel, ggToken: string): Promise<void> {
      const request: LinkGameToGGRequest = {
        masterGameID: masterGame.masterGameID,
        ggToken
      };

      await this.runAction(`${masterGame.gameName} has been linked to GG|.`, 'Something went wrong when linking the game to GG|.', () => factCheckerClient.linkGameToGG(request));
    },
    generateSQL(request: MasterGameChangeRequestViewModel): void {
      this.generatedSQL = `select * from tbl_mastergame where MasterGameID = '${request.masterGame.masterGameID}';`;
    }
  }
});
</script>
<style scoped>
.select-cell {
  text-align: center;
}
</style>
