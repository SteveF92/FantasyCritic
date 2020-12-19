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
        It cannot easily be reversed either. Proceed at your own risk.
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
      <div v-show="playerToRemove && !playerIsSafelyRemoveable(playerToRemove) && !playerIsLeagueManager(playerToRemove)">
        <div class="alert alert-danger">
          If you delete a user's publishers, all of their games will become available for pickup.
          This is not reverseable. You should be really, really, sure that this is what you want.
        </div>
        <div class="alert alert-warning">
          A safer option is to transfer the unwanted player's publishers to a new user, even if you don't plan on using it going forward. Use the "Transfer Publisher" option for that.
          Once you have transferred all of a player's publishers (more than one if they've played multiple years), you can safely delete them using this feature.
        </div>
      </div>
      
      <div class="alert alert-danger" v-show="playerToRemove && playerIsLeagueManager(playerToRemove)">
        You cannot remove yourself!
      </div>

      <b-button variant="danger" v-show="playerToRemove && !playerIsLeagueManager(playerToRemove)" v-on:click="removePlayer">Remove Player</b-button>
    </div>
  </b-modal>
</template>

<script>
  import axios from 'axios';
  export default {
    data() {
      return {
        publisherToRemove: null,
        playerToRemove: null
      };
    },
    props: ['league','leagueYear'],
    computed: {
      publishers() {
        return this.leagueYear.publishers;
      },
      players() {
        return this.leagueYear.players;
      },
      
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
