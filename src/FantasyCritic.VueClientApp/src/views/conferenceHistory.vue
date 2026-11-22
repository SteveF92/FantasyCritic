<template>
  <div class="col-md-10 offset-md-1 col-sm-12">
    <div v-if="hasError" class="alert alert-danger" role="alert">
      Something went wrong with this conference. Contact us on Discord or via email for support. Please include the conference ID in your message (Linking the URL will do).
    </div>
    <div v-if="conferenceYear && conference">
      <h1>Conference History: {{ conference.conferenceName }} (Year {{ year }})</h1>
      <router-link :to="{ name: 'conference', params: { conferenceid, year } }">Back to Conference</router-link>
      <hr />
      <b-alert v-if="errorInfo" variant="danger" :model-value="true">{{ errorInfo }}</b-alert>
      <div v-if="conferenceYear.managerMessages.length > 0">
        <h2>Manager's Messages</h2>
        <div v-for="message in conferenceYear.managerMessages" :key="message.messageID" class="alert alert-info">
          <b-button v-if="conference.isManager" class="delete-button" variant="warning" :disabled="isBusy" @click="deleteMessage(message)">Delete</b-button>
          <h5>{{ formatDateTime(message.timestamp) }}</h5>
          <div class="preserve-whitespace">{{ message.messageText }}</div>
        </div>
        <hr />
      </div>
      <b-alert :model-value="conferenceYear.managerMessages.length === 0">Conference Manager Messages would show up here, but there are none to show.</b-alert>
    </div>
  </div>
</template>

<script lang="ts">
import { defineComponent } from 'vue';
import { mapActions, mapState } from 'pinia';

import { conferenceClient, type DeleteConferenceManagerMessageRequest, type ManagerMessageViewModel } from '@/api/clients';
import { useConferenceStore, type ConferencePageParams } from '@/store/conferenceStore';
import { getApiErrorMessage } from '@/utilities/apiError';
import { formatDateTime } from '@/utilities/formatters';

interface ConferenceHistoryData {
  isBusy: boolean;
  errorInfo: string;
}

export default defineComponent({
  props: {
    conferenceid: { type: String, required: true },
    year: { type: Number, required: true }
  },
  data(): ConferenceHistoryData {
    return {
      isBusy: false,
      errorInfo: ''
    };
  },
  computed: {
    ...mapState(useConferenceStore, ['conference', 'conferenceYear', 'requiredConferenceYear', 'hasError'])
  },
  watch: {
    async $route(to, from) {
      if (to.path !== from.path) {
        await this.initializePage();
      }
    }
  },
  async created() {
    await this.initializePage();
  },
  methods: {
    ...mapActions(useConferenceStore, ['refreshConferenceYear']),
    formatDateTime,
    async initializePage(): Promise<void> {
      this.errorInfo = '';
      const conferencePageParams: ConferencePageParams = { conferenceID: this.conferenceid, year: this.year };
      const conferenceStore = useConferenceStore();
      await conferenceStore.initializeConferencePage(conferencePageParams);
    },
    async deleteMessage(message: ManagerMessageViewModel): Promise<void> {
      const request: DeleteConferenceManagerMessageRequest = {
        conferenceID: this.requiredConferenceYear.conference.conferenceID,
        year: this.requiredConferenceYear.year,
        messageID: message.messageID
      };

      this.isBusy = true;
      this.errorInfo = '';
      try {
        await conferenceClient.deleteConferenceManagerMessage(request);
        await this.refreshConferenceYear();
      } catch (error) {
        this.errorInfo = getApiErrorMessage(error, 'Something went wrong when deleting the message.');
      } finally {
        this.isBusy = false;
      }
    }
  }
});
</script>

<style scoped>
.delete-button {
  float: right;
}
</style>
