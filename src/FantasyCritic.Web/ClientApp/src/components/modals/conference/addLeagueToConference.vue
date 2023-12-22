<template>
  <b-modal id="addLeagueToConference" ref="addLeagueToConferenceRef" title="Add League to Conference" @hidden="clearData">
    <div class="alert alert-info">
      All leagues added to your conference will start with the same options as the first league in the conference. You only need to chose the league name, and the league manager. Both can be changed
      later.
      <br />

      The league manager can be any user in the conference, including you (the conference manager).
    </div>

    <div class="form-horizontal">
      <div class="form-group">
        <label for="newLeagueName" class="control-label">League Name</label>
        <input id="newLeagueName" v-model="newLeagueName" name="newLeagueName" type="text" class="form-control input" />
      </div>

      <div class="form-group">
        <label for="newLeagueManager" class="control-label">League Manager</label>
        <b-form-select v-model="newLeagueManager">
          <option v-for="player in conference.players" :key="player.userID" :value="player">
            {{ player.displayName }}
          </option>
        </b-form-select>
      </div>
    </div>

    <template #modal-footer>
      <input type="submit" class="btn btn-primary" value="Add New League" :disabled="!newLeagueName || !newLeagueManager" @click="addNewLeague" />
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
      newLeagueName: '',
      newLeagueManager: null,
      errorInfo: null
    };
  },
  mounted() {
    this.clearData();
  },
  methods: {
    async addNewLeague() {
      const model = {
        conferenceID: this.conference.conferenceID,
        year: this.conferenceYear.year,
        leagueName: this.newLeagueName,
        leagueManager: this.newLeagueManager.userID
      };

      try {
        await axios.post('/api/conference/AddLeagueToConference', model);
        this.$refs.addLeagueToConferenceRef.hide();
        await this.refreshConferenceYear();
      } catch (error) {
        this.errorInfo = error.response.data;
      }
    },
    clearData() {
      this.newLeagueName = '';
      this.newLeagueManager = null;
      this.errorInfo = null;
    }
  }
};
</script>
