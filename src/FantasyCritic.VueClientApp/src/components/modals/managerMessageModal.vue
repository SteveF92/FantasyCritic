<template>
  <b-modal id="manager-message-modal" ref="managerMessageModalRef" size="lg" title="Post New Message to League" @hidden="clearData">
    <b-alert v-if="errorInfo" variant="danger" :model-value="true">{{ errorInfo }}</b-alert>
    <div class="form-horizontal">
      <div class="form-group">
        <label for="messageText" class="control-label">Message</label>
        <textarea id="messageText" v-model="messageText" class="form-control" rows="3"></textarea>
        <div class="plain-check">
          <input id="isPublic" v-model="isPublic" type="checkbox" class="plain-check-input" />
          <label class="plain-check-label" for="isPublic">Show message to users not in league? (only applies in public leagues)</label>
        </div>
      </div>
    </div>
    <template #footer>
      <input type="submit" class="btn btn-primary" value="Post Message" :disabled="!messageText.trim() || isBusy" @click="postNewMessage" />
    </template>
  </b-modal>
</template>

<script lang="ts">
import { defineComponent } from 'vue';
import { mapActions, mapState } from 'pinia';
import type { BModal } from 'bootstrap-vue-next';

import { leagueManagerClient, type PostNewManagerMessageRequest } from '@/api/clients';
import { useLeagueStore } from '@/store/leagueStore';
import { getApiErrorMessage } from '@/utilities/apiError';

interface ManagerMessageModalData {
  messageText: string;
  isPublic: boolean;
  isBusy: boolean;
  errorInfo: string;
}

export default defineComponent({
  data(): ManagerMessageModalData {
    return {
      messageText: '',
      isPublic: false,
      isBusy: false,
      errorInfo: ''
    };
  },
  computed: {
    ...mapState(useLeagueStore, ['requiredLeagueYear'])
  },
  methods: {
    ...mapActions(useLeagueStore, ['notifyAction']),
    async postNewMessage(): Promise<void> {
      const request: PostNewManagerMessageRequest = {
        leagueID: this.requiredLeagueYear.league.leagueID,
        year: this.requiredLeagueYear.year,
        message: this.messageText,
        isPublic: this.isPublic
      };

      this.isBusy = true;
      this.errorInfo = '';
      try {
        await leagueManagerClient.postNewManagerMessage(request);

        (this.$refs.managerMessageModalRef as InstanceType<typeof BModal> | undefined)?.hide();
        await this.notifyAction("New manager's message posted.");
      } catch (error) {
        this.errorInfo = getApiErrorMessage(error, 'Something went wrong when posting the message.');
      } finally {
        this.isBusy = false;
      }
    },
    clearData(): void {
      this.messageText = '';
      this.errorInfo = '';
    }
  }
});
</script>
