<template>
  <b-modal id="invitePlayers" ref="invitePlayersRef" title="Invite Players" hide-footer @hidden="clearData" @show="fetchInviteLinks">
    <div v-show="errorInfo" class="alert alert-danger">
      {{ errorInfo }}
    </div>
    <div>
      <div v-for="inviteLink in inviteLinks" :key="inviteLink.inviteID" class="invite-link">
        <input type="text" class="form-control input" :value="inviteLink.fullInviteLink" readonly />
        <b-button v-clipboard:copy="inviteLink.fullInviteLink" v-clipboard:success="inviteLinkCopied" variant="info" size="sm">Copy</b-button>
        <b-button variant="danger" size="sm" @click="deleteInviteLink(inviteLink)">Delete</b-button>
      </div>
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
      errorInfo: ''
    };
  },
  methods: {
    async createInviteLink() {
      var model = {
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
        leagueID: this.league.leagueID,
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
    clearData() {
      this.inviteLinks = null;
    }
  }
};
</script>
<style scoped>
.email-form {
  margin-bottom: 10px;
}
.text-black {
  color: black !important;
}
.display-number-label {
  font-size: 35px;
  margin-right: 3px;
}
.invite-link {
  display: flex;
}
</style>
