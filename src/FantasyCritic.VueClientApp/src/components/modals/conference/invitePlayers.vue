<template>
  <b-modal id="invitePlayers" ref="invitePlayersRef" title="Invite/Remove Players" no-footer size="lg" @hidden="clearData" @show="fetchInviteLinks">
    <b-alert v-if="errorInfo" variant="danger" :model-value="true">{{ errorInfo }}</b-alert>
    <div>
      <h3 class="text-black">Invite Links</h3>
      <label>To invite players to your conference, create a link, and send it to anyone you want to invite.</label>
      <div v-for="inviteLink in inviteLinks" :key="inviteLink.inviteID" class="invite-link">
        <input type="text" class="form-control input" :value="inviteLink.fullInviteLink" readonly aria-label="Invite link" />
        <b-button variant="info" size="sm" @click="copyInviteLink(inviteLink)">Copy</b-button>
        <b-button variant="danger" size="sm" :disabled="isBusy" @click="deleteInviteLink(inviteLink)">Delete</b-button>
      </div>
      <br />
      <b-button variant="primary" size="sm" :disabled="isBusy" @click="createInviteLink()">Create Invite Link</b-button>
    </div>
    <hr />
    <h3 class="text-black">Remove Players</h3>
    <div>
      <b-table :items="requiredConference.players" :fields="conferencePlayerFields" bordered small responsive striped>
        <template #cell(removePlayer)="data">
          <b-button size="sm" variant="danger" :disabled="isBusy" @click="removePlayer(data.item.userID)">Remove Player</b-button>
        </template>
      </b-table>
    </div>
  </b-modal>
</template>

<script lang="ts">
import { defineComponent } from 'vue';
import { mapActions, mapState } from 'pinia';
import type { TableField } from 'bootstrap-vue-next';

import {
  conferenceClient,
  type ConferenceInviteLinkViewModel,
  type ConferencePlayerViewModel,
  type CreateConferenceInviteLinkRequest,
  type DeleteConferenceInviteLinkRequest,
  type RemovePlayerFromConferenceRequest
} from '@/api/clients';
import { useConferenceStore } from '@/store/conferenceStore';
import { getApiErrorMessage } from '@/utilities/apiError';
import { makeToast } from '@/utilities/toast';

interface InvitePlayersData {
  inviteLinks: ConferenceInviteLinkViewModel[];
  isBusy: boolean;
  errorInfo: string;
}

export default defineComponent({
  setup() {
    const conferencePlayerFields: TableField<ConferencePlayerViewModel>[] = [
      { key: 'displayName', label: 'Username', thClass: 'bg-primary' },
      { key: 'leaguesIn.length', accessor: (player) => player.leaguesIn.length, label: 'Leagues In', thClass: 'bg-primary' },
      { key: 'leaguesManaging.length', accessor: (player) => player.leaguesManaging.length, label: 'Leagues Managing', thClass: 'bg-primary' },
      { key: 'removePlayer', label: '', thClass: 'bg-primary' }
    ];
    return { conferencePlayerFields };
  },
  computed: {
    ...mapState(useConferenceStore, ['requiredConference'])
  },
  data(): InvitePlayersData {
    return {
      inviteLinks: [],
      isBusy: false,
      errorInfo: ''
    };
  },
  methods: {
    ...mapActions(useConferenceStore, ['refreshConferenceYear']),
    async createInviteLink(): Promise<void> {
      const request: CreateConferenceInviteLinkRequest = {
        conferenceID: this.requiredConference.conferenceID
      };

      this.isBusy = true;
      this.errorInfo = '';
      try {
        await conferenceClient.createInviteLink(request);
        await this.fetchInviteLinks();
      } catch (error) {
        this.errorInfo = getApiErrorMessage(error, 'Something went wrong when creating the invite link.');
      } finally {
        this.isBusy = false;
      }
    },
    async copyInviteLink(inviteLink: ConferenceInviteLinkViewModel): Promise<void> {
      await navigator.clipboard.writeText(inviteLink.fullInviteLink);
      makeToast('Invite Link copied to clipboard.');
    },
    async deleteInviteLink(inviteLink: ConferenceInviteLinkViewModel): Promise<void> {
      const request: DeleteConferenceInviteLinkRequest = {
        conferenceID: this.requiredConference.conferenceID,
        inviteID: inviteLink.inviteID
      };

      this.isBusy = true;
      this.errorInfo = '';
      try {
        await conferenceClient.deleteInviteLink(request);
        await this.fetchInviteLinks();
      } catch (error) {
        this.errorInfo = getApiErrorMessage(error, 'Something went wrong when deleting the invite link.');
      } finally {
        this.isBusy = false;
      }
    },
    async fetchInviteLinks(): Promise<void> {
      try {
        this.inviteLinks = await conferenceClient.inviteLinks(this.requiredConference.conferenceID);
      } catch (error) {
        this.errorInfo = getApiErrorMessage(error, 'Failed to load the invite links.');
      }
    },
    async removePlayer(userID: string): Promise<void> {
      const request: RemovePlayerFromConferenceRequest = {
        conferenceID: this.requiredConference.conferenceID,
        userID
      };

      this.isBusy = true;
      this.errorInfo = '';
      try {
        await conferenceClient.removePlayerFromConference(request);
        await this.refreshConferenceYear();
      } catch (error) {
        this.errorInfo = getApiErrorMessage(error, 'Something went wrong when removing the player.');
      } finally {
        this.isBusy = false;
      }
    },
    clearData(): void {
      this.inviteLinks = [];
      this.errorInfo = '';
    }
  }
});
</script>

<style scoped>
.invite-link {
  display: flex;
}
</style>
