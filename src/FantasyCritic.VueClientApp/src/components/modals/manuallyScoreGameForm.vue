<template>
  <b-modal id="manually-score-game-form" ref="manuallyScoreGameFormRef" title="Manually Score Publisher Game" no-footer @hidden="clearData">
    <b-alert v-if="errorInfo" variant="danger" :model-value="true">{{ errorInfo }}</b-alert>
    <div class="alert alert-warning">
      Warning! This feature is intended to deal with exceptional circumstances. See the
      <a href="/faq#scoring" class="text-secondary" target="_blank">FAQ</a>
      page for more info.
    </div>
    <div class="form-group">
      <label for="manuallyScoreGamePublisher" class="control-label">Publisher</label>
      <b-form-select id="manuallyScoreGamePublisher" v-model="manuallyScoreGamePublisher">
        <option v-for="publisher in requiredLeagueYear.publishers" :key="publisher.publisherID" :value="publisher">
          {{ publisher.publisherName }}
        </option>
      </b-form-select>
      <div v-if="manuallyScoreGamePublisher">
        <label for="manuallyScoreGame" class="control-label">Game</label>
        <b-form-select id="manuallyScoreGame" v-model="manuallyScoreGame" @update:model-value="newGameSelected()">
          <option v-for="publisherGame in manuallyScoreGamePublisher.games" :key="publisherGame.publisherGameID" :value="publisherGame">
            {{ publisherGame.gameName }}
          </option>
        </b-form-select>
      </div>
      <div v-if="manuallyScoreGame">
        <div v-if="manuallyScoreGame.manualCriticScore" class="plain-check">
          <span>
            <label for="removeManualScore" class="plain-check-label">Remove Manual Score?</label>
            <input id="removeManualScore" v-model="removeManualScore" class="plain-check-input remove-manual-score-checkbox" type="checkbox" />
          </span>
        </div>
        <div v-if="!removeManualScore">
          <label for="manualScore" class="control-label">Score</label>
          <input id="manualScore" v-model="manualScore" name="manualScore" type="number" step="any" class="form-control input" />
        </div>
      </div>
    </div>

    <div v-if="manuallyScoreGame && (manualScore !== '' || removeManualScore)">
      <b-button variant="primary" class="full-width-button" :disabled="isBusy" @click="manuallyScorePublisherGame">{{ buttonText }}</b-button>
    </div>
  </b-modal>
</template>

<script lang="ts">
import { defineComponent } from 'vue';
import { mapActions, mapState } from 'pinia';
import type { BModal } from 'bootstrap-vue-next';

import { leagueManagerClient, type ManualPublisherGameScoreRequest, type PublisherGameViewModel, type PublisherViewModel, type RemoveManualPublisherGameScoreRequest } from '@/api/clients';
import { useLeagueStore } from '@/store/leagueStore';
import { getApiErrorMessage } from '@/utilities/apiError';

interface ManuallyScoreGameFormData {
  manuallyScoreGamePublisher: PublisherViewModel | null;
  manuallyScoreGame: PublisherGameViewModel | null;
  // A number input's v-model gives '' when the box is empty.
  manualScore: number | '';
  removeManualScore: boolean;
  isBusy: boolean;
  errorInfo: string;
}

export default defineComponent({
  data(): ManuallyScoreGameFormData {
    return {
      manuallyScoreGamePublisher: null,
      manuallyScoreGame: null,
      manualScore: '',
      removeManualScore: false,
      isBusy: false,
      errorInfo: ''
    };
  },
  computed: {
    ...mapState(useLeagueStore, ['requiredLeagueYear']),
    buttonText(): string {
      if (this.removeManualScore) {
        return 'Remove Manual Score';
      }

      return 'Set Manual Score';
    }
  },
  methods: {
    ...mapActions(useLeagueStore, ['notifyAction']),
    newGameSelected(): void {
      this.manualScore = this.manuallyScoreGame?.criticScore ?? '';
      this.removeManualScore = false;
      this.errorInfo = '';
    },
    async manuallyScorePublisherGame(): Promise<void> {
      const publisher = this.manuallyScoreGamePublisher;
      const game = this.manuallyScoreGame;
      if (!publisher || !game) {
        return;
      }

      this.isBusy = true;
      this.errorInfo = '';
      try {
        if (this.removeManualScore) {
          const request: RemoveManualPublisherGameScoreRequest = {
            publisherGameID: game.publisherGameID,
            publisherID: publisher.publisherID
          };
          await leagueManagerClient.removeManualPublisherGameScore(request);

          (this.$refs.manuallyScoreGameFormRef as InstanceType<typeof BModal> | undefined)?.hide();
          await this.notifyAction(game.gameName + "'s manual score was removed.");
          return;
        }

        if (this.manualScore === '') {
          return;
        }

        const request: ManualPublisherGameScoreRequest = {
          publisherGameID: game.publisherGameID,
          publisherID: publisher.publisherID,
          manualCriticScore: this.manualScore
        };
        await leagueManagerClient.manuallyScorePublisherGame(request);

        (this.$refs.manuallyScoreGameFormRef as InstanceType<typeof BModal> | undefined)?.hide();
        await this.notifyAction(game.gameName + ' was given a score of ' + request.manualCriticScore + '.');
      } catch (error) {
        this.errorInfo = getApiErrorMessage(error, 'Something went wrong when setting the manual score.');
      } finally {
        this.isBusy = false;
      }
    },
    clearData(): void {
      this.manuallyScoreGamePublisher = null;
      this.manuallyScoreGame = null;
      this.manualScore = '';
      this.removeManualScore = false;
      this.errorInfo = '';
    }
  }
});
</script>

<style scoped>
.remove-manual-score-checkbox {
  margin-left: 15px;
  margin-top: 8px;
}
</style>
