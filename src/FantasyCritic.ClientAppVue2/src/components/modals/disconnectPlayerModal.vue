<template>
  <b-modal id="disconnectPlayerModal" ref="disconnectPlayerModalRef" title="Disconnect a Player" @hidden="clearData">
    <div v-show="errorInfo" class="alert alert-danger">
      {{ errorInfo }}
    </div>

    <div class="alert alert-warning">
      If you disconnect a player from their publisher:
      <ul>
        <li>The publisher stays in the league for the rest of the year, with its games and history.</li>
        <li>The player is marked inactive this year and can no longer act for the publisher.</li>
        <li>The publisher's pending bids, drops and watchlist are deleted, and its open trades are rejected.</li>
        <li>The publisher is skipped in any later drafts.</li>
      </ul>
      You can give the publisher to another player later with "Reassign a Publisher".
    </div>

    <div class="form-group">
      <label for="publisherToDisconnect" class="control-label">Publisher</label>
      <b-form-select id="publisherToDisconnect" v-model="publisherToDisconnect">
        <option v-for="publisher in disconnectablePublishers" :key="publisher.publisherID" :value="publisher">{{ publisher.publisherName }} ({{ publisher.playerName }})</option>
      </b-form-select>
    </div>

    <template #modal-footer>
      <input type="submit" class="btn btn-danger" value="Disconnect Player" :disabled="!publisherToDisconnect" @click="disconnectPlayer" />
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
      publisherToDisconnect: null,
      errorInfo: ''
    };
  },
  computed: {
    disconnectablePublishers() {
      return this.publishers.filter((x) => !!x.userID && x.userID !== this.league.leagueManager.userID);
    }
  },
  methods: {
    async disconnectPlayer() {
      const publisher = this.publisherToDisconnect;
      const model = {
        publisherID: publisher.publisherID
      };

      try {
        await axios.post('/api/leagueManager/DisconnectPlayer', model);
        this.$refs.disconnectPlayerModalRef.hide();
        this.notifyAction(`${publisher.playerName} has been disconnected from ${publisher.publisherName}.`);
      } catch (error) {
        this.errorInfo = error.response.data;
      }
    },
    clearData() {
      this.publisherToDisconnect = null;
      this.errorInfo = '';
    }
  }
};
</script>
