<template>
  <b-modal id="transfer-manager-modal" ref="transferManagerModalRef" size="lg" title="Transfer Manager" no-footer @hidden="clearData">
    <b-alert v-if="errorInfo" variant="danger" :model-value="true">{{ errorInfo }}</b-alert>
    <div class="alert alert-warning">Warning! If you promote a new player to be League Manager, you will no longer be League Manager! There can only be one!</div>
    <div class="form-group">
      <label for="newManager" class="control-label">New Manager</label>
      <b-form-select id="newManager" v-model="newManager">
        <option v-for="user in players" :key="user.userID" :value="user">
          {{ user.displayName }}
        </option>
      </b-form-select>
      <br />
      <br />
      <b-button variant="danger" :disabled="!newManager || isBusy" @click="promoteNewManager">Transfer League Manager</b-button>
    </div>
  </b-modal>
</template>

<script lang="ts">
import { defineComponent } from 'vue';
import { mapActions, mapState } from 'pinia';
import type { BModal } from 'bootstrap-vue-next';

import { leagueManagerClient, type PlayerViewModel, type PromoteNewLeagueManagerRequest } from '@/api/clients';
import { useAuthStore } from '@/store/authStore';
import { useLeagueStore } from '@/store/leagueStore';
import { getApiErrorMessage } from '@/utilities/apiError';

interface TransferManagerModalData {
  newManager: PlayerViewModel | null;
  isBusy: boolean;
  errorInfo: string;
}

export default defineComponent({
  data(): TransferManagerModalData {
    return {
      newManager: null,
      isBusy: false,
      errorInfo: ''
    };
  },
  computed: {
    ...mapState(useAuthStore, ['userInfo']),
    ...mapState(useLeagueStore, ['requiredLeague']),
    players(): PlayerViewModel[] {
      return (this.requiredLeague.players ?? []).filter((x) => x.userID !== this.userInfo?.userID);
    }
  },
  methods: {
    ...mapActions(useLeagueStore, ['notifyAction']),
    async promoteNewManager(): Promise<void> {
      if (!this.newManager) {
        return;
      }

      const request: PromoteNewLeagueManagerRequest = {
        leagueID: this.requiredLeague.leagueID,
        newManagerUserID: this.newManager.userID
      };

      this.isBusy = true;
      this.errorInfo = '';
      try {
        await leagueManagerClient.promoteNewLeagueManager(request);

        (this.$refs.transferManagerModalRef as InstanceType<typeof BModal> | undefined)?.hide();
        await this.notifyAction('You have transferred league manager status.');
      } catch (error) {
        this.errorInfo = getApiErrorMessage(error, 'Something went wrong when transferring league manager status.');
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
