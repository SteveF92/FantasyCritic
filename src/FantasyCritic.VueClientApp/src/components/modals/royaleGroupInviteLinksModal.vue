<template>
  <b-modal id="royaleGroupInviteLinksModal" ref="royaleGroupInviteLinksRef" title="Invite Links" @show="fetchInviteLinks" no-footer>
    <b-alert v-if="errorInfo" variant="danger" :model-value="true">{{ errorInfo }}</b-alert>
    <label>Send one of these links to anyone you want to invite to the group.</label>
    <div v-for="link in activeLinks" :key="link.inviteID" class="invite-link">
      <input type="text" class="form-control input" :value="inviteLinkUrl(link.inviteCode)" readonly />
      <b-button variant="info" size="sm" @click="copyInviteLink(link)">Copy</b-button>
      <b-button variant="danger" size="sm" @click="deactivateLink(link.inviteID)">Deactivate</b-button>
    </div>
    <div class="create-button">
      <b-button variant="primary" size="sm" :disabled="activeLinks.length >= 2" @click="createInviteLink">Create Invite Link</b-button>
    </div>
  </b-modal>
</template>
<script lang="ts">
import { defineComponent } from 'vue';

import { royaleGroupClient, type RoyaleGroupInviteLinkViewModel } from '@/api/clients';
import { getApiErrorMessage } from '@/utilities/apiError';
import { makeToast } from '@/utilities/toast';

interface RoyaleGroupInviteLinksModalData {
  inviteLinks: RoyaleGroupInviteLinkViewModel[];
  errorInfo: string;
}

export default defineComponent({
  props: {
    groupid: { type: String, required: true }
  },
  data(): RoyaleGroupInviteLinksModalData {
    return {
      inviteLinks: [],
      errorInfo: ''
    };
  },
  computed: {
    activeLinks(): RoyaleGroupInviteLinkViewModel[] {
      return this.inviteLinks.filter((l) => l.active);
    }
  },
  methods: {
    async fetchInviteLinks(): Promise<void> {
      this.errorInfo = '';
      try {
        this.inviteLinks = await royaleGroupClient.getGroupInviteLinks(this.groupid);
      } catch (error) {
        this.inviteLinks = [];
        this.errorInfo = getApiErrorMessage(error, 'Failed to load invite links.');
      }
    },
    inviteLinkUrl(inviteCode: string): string {
      return `${window.location.origin}/royaleGroup/${this.groupid}?inviteCode=${inviteCode}`;
    },
    async copyInviteLink(link: RoyaleGroupInviteLinkViewModel): Promise<void> {
      await navigator.clipboard.writeText(this.inviteLinkUrl(link.inviteCode));
      makeToast('Invite link copied to clipboard!', 'success');
    },
    async createInviteLink(): Promise<void> {
      try {
        await royaleGroupClient.createGroupInviteLink(this.groupid);
        await this.fetchInviteLinks();
      } catch (error) {
        this.errorInfo = getApiErrorMessage(error, 'Failed to create invite link.');
      }
    },
    async deactivateLink(inviteID: string): Promise<void> {
      try {
        await royaleGroupClient.deactivateGroupInviteLink(inviteID);
        await this.fetchInviteLinks();
      } catch (error) {
        this.errorInfo = getApiErrorMessage(error, 'Failed to deactivate invite link.');
      }
    }
  }
});
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
