<template>
  <b-modal id="removePlayerForm" ref="removePlayerFormRef" size="lg" title="Remove Player" hide-footer @hidden="clearData">
    <div class="alert alert-info">
      This option will allow you to remove any player, even after the draft, midway through the year. You should not do this lightly, as it will invariably affect the experience of your other players.
      It cannot be reversed either. Proceed at your own risk.
    </div>
    <div class="alert alert-info">
      If you are just looking to remove a player that played in previous years but will not be playing this year, please use the "Manage Active Players" option instead.
    </div>
    <div class="form-group">
      <label for="playerToRemove" class="control-label">Player to Remove</label>
      <b-form-select v-model="playerToRemove">
        <option v-for="player in playersWithUsers" :key="player.userID" :value="player">
          {{ player.displayName }}
        </option>
      </b-form-select>

      <template v-if="playerToRemove">
        <hr />
        <div v-show="playerIsLeagueManager(playerToRemove)" class="alert alert-danger">You cannot remove yourself!</div>
        <div v-show="!playerIsLeagueManager(playerToRemove)">
          <div v-if="playerToRemove.removable" class="alert alert-info">This player does not have any publishers created for any years, so they can be safely removed without issue.</div>
          <div v-else>
            <div v-show="!leagueYear.playStatus.playStarted && !playerToRemove.removable" class="alert alert-info">
              Since the draft has not been started, this player can be safely removed. Because they played in previous years, they will be marked as "inactive" this year, and previous years will not
              be affected.
            </div>

            <template v-if="leagueYear.playStatus.playStarted">
              <div class="alert alert-danger">
                If you delete this user, all of their games will become available for pickup. This is not reversible. You should be really, really, sure that this is what you want.
              </div>
              <div class="alert alert-danger">
                The only reason I can think of to use this feature is if a player has been a "problem" in some way, and you need to forcibly remove them from the league, and you are okay with the
                consequences. Again, if you just need to remove a player that played last year, but will not be playing this year,
                <em>do not use this feature.</em>
                Use "Manage Active Players" after starting the new year.
              </div>
            </template>

            <div v-if="!leagueYear.playStatus.playStarted" class="alert alert-info">
              Please type
              <strong>REMOVE PLAYER</strong>
              into the box below and click the button.
            </div>
            <template v-else>
              <div class="alert alert-danger">
                I'm so confident that you
                <em>almost certainly</em>
                do not want to do this that I'm going to be very annoying about it. In order for you to remove this player, you must type the "secret phrase". The reason I do this is because there
                have been many people who have accidentally deleted history from their league just because a player decided not to return the following year.
                <br />
                You can get the "secret phrase" in two ways:
                <ul>
                  <li>Ask me on Discord/Twitter, with an explanation of the situation.</li>
                  <li>Look through the site's source code on GitHub. It's in there, in the file that controls this dialog box.</li>
                </ul>
                Once you have it, type it into the box below and click the button.
              </div>
            </template>
            <input v-show="!playerIsLeagueManager(playerToRemove)" v-model="removeConfirmation" type="text" class="form-control input" />
          </div>
        </div>

        <b-button variant="danger" class="remove-button" :disabled="!removeConfirmed" @click="removePlayer">Remove Player</b-button>
      </template>
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
      playerToRemove: null,
      removeConfirmation: ''
    };
  },
  computed: {
    playersWithUsers() {
      return this.leagueYear.league.players.filter((x) => !!x.userID);
    },
    removeConfirmed() {
      if (this.playerToRemove.removable) {
        return true;
      }
      //This "password" isn't a security concern, it's just to protect the user from doing something they really don't want to do. If you've dug through the code and found this, you probably do know what you are doing.
      let passphrase = 'REMOVE PLAYER';
      if (this.leagueYear.playStatus.playStarted) {
        passphrase = 'I AM SURE I KNOW WHAT I AM DOING';
      }
      let upperCase = this.removeConfirmation.toUpperCase();
      return upperCase === passphrase;
    }
  },
  methods: {
    playerIsLeagueManager(player) {
      return player.userID === this.league.leagueManager.userID;
    },
    async removePlayer() {
      const model = {
        leagueID: this.leagueYear.leagueID,
        userID: this.playerToRemove.userID
      };

      try {
        const response = await axios.post('/api/leagueManager/RemovePlayer', model);
        this.notifyAction(response.data);
        this.clearData();
      } catch (error) {
        this.errorInfo = error.response.data;
      }
    },
    clearData() {
      this.publisherToRemove = null;
      this.playerToRemove = null;
      this.deletePublishers = false;
    }
  }
};
</script>
<style scoped>
.remove-button {
  margin-top: 10px;
  float: right;
}
</style>
