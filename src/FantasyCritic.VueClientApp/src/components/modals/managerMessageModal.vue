<template>
  <b-modal id="managerMessageForm" ref="managerMessageFormRef" size="lg" title="Post New Message to League" @hidden="clearData">
    <div class="form-horizontal">
      <div class="form-group">
        <label for="messageText" class="control-label">Message</label>
        <textarea v-model="messageText" class="form-control" rows="3"></textarea>
        <div class="plain-check">
          <input v-model="isPublic" type="checkbox" class="plain-check-input" />
          <label class="plain-check-label" for="isPublic">Show message to users not in league? (only applies in public leagues)</label>
        </div>
      </div>
    </div>
    <template #footer>
      <input type="submit" class="btn btn-primary" value="Post Message" :disabled="!messageText" @click="postNewMessage" />
    </template>
  </b-modal>
</template>

<script>
import { mapActions, mapState } from 'pinia';
import axios from 'axios';

import { useLeagueStore } from '@/store/leagueStore';

export default {
  data() {
    return {
      messageText: null,
      isPublic: false
    };
  },
  computed: {
    ...mapState(useLeagueStore, ['leagueYear', 'league'])
  },
  methods: {
    ...mapActions(useLeagueStore, ['notifyAction']),
    postNewMessage() {
      const model = {
        leagueID: this.league.leagueID,
        year: this.leagueYear.year,
        message: this.messageText,
        isPublic: this.isPublic
      };
      axios
        .post('/api/leagueManager/PostNewManagerMessage', model)
        .then(() => {
          this.$refs.managerMessageFormRef.hide();
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
