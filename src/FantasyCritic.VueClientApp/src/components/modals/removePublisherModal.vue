<template>
  <b-modal id="remove-publisher-modal" ref="removePublisherModalRef" title="Remove a Publisher" @hidden="clearData">
    <b-alert v-if="errorInfo" variant="danger" :model-value="true">{{ errorInfo }}</b-alert>

    <div class="alert alert-info">Only a publisher with no player can be removed. To remove a player's publisher, use "Disconnect a Player" first.</div>

    <div v-if="playStarted" class="alert alert-warning">
      Removing a publisher affects the competitive balance of your league. If you remove the publisher, all of the games owned by it that haven't released yet become available for bidding. This should
      not be done lightly, as it is not reversible. A publisher with no player connected to it is already effectively "frozen", that player cannot participate any longer.
    </div>

    <div class="form-group">
      <label for="publisherToRemove" class="control-label">Publisher</label>
      <b-form-select id="publisherToRemove" v-model="publisherToRemove">
        <option v-for="publisher in removablePublishers" :key="publisher.publisherID" :value="publisher">{{ publisher.publisherName }}</option>
      </b-form-select>
    </div>

    <div v-if="publisherToRemove">
      <div class="alert alert-warning">If you are sure you want do this, please type "I acknowledge the implications for my league." below.</div>
      <input v-model="removeConfirmation" type="text" class="form-control input" />
    </div>

    <template #footer>
      <input type="submit" class="btn btn-danger" value="Remove Publisher" :disabled="!readyToRemove || isBusy" @click="removePublisher" />
    </template>
  </b-modal>
</template>

<script lang="ts">
import { defineComponent } from 'vue';
import { mapActions, mapState } from 'pinia';
import type { BModal } from 'bootstrap-vue-next';

import { leagueManagerClient, type PublisherViewModel, type RemovePublisherRequest } from '@/api/clients';
import { useLeagueStore } from '@/store/leagueStore';
import { getApiErrorMessage } from '@/utilities/apiError';

interface RemovePublisherModalData {
  publisherToRemove: PublisherViewModel | null;
  removeConfirmation: string;
  isBusy: boolean;
  errorInfo: string;
}

export default defineComponent({
  data(): RemovePublisherModalData {
    return {
      publisherToRemove: null,
      removeConfirmation: '',
      isBusy: false,
      errorInfo: ''
    };
  },
  computed: {
    ...mapState(useLeagueStore, ['requiredLeagueYear', 'playStarted']),
    removablePublishers(): PublisherViewModel[] {
      return this.requiredLeagueYear.publishers.filter((x) => !x.userID);
    },
    readyToRemove(): boolean {
      return !!this.publisherToRemove && this.removeConfirmation === 'I acknowledge the implications for my league.';
    }
  },
  methods: {
    ...mapActions(useLeagueStore, ['notifyAction']),
    async removePublisher(): Promise<void> {
      const publisher = this.publisherToRemove;
      if (!publisher) {
        return;
      }

      const request: RemovePublisherRequest = {
        publisherID: publisher.publisherID
      };

      this.isBusy = true;
      this.errorInfo = '';
      try {
        await leagueManagerClient.removePublisher(request);

        (this.$refs.removePublisherModalRef as InstanceType<typeof BModal> | undefined)?.hide();
        await this.notifyAction(`${publisher.publisherName} has been removed from the league.`);
      } catch (error) {
        this.errorInfo = getApiErrorMessage(error, 'Something went wrong when removing the publisher.');
      } finally {
        this.isBusy = false;
      }
    },
    clearData(): void {
      this.publisherToRemove = null;
      this.removeConfirmation = '';
      this.errorInfo = '';
    }
  }
});
</script>
