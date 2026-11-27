<template>
  <div class="col-md-10 offset-md-1 col-sm-12">
    <div>
      <h1>Pending Discord Game Updates</h1>
      <b-button variant="info" :to="{ name: 'adminConsole' }">Admin Console</b-button>
    </div>
    <hr />
    <p>These are the messages that are planned to be sent with the next cache refresh. If you don't want one to go out, you can delete it here.</p>
    <b-alert v-if="errorInfo" variant="danger" :model-value="true">{{ errorInfo }}</b-alert>

    <div v-if="pendingUpdates">
      <h3>New Games</h3>
      <b-alert v-if="pendingUpdates.newGames.length === 0" variant="info" :model-value="true">No new games pending.</b-alert>
      <table v-else class="table table-sm table-responsive-sm table-bordered table-striped">
        <thead>
          <tr class="bg-primary">
            <th scope="col" class="game-column">Game</th>
            <th scope="col"></th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="update in pendingUpdates.newGames" :key="update.masterGameUpdateID">
            <td><masterGamePopover :master-game="update.masterGame"></masterGamePopover></td>
            <td class="select-cell">
              <b-button variant="danger" size="sm" :disabled="isBusy" @click="deleteUpdate(update)">Delete</b-button>
            </td>
          </tr>
        </tbody>
      </table>

      <h3>Score Updates</h3>
      <b-alert v-if="pendingUpdates.scoreUpdates.length === 0" variant="info" :model-value="true">No score updates pending.</b-alert>
      <table v-else class="table table-sm table-responsive-sm table-bordered table-striped">
        <thead>
          <tr class="bg-primary">
            <th scope="col" class="game-column">Game</th>
            <th scope="col">Old Score</th>
            <th scope="col">New Score</th>
            <th scope="col"></th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="update in pendingUpdates.scoreUpdates" :key="update.masterGameUpdateID">
            <td><masterGamePopover :master-game="update.masterGame"></masterGamePopover></td>
            <td>{{ formatScore(update.oldCriticScore, 1) }}</td>
            <td>{{ formatScore(update.newCriticScore, 1) }}</td>
            <td class="select-cell">
              <b-button variant="danger" size="sm" :disabled="isBusy" @click="deleteUpdate(update)">Delete</b-button>
            </td>
          </tr>
        </tbody>
      </table>

      <h3>Edits</h3>
      <b-alert v-if="pendingUpdates.edits.length === 0" variant="info" :model-value="true">No edits pending.</b-alert>
      <div v-else>
        <table class="table table-sm table-responsive-sm table-bordered table-striped">
          <thead>
            <tr class="bg-primary">
              <th scope="col" class="game-column">Game</th>
              <th scope="col">Changes</th>
              <th scope="col"></th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="update in pendingUpdates.edits" :key="update.masterGameUpdateID">
              <td><masterGamePopover :master-game="update.masterGame"></masterGamePopover></td>
              <td>
                <ul class="mb-0">
                  <li v-for="(change, index) in update.changes" :key="index">{{ change }}</li>
                </ul>
              </td>
              <td class="select-cell">
                <b-button variant="danger" size="sm" :disabled="isBusy" @click="deleteUpdate(update)">Delete</b-button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>
  </div>
</template>

<script lang="ts">
import { defineComponent } from 'vue';

import {
  factCheckerClient,
  type DeletePendingMasterGameUpdateRequest,
  type PendingMasterGameEditViewModel,
  type PendingMasterGameUpdatesViewModel,
  type PendingNewMasterGameViewModel,
  type PendingScoreUpdateViewModel
} from '@/api/clients';
import { getApiErrorMessage } from '@/utilities/apiError';
import { useConfirm } from '@/utilities/confirm';
import { formatScore } from '@/utilities/formatters';
import MasterGamePopover from '@/components/masterGamePopover.vue';

type PendingUpdate = PendingNewMasterGameViewModel | PendingScoreUpdateViewModel | PendingMasterGameEditViewModel;

interface PendingMasterGameUpdatesData {
  pendingUpdates: PendingMasterGameUpdatesViewModel | null;
  errorInfo: string;
  isBusy: boolean;
}

export default defineComponent({
  setup() {
    return { confirm: useConfirm() };
  },
  components: {
    MasterGamePopover
  },
  data(): PendingMasterGameUpdatesData {
    return {
      pendingUpdates: null,
      errorInfo: '',
      isBusy: false
    };
  },
  async created() {
    await this.fetchPendingUpdates();
  },
  methods: {
    formatScore,
    async fetchPendingUpdates(): Promise<void> {
      try {
        this.pendingUpdates = await factCheckerClient.pendingMasterGameUpdates();
      } catch (error) {
        this.errorInfo = getApiErrorMessage(error, 'Could not load the pending updates.');
      }
    },
    async deleteUpdate(update: PendingUpdate): Promise<void> {
      const confirmed = await this.confirm(`Delete the pending update for ${update.masterGame.gameName}? It will not go to Discord.`, {
        title: 'Delete Pending Update',
        okTitle: 'Delete',
        okVariant: 'danger',
        cancelTitle: 'Cancel'
      });
      if (!confirmed) {
        return;
      }

      const request: DeletePendingMasterGameUpdateRequest = { masterGameUpdateID: update.masterGameUpdateID };

      //The list is refreshed even after a failure: a 400 usually means the update went out while the page was open.
      this.errorInfo = '';
      this.isBusy = true;
      try {
        await factCheckerClient.deletePendingMasterGameUpdate(request);
      } catch (error) {
        this.errorInfo = getApiErrorMessage(error, 'Something went wrong when deleting the update.');
      } finally {
        await this.fetchPendingUpdates();
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
