<template>
  <b-modal id="removePlayerModal" ref="removePlayerModalRef" title="Remove a Player From the League" @hidden="clearData">
    <div v-show="errorInfo" class="alert alert-danger">
      {{ errorInfo }}
    </div>

    <div class="alert alert-info">
     This option will let you remove a player from the league, provided they have no publishers, either in the current year, or in previous years.
      <ul>
        <li>If you are starting a new year, and there is a player who doesn't wish to return, use "Manage Active Players".</li>
        <li>If you are in the middle of a year and need to remove a player, use "Disconnect a Player".</li>
      </ul>

      This particular feature is most useful for situations such as a player not showing up to the draft. If they never played at all, they can be removed here.
    </div>

    <div class="form-group">
      <label for="playerToRemove" class="control-label">Player</label>
      <b-form-select id="playerToRemove" v-model="playerToRemove">
        <option v-for="player in removablePlayers" :key="player.userID" :value="player">{{ player.displayName }}</option>
      </b-form-select>
    </div>

    <template #modal-footer>
      <input type="submit" class="btn btn-danger" value="Remove Player" :disabled="!playerToRemove" @click="removePlayer" />
    </template>
  </b-modal>
</template>
<script>
import axios from 'axios';
import LeagueMixin from '@/mixins/leagueMixin.js';

export default {
  mixins: [LeagueMixin],
  data() {
    return {
      playerToRemove: null,
      errorInfo: ''
    };
  },
  computed: {
    removablePlayers() {
      return this.league.players.filter((x) => x.userID !== this.league.leagueManager.userID);
    }
  },
  methods: {
    async removePlayer() {
      const player = this.playerToRemove;
      const model = {
        leagueID: this.league.leagueID,
        userID: player.userID
      };

      try {
        await axios.post('/api/leagueManager/RemovePlayerFromLeague', model);
        this.$refs.removePlayerModalRef.hide();
        this.notifyAction(`${player.displayName} has been removed from the league.`);
      } catch (error) {
        this.errorInfo = error.response.data;
      }
    },
    clearData() {
      this.playerToRemove = null;
      this.errorInfo = '';
    }
  }
};
</script>
