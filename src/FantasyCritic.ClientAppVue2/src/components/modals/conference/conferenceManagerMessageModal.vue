<template>
  <b-modal id="conferenceManagerMessageForm" ref="conferenceManagerMessageFormRef" size="lg" title="Post New Message to Conference" @hidden="clearData">
    <div class="form-horizontal">
      <div class="form-group">
        <label for="messageText" class="control-label">Message</label>
        <textarea v-model="messageText" class="form-control" rows="3"></textarea>
        <div class="form-check">
          <input v-model="isPublic" type="checkbox" class="form-check-input" />
          <label class="form-check-label" for="isPublic">Show message to users not in conference?</label>
        </div>
      </div>
    </div>
    <template #modal-footer>
      <input type="submit" class="btn btn-primary" value="Post Message" :disabled="!messageText" @click="postNewMessage" />
    </template>
  </b-modal>
</template>

<script>
import axios from 'axios';
import ConferenceMixin from '@/mixins/conferenceMixin.js';

export default {
  mixins: [ConferenceMixin],
  data() {
    return {
      messageText: null,
      isPublic: false
    };
  },
  computed: {},
  methods: {
    postNewMessage() {
      const model = {
        conferenceID: this.conference.conferenceID,
        year: this.conferenceYear.year,
        message: this.messageText,
        isPublic: this.isPublic
      };
      axios
        .post('/api/conference/PostNewConferenceManagerMessage', model)
        .then(() => {
          this.$refs.conferenceManagerMessageFormRef.hide();
          this.notifyAction("New manager's message posted.");
          this.clearData();
        })
        .catch(() => {});
    },
    clearData() {
      this.messageText = null;
    }
  }
};
</script>
