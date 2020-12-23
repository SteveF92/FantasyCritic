<template>
  <b-modal id="removePlayerForm" ref="removePlayerFormRef" size="lg" title="Remove Player" hide-footer @hidden="clearData">
    <div v-if="!leagueYear.playStatus.playStarted">
      <label>Use this option to remove a Publisher (does not remove the player).</label>
      <div class="form-group">
        <label for="publisherToRemove" class="control-label">Publisher to Remove</label>
        <b-form-select v-model="publisherToRemove">
          <option v-for="publisher in publishers" v-bind:value="publisher">
            {{ publisher.publisherName }}
          </option>
        </b-form-select>
      </div>
      <b-button variant="danger" v-show="publisherToRemove" v-on:click="removePublisher">Remove Publisher</b-button>
      <hr />
    </div>
    <div>
      <label>Use this option to remove a player.</label>
      <div class="alert alert-info">
        This option will allow you to remove any player, even after the draft, midway through the year. You should not do this lightly, as it will invariably affect the experience of your other players.
        This affects all years of your league, not just the current one. It cannot easily be reversed either. Proceed at your own risk.
      </div>
      <div class="form-group">
        <label for="playerToRemove" class="control-label">Player to Remove</label>
        <b-form-select v-model="playerToRemove">
          <option v-for="player in players" v-bind:value="player">
            {{ player.user.displayName }}
          </option>
        </b-form-select>
      </div>

      <div class="alert alert-info" v-show="playerToRemove && playerIsSafelyRemoveable(playerToRemove)">
        This player can be safely removed without any issues.
      </div>
      <div class="alert alert-danger" v-show="playerToRemove && !playerIsSafelyRemoveable(playerToRemove) && !playerIsLeagueManager(playerToRemove)">
        This will affect prior years of this league, not only the current one. Removing a player for the current year in this way will delete their publishers from prior years. If you
        just want to remove a player because they won't be participating in the league anymore, you should use the "Manage Active Players" feature.
      </div>
      <div class="alert alert-danger" v-show="playerToRemove && !playerIsSafelyRemoveable(playerToRemove) && !playerIsLeagueManager(playerToRemove)">
        If you delete a user's publishers, all of their games will become available for pickup.
        This is not reverseable. You should be really, really, sure that this is what you want.
      </div>
      <div class="alert alert-danger" v-show="playerToRemove && !playerIsSafelyRemoveable(playerToRemove) && !playerIsLeagueManager(playerToRemove)">
        If you are sure, type <strong>REMOVE PLAYER</strong> into the box below and click the button.
      </div>


      <div class="alert alert-danger" v-show="playerToRemove && playerIsLeagueManager(playerToRemove)">
        You cannot remove yourself!
      </div>

      <input v-model="removeConfirmation" v-show="playerToRemove && !playerIsLeagueManager(playerToRemove)" type="text" class="form-control input" />


      <b-button variant="danger" class="remove-button" v-show="playerToRemove && !playerIsLeagueManager(playerToRemove)" v-on:click="removePlayer" :disabled="!removeConfirmed">Remove Player</b-button>
    </div>
  </b-modal>
</template>

<script>
  import axios from 'axios';
  export default {
    data() {
      return {
        publisherToRemove: null,
        playerToRemove: null,
        removeConfirmation: ""
      };
    },
    props: ['league','leagueYear'],
    props: ['league', 'leagueYear'],
    computed: {
      publishers() {
        return this.leagueYear.publishers;
      },
      players() {
        return this.leagueYear.players;
      },
      removeConfirmed() {
        let upperCase = this.removeConfirmation.toUpperCase();
        return upperCase === 'REMOVE PLAYER';
      }
    },
    methods: {
      playerIsSafelyRemoveable(player) {
        let matchingLeagueLevelPlayer = _.find(this.league.players, function (item) {
          return item.userID === player.user.userID;
        });

        return matchingLeagueLevelPlayer.removable;
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
          .then(response => {
            let actionInfo = {
              playerName: this.playerToRemove.user.displayName
            };
            this.$emit('playerRemoved', actionInfo);
            this.clearData();
          })
          .catch(response => {

          });
      },
      removePublisher() {
        var model = {
          leagueID: this.leagueYear.leagueID,
          publisherID: this.publisherToRemove.publisherID
        };
        axios
          .post('/api/leagueManager/RemovePublisher', model)
          .then(response => {
            let actionInfo = {
              publisherName: this.publisherToRemove.publisherName
            };
            this.$emit('publisherRemoved', actionInfo);
            this.clearData();
          })
          .catch(response => {

          });
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
