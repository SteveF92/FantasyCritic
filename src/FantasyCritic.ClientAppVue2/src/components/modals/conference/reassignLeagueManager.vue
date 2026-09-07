<template>
  <b-modal id="reassignLeagueManager" ref="reassignLeagueManagerRef" title="Reassign League Manager" hide-footer>
    <div class="alert alert-warning">
      This option can be used to designate a new league manager for one of the leagues in this conference. This means that the player that is currently league manager will no longer be league manager
      of said league. Leagues can only ever have one manager.
    </div>
    <b-alert variant="danger" :show="!!errorInfo">{{ errorInfo }}</b-alert>
    <div class="form-group">
      <label for="league" class="control-label">League</label>
      <b-form-select v-model="selectedLeague">
        <option v-for="league in conference.leaguesInConference" :key="league.leagueID" :value="league">
          {{ league.leagueName }}
        </option>
      </b-form-select>
      <label for="newManager" class="control-label">New Manager</label>
      <b-form-select v-model="newManager">
        <option v-for="user in players" :key="user.userID" :value="user">
          {{ user.displayName }}
        </option>
      </b-form-select>
      <br />
      <br />
      <b-button variant="danger" class="full-width-button" @click="promoteNewManager">Transfer League Manager</b-button>
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
      selectedLeague: null,
      newManager: null,
      errorInfo: null
    };
  },
  computed: {
    players() {
      return this.conference.players;
    }
  },
  methods: {
    async promoteNewManager() {
      const model = {
        conferenceID: this.conference.conferenceID,
        leagueID: this.selectedLeague.leagueID,
        newManagerUserID: this.newManager.userID
      };
      try {
        await axios.post('/api/conference/ReassignLeagueManager', model);
        this.$refs.reassignLeagueManagerRef.hide();
        this.notifyAction('League manager has been changed.');
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
