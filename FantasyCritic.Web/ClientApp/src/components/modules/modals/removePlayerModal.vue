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
    </div>
    <hr/>
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
      <div class="form-check" v-show="playerToRemove">
        <input type="checkbox" class="form-check-input" v-model="deletePublishers">
        <label class="form-check-label" for="deletePublishers">Delete User's Publishers?</label>
      </div>
      <div class="alert alert-info" v-show="playerToRemove && !deletePublishers">
        Removing a player without removing their publisher will preserve the publisher for gameplay purposes, but the removed player will no longer control that player.
        Other players will not be able to pick up any of the removed publishers games. It will effectively be an "orphaned" or "inactive" publisher. This is the recommended option.
      </div>
      <div class="alert alert-danger" v-show="playerToRemove && deletePublishers">
        If you delete a user's publishers, all of their games will become available for pickup.
        This is not reverseable. You should be really, really, sure that this is what you want.
      </div>
      <b-button variant="danger" v-show="playerToRemove" v-on:click="removePlayer">Remove Player</b-button>
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
        deletePublishers: false
      };
    },
    props: ['league','leagueYear'],
    computed: {
      publishers() {
        return this.leagueYear.publishers;
      },
      players() {
        return this.leagueYear.players;
      }
    },
    methods: {
      removeUser(user) {
        var model = {
          leagueID: this.leagueYear.leagueID,
          userID: user.userID
        };
        axios
          .post('/api/leagueManager/RemovePlayer', model)
          .then(response => {
            let actionInfo = {
              playerName: user.displayName,
            };
            this.$emit('playerRemoved', actionInfo);
          })
          .catch(response => {

          });

        var model = {
          leagueID: this.leagueYear.leagueID,
          userID: this.playerToRemove.user.userID,
          deletePublishers: this.deletePublishers
        };
        axios
          .post('/api/leagueManager/RemovePlayer', model)
          .then(response => {
            let actionInfo = {
              playerName: this.playerToRemove.user.displayName
            };
            this.$emit('playerRemoved', actionInfo);
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
          })
          .catch(response => {

          });
      },
      clearData() {

      }
    }
  };
</script>
