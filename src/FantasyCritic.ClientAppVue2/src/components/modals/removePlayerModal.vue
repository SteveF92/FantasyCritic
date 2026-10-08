<template>
  <b-modal id="removePlayerModal" ref="removePlayerModalRef" title="Remove a Player From the League" @hidden="clearData">
    <div v-show="errorInfo" class="alert alert-danger">
      {{ errorInfo }}
    </div>

    <div class="alert alert-info">
      Use this to remove someone who never played in this league, such as a player who joined but never showed up. A player who has had a publisher in any year can't be removed, because that would
      erase league history.
      <ul>
        <li>To cut a player off during this year, use "Disconnect a Player".</li>
        <li>To leave a player out of a new year, use "Manage Active Players".</li>
      </ul>
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
