<template>
  <b-modal id="invitePlayers" ref="invitePlayersRef" title="Invite/Remove Players" hide-footer @hidden="clearData" @show="fetchInviteLinks" size="lg">
    <div v-show="errorInfo" class="alert alert-danger">
      {{ errorInfo }}
    </div>
    <div>
      <h3 class="text-black">Invite Links</h3>
      <label>To invite players to your conference, create a link, and send it to anyone you want to invite.</label>
      <div v-for="inviteLink in inviteLinks" :key="inviteLink.inviteID" class="invite-link">
        <input type="text" class="form-control input" :value="inviteLink.fullInviteLink" readonly />
        <b-button v-clipboard:copy="inviteLink.fullInviteLink" v-clipboard:success="inviteLinkCopied" variant="info" size="sm">Copy</b-button>
        <b-button variant="danger" size="sm" @click="deleteInviteLink(inviteLink)">Delete</b-button>
      </div>
      <br />
      <b-button variant="primary" size="sm" @click="createInviteLink()">Create Invite Link</b-button>
    </div>
    <hr />
    <h3 class="text-black">Remove Players</h3>
    <div>
      <b-table :items="conference.players" :fields="conferencePlayerFields" bordered small responsive striped>
        <template #cell(removePlayer)="data">
          <b-button size="sm" variant="danger" @click="removePlayer(data.item.userID)">Remove Player</b-button>
        </template>
      </b-table>
    </div>
  </b-modal>
</template>
<script>
import axios from 'axios';
import ConferenceMixin from '@/mixins/conferenceMixin.js';

export default {
  mixins: [ConferenceMixin],
  data() {
    return {
      inviteLinks: null,
      errorInfo: '',
      conferencePlayerFields: [
        { key: 'displayName', label: 'Username', thClass: 'bg-primary' },
        { key: 'numberOfLeaguesIn', label: 'Leagues In', thClass: 'bg-primary' },
        { key: 'numberOfLeaguesManaging', label: 'Leagues Managing', thClass: 'bg-primary' },
        { key: 'removePlayer', label: '', thClass: 'bg-primary' }
      ]
    };
  },
  methods: {
    async createInviteLink() {
      const model = {
        conferenceID: this.conference.conferenceID
      };

      try {
        await axios.post('/api/conference/CreateInviteLink', model);
        await this.fetchInviteLinks();
      } catch (error) {
        this.errorInfo = error.response.data;
      }
    },
    inviteLinkCopied() {
      this.makeToast('Invite Link copied to clipboard.');
    },
    async deleteInviteLink(inviteLink) {
      const model = {
        conferenceID: this.conference.conferenceID,
        inviteID: inviteLink.inviteID
      };

      try {
        await axios.post('/api/conference/DeleteInviteLink', model);
        await this.fetchInviteLinks();
      } catch (error) {
        this.errorInfo = error.response.data;
      }
    },
    async fetchInviteLinks() {
      try {
        const response = await axios.get('/api/conference/InviteLinks/' + this.conference.conferenceID);
        this.inviteLinks = response.data;
      } catch (error) {
        this.errorInfo = error.response.data;
      }
    },
    async removePlayer(userID) {
      const model = {
        conferenceID: this.conference.conferenceID,
        userID: userID
      };

      try {
        await axios.post('/api/conference/RemovePlayerFromConference', model);
        await this.refreshConferenceYear();
      } catch (error) {
        this.errorInfo = error.response.data;
      }
    },
    clearData() {
      this.inviteLinks = null;
    }
  }
};
</script>
<style scoped>
.invite-link {
  display: flex;
}
</style>
