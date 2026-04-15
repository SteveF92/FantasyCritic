<template>
  <b-modal id="royaleGroupInviteLinksModal" ref="royaleGroupInviteLinksRef" title="Invite Links" @show="fetchInviteLinks" hide-footer>
    <div v-if="errorInfo" class="alert alert-danger">{{ errorInfo }}</div>
    <label>Send one of these links to anyone you want to invite to the group.</label>
    <div v-for="link in activeLinks" :key="link.inviteID" class="invite-link">
      <input type="text" class="form-control input" :value="inviteLinkUrl(link.inviteCode)" readonly />
      <b-button variant="info" size="sm" v-clipboard:copy="inviteLinkUrl(link.inviteCode)" v-clipboard:success="linkCopied">Copy</b-button>
      <b-button variant="danger" size="sm" @click="deactivateLink(link.inviteID)">Deactivate</b-button>
    </div>
    <div class="create-button">
      <b-button variant="primary" size="sm" :disabled="activeLinks.length >= 2" @click="createInviteLink">Create Invite Link</b-button>
    </div>
  </b-modal>
</template>
<script>
import axios from 'axios';

export default {
  props: {
    groupid: { type: String, required: true }
  },
  data() {
    return {
      inviteLinks: [],
      errorInfo: ''
    };
  },
  computed: {
    activeLinks() {
      return this.inviteLinks.filter((l) => l.active);
    }
  },
  methods: {
    async fetchInviteLinks() {
      this.errorInfo = '';
      try {
        const response = await axios.get(`/api/RoyaleGroup/GetGroupInviteLinks/${this.groupid}`);
        this.inviteLinks = response.data;
      } catch {
        this.inviteLinks = [];
      }
    },
    inviteLinkUrl(inviteCode) {
      return `${window.location.origin}/royaleGroup/${this.groupid}?inviteCode=${inviteCode}`;
    },
    linkCopied() {
      this.$bvToast.toast('Invite link copied to clipboard!', { variant: 'success', solid: true, noCloseButton: true });
    },
    async createInviteLink() {
      try {
        await axios.post(`/api/RoyaleGroup/CreateGroupInviteLink/${this.groupid}`);
        await this.fetchInviteLinks();
      } catch (error) {
        this.errorInfo = error.response?.data || 'Failed to create invite link.';
      }
    },
    async deactivateLink(inviteID) {
      try {
        await axios.post(`/api/RoyaleGroup/DeactivateGroupInviteLink/${inviteID}`);
        await this.fetchInviteLinks();
      } catch (error) {
        this.errorInfo = error.response?.data || 'Failed to deactivate invite link.';
      }
    }
  }
};
</script>
<style scoped>
.invite-link {
  display: flex;
  gap: 8px;
  margin-bottom: 8px;
}
.create-button {
  margin-top: 12px;
}
</style>
