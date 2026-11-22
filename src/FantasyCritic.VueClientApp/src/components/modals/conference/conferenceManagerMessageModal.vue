<template>
  <b-modal id="conferenceManagerMessageForm" ref="conferenceManagerMessageFormRef" size="lg" title="Post New Message to Conference" @hidden="clearData">
    <b-alert v-if="errorInfo" variant="danger" :model-value="true">{{ errorInfo }}</b-alert>
    <div class="form-horizontal">
      <div class="form-group">
        <label for="conferenceMessageText" class="control-label">Message</label>
        <textarea id="conferenceMessageText" v-model="messageText" class="form-control" rows="3"></textarea>
        <div class="plain-check">
          <input id="conferenceMessageIsPublic" v-model="isPublic" type="checkbox" class="plain-check-input" />
          <label class="plain-check-label" for="conferenceMessageIsPublic">Show message to users not in conference?</label>
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

import { conferenceClient, type PostNewConferenceManagerMessageRequest } from '@/api/clients';
import { useConferenceStore } from '@/store/conferenceStore';
import { getApiErrorMessage } from '@/utilities/apiError';

interface ConferenceManagerMessageModalData {
  messageText: string;
  isPublic: boolean;
  isBusy: boolean;
  errorInfo: string;
}

export default defineComponent({
  data(): ConferenceManagerMessageModalData {
    return {
      messageText: '',
      isPublic: false,
      isBusy: false,
      errorInfo: ''
    };
  },
  computed: {
    ...mapState(useConferenceStore, ['requiredConference', 'requiredConferenceYear'])
  },
  methods: {
    ...mapActions(useConferenceStore, ['notifyAction']),
    async postNewMessage(): Promise<void> {
      const request: PostNewConferenceManagerMessageRequest = {
        conferenceID: this.requiredConference.conferenceID,
        year: this.requiredConferenceYear.year,
        message: this.messageText,
        isPublic: this.isPublic
      };

      this.isBusy = true;
      this.errorInfo = '';
      try {
        await conferenceClient.postNewConferenceManagerMessage(request);

        (this.$refs.conferenceManagerMessageFormRef as InstanceType<typeof BModal> | undefined)?.hide();
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
