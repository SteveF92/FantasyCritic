<template>
  <div class="col-md-10 offset-md-1 col-sm-12">
    <div>
      <h1>Pending Discord Game Updates</h1>
      <b-button variant="info" :to="{ name: 'adminConsole' }">Admin Console</b-button>
    </div>
    <hr />
    <p>These go out to Discord with the next cache refresh. Delete any that were queued by mistake.</p>
    <div v-if="errorResponse" class="alert alert-danger">{{ errorResponse }}</div>

    <div v-if="pendingUpdates">
      <h3>New Games</h3>
      <div v-if="pendingUpdates.newGames.length === 0" class="alert alert-info">No new games pending.</div>
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
      <div v-if="pendingUpdates.scoreUpdates.length === 0" class="alert alert-info">No score updates pending.</div>
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
            <td>{{ update.oldCriticScore | score(1) }}</td>
            <td>{{ update.newCriticScore | score(1) }}</td>
            <td class="select-cell">
              <b-button variant="danger" size="sm" :disabled="isBusy" @click="deleteUpdate(update)">Delete</b-button>
            </td>
          </tr>
        </tbody>
      </table>

      <h3>Edits</h3>
      <div v-if="pendingUpdates.edits.length === 0" class="alert alert-info">No edits pending.</div>
      <div v-else>
        <b-button variant="warning" size="sm" class="mb-2" :disabled="isBusy" @click="clearEdits">Clear All Edits</b-button>
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
<script>
import { ApiException, factCheckerClient } from '@/api/clients';
import MasterGamePopover from '@/components/masterGamePopover.vue';

export default {
  components: {
    MasterGamePopover
  },
  data() {
    return {
      pendingUpdates: null,
      errorResponse: null,
      isBusy: false
    };
  },
  async created() {
    await this.fetchPendingUpdates();
  },
  methods: {
    async fetchPendingUpdates() {
      try {
        this.pendingUpdates = await factCheckerClient.pendingMasterGameUpdates();
      } catch (error) {
        this.errorResponse = this.describeError(error);
      }
    },
    async deleteUpdate(update) {
      const confirmed = await this.$bvModal.msgBoxConfirm(`Delete the pending update for ${update.masterGame.gameName}? It will not go to Discord.`, {
        title: 'Delete Pending Update',
        okTitle: 'Delete',
        okVariant: 'danger',
        cancelTitle: 'Cancel'
      });
      if (!confirmed) {
        return;
      }

      await this.runAndRefresh(() => factCheckerClient.deletePendingMasterGameUpdate({ masterGameUpdateID: update.masterGameUpdateID }));
    },
    async clearEdits() {
      const confirmed = await this.$bvModal.msgBoxConfirm('Delete every pending edit? None of them will go to Discord.', {
        title: 'Clear All Edits',
        okTitle: 'Clear',
        okVariant: 'danger',
        cancelTitle: 'Cancel'
      });
      if (!confirmed) {
        return;
      }

      await this.runAndRefresh(() => factCheckerClient.clearMasterGameEditDiscordQueue());
    },
    //The list is refreshed even after a failure: a 400 usually means the update went out while the page was open.
    async runAndRefresh(call) {
      this.errorResponse = null;
      this.isBusy = true;
      try {
        await call();
      } catch (error) {
        this.errorResponse = this.describeError(error);
      } finally {
        await this.fetchPendingUpdates();
        this.isBusy = false;
      }
    },
    describeError(error) {
      if (ApiException.isApiException(error) && error.response) {
        return error.response;
      }

      return error.message || String(error);
    }
  }
};
</script>
<style scoped>
.select-cell {
  text-align: center;
}
</style>
