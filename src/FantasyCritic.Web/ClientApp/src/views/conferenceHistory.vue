<template>
  <div>
    <div class="col-md-10 offset-md-1 col-sm-12">
      <div v-if="conferenceYear && conference">
        <h1>Conference History: {{ conference.conferenceName }} (Year {{ year }})</h1>
        <hr />
        <div v-if="conferenceYear.managerMessages && conferenceYear.managerMessages.length > 0">
          <h2>Manager's Messages</h2>
          <div v-for="message in conferenceYear.managerMessages" :key="message.messageID" class="alert alert-info">
            <b-button v-if="conference.isManager" class="delete-button" variant="warning" @click="deleteMessage(message)">Delete</b-button>
            <h5>{{ message.timestamp | dateTime }}</h5>
            <div class="preserve-whitespace">{{ message.messageText }}</div>
          </div>
          <hr />
        </div>
        <b-alert :show="conferenceYear.managerMessages && conferenceYear.managerMessages.length === 0">Conference Manager Messages would show up here, but there are none to show.</b-alert>
      </div>
    </div>
  </div>
</template>
<script>
import axios from 'axios';
import ConferenceMixin from '@/mixins/conferenceMixin.js';

export default {
  mixins: [ConferenceMixin],
  props: {
    conferenceid: { type: String, required: true },
    year: { type: Number, required: true }
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
    async initializePage() {
      this.selectedYear = this.year;
      const conferencePageParams = { conferenceID: this.conferenceid, year: this.year };
      await this.$store.dispatch('initializeConferencePage', conferencePageParams);
    },
    async deleteMessage(message) {
      const model = {
        conferenceid: this.conference.conferenceID,
        year: this.conferenceYear.year,
        messageID: message.messageID
      };
      await axios.post('/api/conference/DeleteConferenceManagerMessage', model);
      this.refreshConferenceYear();
    }
  }
};
</script>
<style scoped>
.delete-button {
  float: right;
}
</style>
