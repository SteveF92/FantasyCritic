<template>
  <b-modal id="createPublisherForUserForm" ref="createPublisherForUserFormRef" title="Create Publisher For User" @hidden="clearData">
    <div class="alert alert-warning">It is better that the players in your league create their own publishers, but if they cannot, you can create one for them here. They can rename it later.</div>

    <div class="form-group">
      <label for="playerToRemove" class="control-label">Player</label>
      <b-form-select v-model="playerToCreatePublisherFor">
        <option v-for="player in playersWithoutPublishers" :key="player.userID" :value="player">
          {{ player.user.displayName }}
        </option>
      </b-form-select>
    </div>

    <div class="form-horizontal">
      <div class="form-group">
        <label for="publisherName" class="control-label">Publisher Name</label>
        <input id="publisherName" v-model="publisherName" name="publisherName" type="text" class="form-control input" />
      </div>
    </div>
    <template #modal-footer>
      <input type="submit" class="btn btn-primary" value="Create Publisher" :disabled="!publisherName" @click="createPublisher" />
    </template>
  </b-modal>
</template>
<script>
import axios from 'axios';
import LeagueMixin from '@/mixins/leagueMixin';

export default {
  mixins: [LeagueMixin],
  data() {
    return {
      playerToCreatePublisherFor: null,
      publisherName: '',
      errorInfo: ''
    };
  },
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
        .then(() => {
          this.$refs.createPublisherForUserFormRef.hide();
          this.notifyAction('Publisher ' + this.publisherName + ' has been created.');
          this.clearData();
        })
        .catch(() => {});
    },
    clearData() {
      this.publisherName = '';
      this.playerToCreatePublisherFor = null;
    }
  }
};
</script>
