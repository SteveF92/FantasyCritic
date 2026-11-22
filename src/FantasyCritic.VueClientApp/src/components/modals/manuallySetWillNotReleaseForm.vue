<template>
  <b-modal id="manually-set-will-not-release-form" ref="manuallySetWillNotReleaseFormRef" title="Manually Set Will Not Release" no-footer @hidden="clearData">
    <b-alert v-if="errorInfo" variant="danger" :model-value="true">{{ errorInfo }}</b-alert>
    <div class="alert alert-info">This form can be used when your league is extremely confident that a player's game will not release this year, even though it is not confirmed.</div>
    <div class="form-group">
      <label for="willNotReleasePublisher" class="control-label">Publisher</label>
      <b-form-select id="willNotReleasePublisher" v-model="manuallySetWillNotReleasePublisher">
        <option v-for="publisher in requiredLeagueYear.publishers" :key="publisher.publisherID" :value="publisher">
          {{ publisher.publisherName }}
        </option>
      </b-form-select>
      <div v-if="manuallySetWillNotReleasePublisher">
        <label for="willNotReleaseGame" class="control-label">Game</label>
        <b-form-select id="willNotReleaseGame" v-model="manuallySetWillNotReleaseGame">
          <option v-for="publisherGame in manuallySetWillNotReleasePublisher.games" :key="publisherGame.publisherGameID" :value="publisherGame">
            {{ publisherGame.gameName }}
          </option>
        </b-form-select>
      </div>
      <br />
      <div v-if="manuallySetWillNotReleaseGame">
        <div v-if="manuallySetWillNotReleaseGame.manualWillNotRelease">
          <b-button variant="primary" class="full-width-button" :disabled="isBusy" @click="manuallySetWillNotRelease(false)">Clear manual 'Will not Release'</b-button>
        </div>
        <div v-if="!manuallySetWillNotReleaseGame.manualWillNotRelease">
          <b-button variant="primary" class="full-width-button" :disabled="isBusy" @click="manuallySetWillNotRelease(true)">Set game as 'Will not Release'</b-button>
        </div>
      </div>
    </div>
  </b-modal>
</template>

<script lang="ts">
import { defineComponent } from 'vue';
import { mapActions, mapState } from 'pinia';
import type { BModal } from 'bootstrap-vue-next';

import { leagueManagerClient, type ManualPublisherGameWillNotReleaseRequest, type PublisherGameViewModel, type PublisherViewModel } from '@/api/clients';
import { useLeagueStore } from '@/store/leagueStore';
import { getApiErrorMessage } from '@/utilities/apiError';

interface ManuallySetWillNotReleaseFormData {
  manuallySetWillNotReleasePublisher: PublisherViewModel | null;
  manuallySetWillNotReleaseGame: PublisherGameViewModel | null;
  isBusy: boolean;
  errorInfo: string;
}

export default defineComponent({
  computed: {
    ...mapState(useLeagueStore, ['requiredLeagueYear'])
  },
  data(): ManuallySetWillNotReleaseFormData {
    return {
      manuallySetWillNotReleasePublisher: null,
      manuallySetWillNotReleaseGame: null,
      isBusy: false,
      errorInfo: ''
    };
  },
  methods: {
    ...mapActions(useLeagueStore, ['notifyAction']),
    async manuallySetWillNotRelease(willNotRelease: boolean): Promise<void> {
      if (!this.manuallySetWillNotReleasePublisher || !this.manuallySetWillNotReleaseGame) {
        return;
      }

      const request: ManualPublisherGameWillNotReleaseRequest = {
        publisherGameID: this.manuallySetWillNotReleaseGame.publisherGameID,
        publisherID: this.manuallySetWillNotReleasePublisher.publisherID,
        willNotRelease: willNotRelease
      };

      this.isBusy = true;
      this.errorInfo = '';
      try {
        await leagueManagerClient.manuallySetWillNotRelease(request);

        (this.$refs.manuallySetWillNotReleaseFormRef as InstanceType<typeof BModal> | undefined)?.hide();
        await this.notifyAction('Will not release status updated.');
      } catch (error) {
        this.errorInfo = getApiErrorMessage(error, "Something went wrong when setting 'Will not Release'.");
      } finally {
        this.isBusy = false;
      }
    },
    clearData(): void {
      this.manuallySetWillNotReleasePublisher = null;
      this.manuallySetWillNotReleaseGame = null;
      this.errorInfo = '';
    }
  }
});
</script>
