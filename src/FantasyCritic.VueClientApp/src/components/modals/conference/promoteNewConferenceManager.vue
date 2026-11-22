<template>
  <b-modal id="promoteNewConferenceManager" ref="promoteNewConferenceManagerRef" title="Promote New Conference Manager" no-footer @hidden="clearData">
    <b-alert v-if="errorInfo" variant="danger" :model-value="true">{{ errorInfo }}</b-alert>
    <div class="alert alert-warning">Warning! If you promote a new player to be Conference Manager, you will no longer be Conference Manager! There can only be one!</div>
    <div class="form-group">
      <label for="newConferenceManager" class="control-label">New Manager</label>
      <b-form-select id="newConferenceManager" v-model="newManager">
        <option v-for="user in players" :key="user.userID" :value="user">
          {{ user.displayName }}
        </option>
      </b-form-select>
      <br />
      <br />
      <b-button variant="danger" class="full-width-button" :disabled="!newManager || isBusy" @click="promoteNewManager">Transfer Conference Manager</b-button>
    </div>
  </b-modal>
</template>

<script lang="ts">
import { defineComponent } from 'vue';
import { mapActions, mapState } from 'pinia';
import type { BModal } from 'bootstrap-vue-next';

import { conferenceClient, type ConferencePlayerViewModel, type PromoteNewConferenceManagerRequest } from '@/api/clients';
import { useAuthStore } from '@/store/authStore';
import { useConferenceStore } from '@/store/conferenceStore';
import { getApiErrorMessage } from '@/utilities/apiError';

interface PromoteNewConferenceManagerData {
  newManager: ConferencePlayerViewModel | null;
  isBusy: boolean;
  errorInfo: string;
}

export default defineComponent({
  data(): PromoteNewConferenceManagerData {
    return {
      newManager: null,
      isBusy: false,
      errorInfo: ''
    };
  },
  computed: {
    ...mapState(useAuthStore, ['userInfo']),
    ...mapState(useConferenceStore, ['requiredConference']),
    players(): ConferencePlayerViewModel[] {
      return this.requiredConference.players.filter((x) => x.userID !== this.userInfo?.userID);
    }
  },
  methods: {
    ...mapActions(useConferenceStore, ['notifyAction']),
    async promoteNewManager(): Promise<void> {
      if (!this.newManager) {
        return;
      }

      const request: PromoteNewConferenceManagerRequest = {
        conferenceID: this.requiredConference.conferenceID,
        newManagerUserID: this.newManager.userID
      };

      this.isBusy = true;
      this.errorInfo = '';
      try {
        await conferenceClient.promoteNewConferenceManager(request);

        (this.$refs.promoteNewConferenceManagerRef as InstanceType<typeof BModal> | undefined)?.hide();
        await this.notifyAction('You have transferred conference manager status.');
      } catch (error) {
        this.errorInfo = getApiErrorMessage(error, 'Something went wrong when transferring conference manager status.');
      } finally {
        this.isBusy = false;
      }
    },
    clearData(): void {
      this.newManager = null;
      this.errorInfo = '';
    }
  }
});
</script>
