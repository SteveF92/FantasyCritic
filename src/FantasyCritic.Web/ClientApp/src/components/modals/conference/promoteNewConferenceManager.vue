<template>
  <b-modal id="promoteNewConferenceManager" ref="promoteNewConferenceManagerRef" title="Promote New Conference Manager" hide-footer @hidden="clearData">
    <div class="alert alert-warning">Warning! If you promote a new player to be Conference Manager, you will no longer be Conference Manager! There can only be one!</div>
    <b-alert variant="danger" :show="!!errorInfo">{{ errorInfo }}</b-alert>
    <div class="form-group">
      <label for="newManager" class="control-label">New Manager</label>
      <b-form-select v-model="newManager">
        <option v-for="user in players" :key="user.userID" :value="user">
          {{ user.displayName }}
        </option>
      </b-form-select>
      <br />
      <br />
      <b-button variant="danger" class="full-width-button" @click="promoteNewManager">Transfer Conference Manager</b-button>
    </div>
  </b-modal>
</template>

<script>
import axios from 'axios';
import ConferenceMixin from '@/mixins/conferenceMixin.js';

export default {
  mixins: [ConferenceMixin],
  data() {
    return {
      newManager: null,
      errorInfo: null
    };
  },
  computed: {
    players() {
      return this.conference.players.filter((x) => x.userID !== this.userInfo.userID);
    }
  },
  methods: {
    async promoteNewManager() {
      const model = {
        conferenceID: this.conference.conferenceID,
        newManagerUserID: this.newManager.userID
      };
      try {
        await axios.post('/api/conference/PromoteNewConferenceManager', model);
        this.$refs.promoteNewConferenceManagerRef.hide();
        this.notifyAction('You have transferred conference manager status.');
      } catch (error) {
        this.errorInfo = error.response.data;
      }
    },
    clearData() {
      this.newManager = null;
    }
  }
};
</script>
