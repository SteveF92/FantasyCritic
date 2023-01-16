<template>
  <b-modal id="removePlayerForm" ref="removePlayerFormRef" size="lg" title="Remove Player" hide-footer @hidden="clearData">
    <div class="form-group">
      <label for="playerToRemove" class="control-label">Player to Remove</label>
      <b-form-select v-model="playerToRemove">
        <option v-for="player in playersWithUsers" :key="player.user.userID" :value="player">
          {{ player.user.displayName }}
        </option>
      </b-form-select>

      <template v-if="playerToRemove">
        <div v-show="playerIsLeagueManager(playerToRemove)" class="alert alert-danger">You cannot remove yourself!</div>
        <div v-show="!playerIsLeagueManager(playerToRemove)">
          <b-form-checkbox v-model="removeFromAllYears">
            <span class="checkbox-label">Remove from previous years</span>
          </b-form-checkbox>

          <template v-if="!removeFromAllYears">
            <div v-show="leagueYear.playStatus.playStarted" class="alert alert-danger">
              If you delete this user, all of their games will become available for pickup. This is not reverseable. You should be really, really, sure that this is what you want.
            </div>

            <div v-show="!leagueYear.playStatus.playStarted && playerToRemove.removable" class="alert alert-info">
              Since the draft has not been started, this player can be safely removed. They did not play in any previous years, so they will be fully removed from the league.
            </div>

            <div v-show="!leagueYear.playStatus.playStarted && !playerToRemove.removable" class="alert alert-info">
              Since the draft has not been started, this player can be safely removed. Because they played in previous years, they will be marked as "inactive" this year, and previous years will not
              be affected.
            </div>
          </template>
          <template v-else>
            <div v-show="!leagueYear.playStatus.playStarted && playerToRemove.removable" class="alert alert-info">
              Since the draft has not been started, this player can be safely removed. They did not play in any previous years, so they will be fully removed from the league.
            </div>
          </template>

          <b-button variant="danger" class="remove-button" :disabled="!removeConfirmed(playerToRemove)" @click="removePlayer">Remove Player</b-button>
        </div>
      </template>
    </div>
  </b-modal>
</template>

<script>
import axios from 'axios';
import LeagueMixin from '@/mixins/leagueMixin';

export default {
  mixins: [LeagueMixin],
  data() {
    return {
      playerToRemove: null,
      removeFromAllYears: false,
      removeConfirmation: ''
    };
  },
  computed: {
    playersWithUsers() {
      return this.players.filter((x) => !!x.user);
    }
  },
  methods: {
    playerIsSafelyRemoveable(player) {
      let matchingLeagueLevelPlayer = _.find(this.playersWithUsers, function (item) {
        return item.user.userID === player.user.userID;
      });

      return matchingLeagueLevelPlayer.user.removable;
    },
    removeConfirmed(player) {
      //This "password" isn't a security concern, it's just to protect the user from doing something they really don't want to do. If you've dug through the code and found this, you probably do know what you are doing.
      let passphase = 'REMOVE PLAYER';
      if (!this.playerIsSafelyRemoveable(player)) {
        passphase = 'I AM SURE I KNOW WHAT I AM DOING';
      }
      let upperCase = this.removeConfirmation.toUpperCase();
      return upperCase === passphase;
    },
    playerIsLeagueManager(player) {
      return player.user.userID === this.league.leagueManager.userID;
    },
    removePlayer() {
      var model = {
        leagueID: this.leagueYear.leagueID,
        userID: this.playerToRemove.user.userID
      };
      axios
        .post('/api/leagueManager/RemovePlayer', model)
        .then(() => {
          this.notifyAction('Player ' + this.playerToRemove.user.displayName + ' has been removed from the league.');
          this.clearData();
        })
        .catch(() => {});
    },
    removePublisher() {
      var model = {
        publisherID: this.publisherToRemove.publisherID
      };
      axios
        .post('/api/leagueManager/RemovePublisher', model)
        .then(() => {
          this.notifyAction('Publisher ' + this.publisherToRemove.publisherName + ' has been removed from the league.');
          this.clearData();
        })
        .catch(() => {});
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
