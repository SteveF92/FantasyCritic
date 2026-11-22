<template>
  <b-modal id="superDropGameForm" ref="superDropGameFormRef" size="lg" title="Super Drop a Game" no-footer @hidden="clearData">
    <div v-if="errorInfo" class="alert alert-danger" role="alert">
      {{ errorInfo }}
    </div>
    <div class="alert alert-info">
      Super drops work differently from normal drops. You can drop any game you wish, and drops happen immediately, rather than having to wait until Saturday.
      <br />
      <br />
      <template v-if="requiredUserPublisher.superDropsAvailable > 1">You currently have {{ requiredUserPublisher.superDropsAvailable }} super drops.</template>
      <template v-if="requiredUserPublisher.superDropsAvailable === 1">You currently have 1 super drop.</template>
      <template v-if="!requiredUserPublisher.superDropsAvailable">You do not currently have any super drops.</template>
    </div>
    <form class="form-horizontal" no-footer @submit.prevent="dropGame">
      <div class="form-group">
        <label for="gameToDrop" class="control-label">Game</label>
        <b-form-select v-model="gameToDrop">
          <option v-for="publisherGame in requiredUserPublisher.games" :key="publisherGame.publisherGameID" :value="publisherGame">
            {{ publisherGame.gameName }}
          </option>
        </b-form-select>
      </div>

      <div v-if="gameToDrop">
        <input type="submit" class="btn btn-danger full-width-button" value="Super Drop Game" :disabled="isBusy" />
      </div>

      <div v-if="dropResult && !dropResult.success" class="alert alert-danger bid-error">
        <h3 class="alert-heading">Error!</h3>
        <ul>
          <li v-for="error in dropResult.errors" :key="error">{{ error }}</li>
        </ul>
      </div>
    </form>
  </b-modal>
</template>

<script lang="ts">
import { defineComponent } from 'vue';
import { mapActions, mapState } from 'pinia';
import type { BModal } from 'bootstrap-vue-next';

import { leagueClient, type DropGameRequestRequest, type DropGameResultViewModel, type PublisherGameViewModel } from '@/api/clients';
import { useLeagueStore } from '@/store/leagueStore';
import { getApiErrorMessage } from '@/utilities/apiError';

interface SuperDropGameFormData {
  dropResult: DropGameResultViewModel | null;
  gameToDrop: PublisherGameViewModel | null;
  isBusy: boolean;
  errorInfo: string;
}

export default defineComponent({
  data(): SuperDropGameFormData {
    return {
      dropResult: null,
      gameToDrop: null,
      isBusy: false,
      errorInfo: ''
    };
  },
  computed: {
    ...mapState(useLeagueStore, ['requiredUserPublisher'])
  },
  methods: {
    ...mapActions(useLeagueStore, ['notifyAction']),
    async dropGame(): Promise<void> {
      if (!this.gameToDrop) {
        throw new Error('Attempted to drop a game with none selected.');
      }

      const request: DropGameRequestRequest = {
        publisherID: this.requiredUserPublisher.publisherID,
        publisherGameID: this.gameToDrop.publisherGameID
      };

      this.isBusy = true;
      try {
        this.dropResult = await leagueClient.useSuperDrop(request);
        if (!this.dropResult.success) {
          return;
        }

        await this.notifyAction(this.gameToDrop.gameName + ' has been super dropped.');
        (this.$refs.superDropGameFormRef as InstanceType<typeof BModal> | undefined)?.hide();
        this.clearData();
      } catch (error) {
        this.errorInfo = getApiErrorMessage(error, 'Failed to use super drop.');
      } finally {
        this.isBusy = false;
      }
    },
    clearData(): void {
      this.dropResult = null;
      this.gameToDrop = null;
    }
  }
});
</script>
