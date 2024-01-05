<template>
  <b-modal id="transferManagerForm" ref="transferManagerFormRef" size="lg" title="Transfer Manager" hide-footer @hidden="clearData">
    <div class="alert alert-warning">Warning! If you promote a new player to be League Manager, you will no longer be League Manager! There can only be one!</div>
    <div class="form-group">
      <label for="newManager" class="control-label">New Manager</label>
      <b-form-select v-model="newManager">
        <option v-for="user in players" :key="user.userID" :value="user">
          {{ user.displayName }}
        </option>
      </b-form-select>
      <br />
      <br />
      <b-button variant="danger" @click="promoteNewManager">Transfer League Manager</b-button>
    </div>
  </b-modal>
</template>

<script>
import axios from 'axios';
import LeagueMixin from '@/mixins/leagueMixin.js';

export default {
  mixins: [LeagueMixin],
  data() {
    return {
      newManager: null
    };
  },
  computed: {
    players() {
      return this.league.players.filter((x) => x.userID !== this.userInfo.userID);
    }
  },
  methods: {
    async promoteNewManager() {
      const model = {
        leagueID: this.league.leagueID,
        newManagerUserID: this.newManager.userID
      };
      await axios.post('/api/leagueManager/PromoteNewLeagueManager', model);
      this.$refs.transferManagerFormRef.hide();
      this.notifyAction('You have transferred league manager status.');
    },
    clearData() {
      this.newManager = null;
    }
  }
};
</script>
