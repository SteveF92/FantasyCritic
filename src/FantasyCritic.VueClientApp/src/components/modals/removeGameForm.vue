<template>
  <b-modal id="remove-game-form" ref="removeGameFormRef" size="lg" title="Remove Publisher Game" no-footer @hidden="clearData">
    <b-alert v-if="errorInfo" variant="danger" :model-value="true">{{ errorInfo }}</b-alert>
    <div class="alert alert-warning">
      Warning! This feature is intended to fix mistakes and other exceptional circumstances. In general, managers should not be removing games from player's rosters.
    </div>
    <div class="form-group">
      <label for="removeGamePublisher" class="control-label">Publisher</label>
      <b-form-select id="removeGamePublisher" v-model="removeGamePublisher">
        <option v-for="publisher in requiredLeagueYear.publishers" :key="publisher.publisherID" :value="publisher">
          {{ publisher.publisherName }}
        </option>
      </b-form-select>
      <div v-if="removeGamePublisher">
        <label for="removeGame" class="control-label">Game</label>
        <b-form-select id="removeGame" v-model="removeGame">
          <option v-for="publisherGame in removeGamePublisher.games" :key="publisherGame.publisherGameID" :value="publisherGame">
            {{ formatGameOption(publisherGame) }}
          </option>
        </b-form-select>
      </div>
    </div>

    <div v-if="removeGame">
      <b-button variant="primary" class="full-width-button" :disabled="isBusy" @click="removePublisherGame">Remove Game</b-button>
    </div>
  </b-modal>
</template>

<script lang="ts">
import { defineComponent } from 'vue';
import { mapActions, mapState } from 'pinia';
import type { BModal } from 'bootstrap-vue-next';

import { leagueManagerClient, type GameRemoveRequest, type PublisherGameViewModel, type PublisherViewModel } from '@/api/clients';
import { useLeagueStore } from '@/store/leagueStore';
import { getApiErrorMessage } from '@/utilities/apiError';

interface RemoveGameFormData {
  removeGamePublisher: PublisherViewModel | null;
  removeGame: PublisherGameViewModel | null;
  isBusy: boolean;
  errorInfo: string;
}

export default defineComponent({
  computed: {
    ...mapState(useLeagueStore, ['requiredLeagueYear'])
  },
  data(): RemoveGameFormData {
    return {
      removeGamePublisher: null,
      removeGame: null,
      isBusy: false,
      errorInfo: ''
    };
  },
  methods: {
    ...mapActions(useLeagueStore, ['notifyAction']),
    formatGameOption(publisherGame: PublisherGameViewModel): string {
      return publisherGame.counterPick ? `${publisherGame.gameName} (Counter Pick)` : publisherGame.gameName;
    },
    async removePublisherGame(): Promise<void> {
      const publisher = this.removeGamePublisher;
      const game = this.removeGame;
      if (!publisher || !game) {
        return;
      }

      const request: GameRemoveRequest = {
        publisherGameID: game.publisherGameID,
        publisherID: publisher.publisherID
      };

      this.isBusy = true;
      this.errorInfo = '';
      try {
        await leagueManagerClient.removePublisherGame(request);

        (this.$refs.removeGameFormRef as InstanceType<typeof BModal> | undefined)?.hide();
        await this.notifyAction(game.gameName + ' removed from ' + publisher.publisherName);
      } catch (error) {
        this.errorInfo = getApiErrorMessage(error, 'Something went wrong when removing the game.');
      } finally {
        this.isBusy = false;
      }
    },
    clearData(): void {
      this.removeGamePublisher = null;
      this.removeGame = null;
      this.errorInfo = '';
    }
  }
});
</script>
