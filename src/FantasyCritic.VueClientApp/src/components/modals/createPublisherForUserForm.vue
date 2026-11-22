<template>
  <b-modal id="create-publisher-for-user-form" ref="createPublisherForUserFormRef" title="Create Publisher For User" @hidden="clearData">
    <b-alert v-if="errorInfo" variant="danger" :model-value="true">{{ errorInfo }}</b-alert>
    <div class="alert alert-warning">It is better that the players in your league create their own publishers, but if they cannot, you can create one for them here. They can rename it later.</div>

    <div class="form-group">
      <label for="playerToCreatePublisherFor" class="control-label">Player</label>
      <b-form-select id="playerToCreatePublisherFor" v-model="playerToCreatePublisherFor">
        <option v-for="player in playersWithoutPublishers" :key="player.userID" :value="player">
          {{ player.displayName }}
        </option>
      </b-form-select>
    </div>

    <div class="form-horizontal">
      <div class="form-group">
        <label for="publisherName" class="control-label">Publisher Name</label>
        <input id="publisherName" v-model="publisherName" name="publisherName" type="text" class="form-control input" />
      </div>
    </div>
    <template #footer>
      <input type="submit" class="btn btn-primary" value="Create Publisher" :disabled="!playerToCreatePublisherFor || !publisherName.trim() || isBusy" @click="createPublisher" />
    </template>
  </b-modal>
</template>

<script lang="ts">
import { defineComponent } from 'vue';
import { mapActions, mapState } from 'pinia';
import type { BModal } from 'bootstrap-vue-next';

import { leagueManagerClient, type CreatePublisherForUserRequest, type PlayerViewModel } from '@/api/clients';
import { useLeagueStore } from '@/store/leagueStore';
import { getApiErrorMessage } from '@/utilities/apiError';

interface CreatePublisherForUserFormData {
  playerToCreatePublisherFor: PlayerViewModel | null;
  publisherName: string;
  isBusy: boolean;
  errorInfo: string;
}

export default defineComponent({
  data(): CreatePublisherForUserFormData {
    return {
      playerToCreatePublisherFor: null,
      publisherName: '',
      isBusy: false,
      errorInfo: ''
    };
  },
  computed: {
    ...mapState(useLeagueStore, ['requiredLeagueYear']),
    playersWithoutPublishers(): PlayerViewModel[] {
      const userIDsWithPublishers = this.requiredLeagueYear.publishers.map((x) => x.userID);
      return this.requiredLeagueYear.players.flatMap((x) => (x.user && !userIDsWithPublishers.includes(x.user.userID) ? [x.user] : []));
    }
  },
  methods: {
    ...mapActions(useLeagueStore, ['notifyAction']),
    async createPublisher(): Promise<void> {
      if (!this.playerToCreatePublisherFor) {
        return;
      }

      const request: CreatePublisherForUserRequest = {
        leagueID: this.requiredLeagueYear.league.leagueID,
        year: this.requiredLeagueYear.year,
        userID: this.playerToCreatePublisherFor.userID,
        publisherName: this.publisherName.trim()
      };

      this.isBusy = true;
      this.errorInfo = '';
      try {
        await leagueManagerClient.createPublisherForUser(request);

        (this.$refs.createPublisherForUserFormRef as InstanceType<typeof BModal> | undefined)?.hide();
        await this.notifyAction('Publisher ' + request.publisherName + ' has been created.');
      } catch (error) {
        this.errorInfo = getApiErrorMessage(error, 'Something went wrong when creating the publisher.');
      } finally {
        this.isBusy = false;
      }
    },
    clearData(): void {
      this.publisherName = '';
      this.playerToCreatePublisherFor = null;
      this.errorInfo = '';
    }
  }
});
</script>
