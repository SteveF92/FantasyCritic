<template>
  <b-modal id="current-drops-form" title="My Pending Drop Requests" @hidden="clearData">
    <b-alert v-if="errorInfo" variant="danger" :model-value="true">{{ errorInfo }}</b-alert>

    <table class="table table-sm table-bordered table-striped">
      <thead>
        <tr class="bg-primary">
          <th scope="col" class="game-column">Game</th>
          <th scope="col">Cancel</th>
        </tr>
      </thead>
      <tbody>
        <tr v-for="drop in currentDrops" :key="drop.dropRequestID">
          <td>{{ drop.masterGame.gameName }}</td>
          <td class="select-cell">
            <b-button variant="danger" size="sm" @click="confirmCancelDrop(drop)">Cancel</b-button>
          </td>
        </tr>
      </tbody>
    </table>
    <b-modal id="confirm-cancel-drop-modal" ref="confirmCancelDropModalRef" @ok="cancelDrop">
      <!-- prettier-ignore -->
      <p>
        Are you sure you want to cancel your drop for
        <strong>{{ dropToCancel?.masterGame.gameName }}</strong>?
      </p>
    </b-modal>
  </b-modal>
</template>

<script lang="ts">
import { defineComponent } from 'vue';
import { mapActions, mapState } from 'pinia';
import type { BModal } from 'bootstrap-vue-next';

import { leagueClient, type DropGameRequestDeleteRequest, type DropGameRequestViewModel } from '@/api/clients';
import { useLeagueStore } from '@/store/leagueStore';
import { getApiErrorMessage } from '@/utilities/apiError';

interface CurrentDropsFormData {
  dropToCancel: DropGameRequestViewModel | null;
  errorInfo: string;
  isBusy: boolean;
}

export default defineComponent({
  computed: {
    ...mapState(useLeagueStore, ['requiredUserPublisher', 'currentDrops'])
  },
  data(): CurrentDropsFormData {
    return {
      dropToCancel: null,
      errorInfo: '',
      isBusy: false
    };
  },
  methods: {
    ...mapActions(useLeagueStore, ['notifyAction']),
    confirmCancelDrop(drop: DropGameRequestViewModel): void {
      this.errorInfo = '';
      this.dropToCancel = drop;
      (this.$refs.confirmCancelDropModalRef as InstanceType<typeof BModal> | undefined)?.show();
    },
    async cancelDrop(): Promise<void> {
      if (!this.dropToCancel) {
        throw new Error('Attempted to delete a drop with none selected.');
      }

      const request: DropGameRequestDeleteRequest = {
        publisherID: this.requiredUserPublisher.publisherID,
        dropRequestID: this.dropToCancel.dropRequestID
      };

      this.isBusy = true;
      try {
        await leagueClient.deleteDropRequest(request);
        await this.notifyAction('Drop Request for ' + this.dropToCancel.masterGame.gameName + ' was cancelled.');
        this.clearData();
      } catch (error) {
        this.errorInfo = getApiErrorMessage(error, 'Failed to cancel drop request.');
      } finally {
        this.isBusy = false;
      }
    },
    clearData() {
      this.dropToCancel = null;
      this.errorInfo = '';
      this.isBusy = false;
    }
  }
});
</script>
