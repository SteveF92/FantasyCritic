<template>
  <b-modal id="drop-game-form" ref="dropGameFormRef" size="lg" title="Drop a Game" no-footer @hidden="clearData">
    <b-alert v-if="errorInfo" variant="danger" :model-value="true">{{ errorInfo }}</b-alert>

    <p>
      You can use this form to request to drop a game.
      <br />
      Your league's settings determine how often you can do this.
      <br />
      Drop requests are processed on Saturday Nights. See the FAQ for more info.
    </p>
    <div class="drop-counts">
      <strong>Drops Remaining:</strong>
      <ul>
        <li v-if="requiredUserPublisher.willReleaseDroppableGames !== 0">
          Will Release: {{ dropStatus(requiredUserPublisher.willReleaseGamesDropped, requiredUserPublisher.willReleaseDroppableGames) }}
        </li>
        <li v-if="requiredUserPublisher.willNotReleaseDroppableGames !== 0">
          Will Not Release: {{ dropStatus(requiredUserPublisher.willNotReleaseGamesDropped, requiredUserPublisher.willNotReleaseDroppableGames) }}
        </li>
        <li v-if="requiredUserPublisher.unrestrictedReleaseStatusDroppableGames !== 0">
          Any Unreleased: {{ dropStatus(requiredUserPublisher.unrestrictedReleaseStatusGamesDropped, requiredUserPublisher.unrestrictedReleaseStatusDroppableGames) }}
        </li>
        <li v-if="requiredUserPublisher.superDropsAvailable > 0">Super Drops: {{ requiredUserPublisher.superDropsAvailable }}</li>
      </ul>
    </div>
    <form class="form-horizontal" @submit.prevent="dropGame">
      <div class="form-group">
        <label for="game-to-drop" class="control-label">Game</label>
        <b-form-select id="game-to-drop" v-model="gameToDrop">
          <option v-for="publisherGame in droppableGames" :key="publisherGame.publisherGameID" :value="publisherGame">
            {{ publisherGame.gameName }}
          </option>
        </b-form-select>
      </div>

      <div v-if="gameToDrop">
        <input type="submit" class="btn btn-danger full-width-button" value="Make Drop Game Request" :disabled="isBusy" />
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

interface DropGameFormData {
  dropResult: DropGameResultViewModel | null;
  gameToDrop: PublisherGameViewModel | null;
  isBusy: boolean;
  errorInfo: string;
}

export default defineComponent({
  data(): DropGameFormData {
    return {
      dropResult: null,
      gameToDrop: null,
      isBusy: false,
      errorInfo: ''
    };
  },
  computed: {
    ...mapState(useLeagueStore, ['requiredUserPublisher']),
    droppableGames(): PublisherGameViewModel[] {
      return this.requiredUserPublisher.games.filter((x) => !x.counterPick);
    }
  },
  methods: {
    ...mapActions(useLeagueStore, ['notifyAction']),
    dropStatus(dropped: number, droppable: number): string {
      if (droppable === -1) {
        return '\u221E';
      }
      return String(droppable - dropped);
    },
    async dropGame(): Promise<void> {
      if (!this.gameToDrop) {
        throw new Error('Attempted to drop a game with none selected.');
      }

      const request: DropGameRequestRequest = {
        publisherID: this.requiredUserPublisher.publisherID,
        publisherGameID: this.gameToDrop.publisherGameID
      };

      this.isBusy = true;
      this.dropResult = null;
      try {
        this.dropResult = await leagueClient.makeDropRequest(request);
        if (!this.dropResult.success) {
          return;
        }

        (this.$refs.dropGameFormRef as InstanceType<typeof BModal> | undefined)?.hide();
        await this.notifyAction('Drop Request for ' + this.gameToDrop.gameName + ' was made.');
      } catch (error) {
        this.errorInfo = getApiErrorMessage(error, 'Failed to place drop request.');
      } finally {
        this.isBusy = false;
      }
    },
    clearData(): void {
      this.dropResult = null;
      this.gameToDrop = null;
      this.errorInfo = '';
    }
  }
});
</script>
