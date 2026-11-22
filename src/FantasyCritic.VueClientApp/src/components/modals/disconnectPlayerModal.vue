<template>
  <b-modal id="disconnect-player-modal" ref="disconnectPlayerModalRef" title="Disconnect a Player" @hidden="clearData">
    <b-alert v-if="errorInfo" variant="danger" :model-value="true">{{ errorInfo }}</b-alert>

    <div class="alert alert-warning">
      You can use this option to remove a player from the league.
      <ul>
        <li>The player is marked inactive this year and can no longer act for the publisher.</li>
        <li>The publisher, and any games it already contains, remains in place for the rest of the year, but it is effectively "frozen in time" as it no longer has a player attached to it.</li>
        <li>Any pending bids or drops the player placed are deleted, and any open trades they are involved in are automatically rejected.</li>
        <li>If the player had any games on their publisher's watch list, they will be cleared.</li>
        <li>If this is a multi draft league, the publisher will be skipped in any future drafts.</li>
      </ul>
      If you chose to, you can transfer the publisher to a new player using the "Reassign a Publisher" option.
    </div>

    <div class="form-group">
      <label for="publisherToDisconnect" class="control-label">Publisher</label>
      <b-form-select id="publisherToDisconnect" v-model="publisherToDisconnect">
        <option v-for="publisher in disconnectablePublishers" :key="publisher.publisherID" :value="publisher">{{ publisher.publisherName }} ({{ publisher.playerName }})</option>
      </b-form-select>
    </div>

    <template #footer>
      <input type="submit" class="btn btn-danger" value="Disconnect Player" :disabled="!publisherToDisconnect || isBusy" @click="disconnectPlayer" />
    </template>
  </b-modal>
</template>

<script lang="ts">
import { defineComponent } from 'vue';
import { mapActions, mapState } from 'pinia';
import type { BModal } from 'bootstrap-vue-next';

import { leagueManagerClient, type DisconnectPlayerRequest, type PublisherViewModel } from '@/api/clients';
import { useLeagueStore } from '@/store/leagueStore';
import { getApiErrorMessage } from '@/utilities/apiError';

interface DisconnectPlayerModalData {
  publisherToDisconnect: PublisherViewModel | null;
  isBusy: boolean;
  errorInfo: string;
}

export default defineComponent({
  data(): DisconnectPlayerModalData {
    return {
      publisherToDisconnect: null,
      isBusy: false,
      errorInfo: ''
    };
  },
  computed: {
    ...mapState(useLeagueStore, ['requiredLeagueYear']),
    disconnectablePublishers(): PublisherViewModel[] {
      const managerUserID = this.requiredLeagueYear.league.leagueManager.userID;
      return this.requiredLeagueYear.publishers.filter((x) => !!x.userID && x.userID !== managerUserID);
    }
  },
  methods: {
    ...mapActions(useLeagueStore, ['notifyAction']),
    async disconnectPlayer(): Promise<void> {
      const publisher = this.publisherToDisconnect;
      if (!publisher) {
        return;
      }

      const request: DisconnectPlayerRequest = {
        publisherID: publisher.publisherID
      };

      this.isBusy = true;
      this.errorInfo = '';
      try {
        await leagueManagerClient.disconnectPlayer(request);

        (this.$refs.disconnectPlayerModalRef as InstanceType<typeof BModal> | undefined)?.hide();
        await this.notifyAction(`${publisher.playerName} has been disconnected from ${publisher.publisherName}.`);
      } catch (error) {
        this.errorInfo = getApiErrorMessage(error, 'Something went wrong when disconnecting the player.');
      } finally {
        this.isBusy = false;
      }
    },
    clearData(): void {
      this.publisherToDisconnect = null;
      this.errorInfo = '';
    }
  }
});
</script>
