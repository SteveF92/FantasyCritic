<template>
  <b-modal id="reassign-publisher-modal" title="Reassign a Publisher" @hidden="clearData">
    <b-alert v-if="errorInfo" variant="danger" :model-value="true">{{ errorInfo }}</b-alert>

    <div class="alert alert-warning">
      This feature will allow you to replace the user of one of the leagues publishers with a different player who is currently inactive (does not have a publisher for this year). If you need to
      invite a new player, you can do so by clicking "Invite a Player".
    </div>
    <div class="alert alert-warning">
      If you use this option, the following will happen:
      <ol>
        <li>The new player will take ownership of the publisher selected.</li>
        <li>The old player will be marked as inactive in the current year.</li>
      </ol>
    </div>
    <div class="alert alert-warning">You can also use this to assign a player to a publisher that has no player attached to it, which you can do by using the "Disconnect Player" option.</div>
    <div class="form-horizontal">
      <div class="form-group">
        <label for="publisherToReassign" class="control-label">Publisher to Reassign</label>
        <b-form-select id="publisherToReassign" v-model="publisherToReassign">
          <option v-for="publisher in requiredLeagueYear.publishers" :key="publisher.publisherID" :value="publisher">{{ publisher.publisherName }} ({{ publisher.playerName }})</option>
        </b-form-select>
      </div>

      <div v-if="inactiveUsers.length > 0" class="form-group">
        <label for="newUser" class="control-label">New User to Assign Publisher To</label>
        <b-form-select id="newUser" v-model="newUser">
          <option v-for="user in inactiveUsers" :key="user.userID" :value="user">
            {{ user.displayName }}
          </option>
        </b-form-select>
      </div>
      <div v-else class="alert alert-warning">There are no inactive players in this league. Invite one by going to 'Invite a Player'.</div>
    </div>

    <div v-if="readyToConfirm">
      <div class="alert alert-warning">If you are sure you want do this, please type "Reassign Publisher" below.</div>
      <input v-model="reassignConfirmation" type="text" class="form-control input" />
    </div>

    <template #footer>
      <input type="submit" class="btn btn-primary" value="Reassign Publisher" :disabled="!readyToReassign || isBusy" @click="reassignPublisher" />
    </template>
  </b-modal>
</template>

<script lang="ts">
import { defineComponent } from 'vue';
import { mapActions, mapState } from 'pinia';

import { leagueManagerClient, type PlayerViewModel, type PublisherViewModel, type ReassignPublisherRequest } from '@/api/clients';
import { useLeagueStore } from '@/store/leagueStore';
import { getApiErrorMessage } from '@/utilities/apiError';

interface ReassignPublisherModalData {
  publisherToReassign: PublisherViewModel | null;
  newUser: PlayerViewModel | null;
  reassignConfirmation: string;
  isBusy: boolean;
  errorInfo: string;
}

export default defineComponent({
  data(): ReassignPublisherModalData {
    return {
      publisherToReassign: null,
      newUser: null,
      reassignConfirmation: '',
      isBusy: false,
      errorInfo: ''
    };
  },
  computed: {
    ...mapState(useLeagueStore, ['requiredLeagueYear']),
    readyToConfirm(): boolean {
      return !!this.publisherToReassign && !!this.newUser;
    },
    readyToReassign(): boolean {
      return this.readyToConfirm && this.reassignConfirmation === 'Reassign Publisher';
    },
    inactiveUsers(): PlayerViewModel[] {
      const allPlayers = this.requiredLeagueYear.league.players ?? [];
      const activePlayerUserIDs = this.requiredLeagueYear.players.flatMap((x) => (x.user ? [x.user.userID] : []));
      return allPlayers.filter((x) => !activePlayerUserIDs.includes(x.userID));
    }
  },
  methods: {
    ...mapActions(useLeagueStore, ['notifyAction']),
    async reassignPublisher(): Promise<void> {
      if (!this.publisherToReassign || !this.newUser) {
        return;
      }

      const request: ReassignPublisherRequest = {
        leagueID: this.requiredLeagueYear.league.leagueID,
        year: this.requiredLeagueYear.year,
        publisherID: this.publisherToReassign.publisherID,
        newUserID: this.newUser.userID
      };

      this.isBusy = true;
      this.errorInfo = '';
      try {
        await leagueManagerClient.reassignPublisher(request);

        await this.notifyAction('Publisher has been reassigned.');
        this.clearData();
      } catch (error) {
        this.errorInfo = getApiErrorMessage(error, 'Something went wrong when reassigning the publisher.');
      } finally {
        this.isBusy = false;
      }
    },
    clearData(): void {
      this.publisherToReassign = null;
      this.newUser = null;
      this.reassignConfirmation = '';
      this.errorInfo = '';
    }
  }
});
</script>
