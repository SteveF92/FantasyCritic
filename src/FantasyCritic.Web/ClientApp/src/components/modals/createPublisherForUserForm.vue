<template>
  <b-modal id="createPublisherForUserForm" ref="createPublisherForUserFormRef" title="Create Publisher For User" @hidden="clearData">
    <div class="alert alert-warning">It is better that the players in your league create their own publishers, but if they cannot, you can create one for them here. They can rename it later.</div>

    <div class="form-group">
      <label for="playerToRemove" class="control-label">Player</label>
      <b-form-select v-model="playerToCreatePublisherFor">
        <option v-for="player in playersWithoutPublishers" v-bind:value="player">
          {{ player.user.displayName }}
        </option>
      </b-form-select>
    </div>

    <div class="form-horizontal">
      <div class="form-group">
        <label for="publisherName" class="control-label">Publisher Name</label>
        <input v-model="publisherName" id="publisherName" name="publisherName" type="text" class="form-control input" />
      </div>
    </div>
    <div slot="modal-footer">
      <input type="submit" class="btn btn-primary" value="Create Publisher" v-on:click="createPublisher" :disabled="!publisherName" />
    </div>
  </b-modal>
</template>
<script>
import Vue from 'vue';
import axios from 'axios';

export default {
  data() {
    return {
      playerToCreatePublisherFor: null,
      publisherName: '',
      errorInfo: ''
    };
  },
  props: ['leagueYear'],
  computed: {
    playersWithoutPublishers() {
      let playersToReturn = [];
      let userIDsWithPublishers = this.leagueYear.publishers.map((x) => x.userID);
      for (const player of this.leagueYear.players) {
        if (userIDsWithPublishers.includes(player.user.userID)) {
          continue;
        }
        playersToReturn.push(player);
      }
      return playersToReturn;
    }
  },
  methods: {
    createPublisher() {
      var model = {
        leagueID: this.leagueYear.leagueID,
        year: this.leagueYear.year,
        userID: this.playerToCreatePublisherFor.user.userID,
        publisherName: this.publisherName
      };
      axios
        .post('/api/leagueManager/createPublisherForUser', model)
        .then((response) => {
          this.$refs.createPublisherForUserFormRef.hide();
          let actionInfo = {
            publisherName: this.publisherName
          };
          this.$emit('publisherCreated', actionInfo);
          this.clearData();
        })
        .catch((response) => {});
    },
    clearData() {
      this.publisherName = '';
      this.playerToCreatePublisherFor = null;
    }
  }
};
</script>
