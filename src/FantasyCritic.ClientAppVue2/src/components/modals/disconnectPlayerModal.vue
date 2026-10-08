<template>
  <b-modal id="disconnectPlayerModal" ref="disconnectPlayerModalRef" title="Disconnect a Player" @hidden="clearData">
    <div v-show="errorInfo" class="alert alert-danger">
      {{ errorInfo }}
    </div>

    <div class="alert alert-warning">
      You can use this option to remove a player from the league.
      <ul>
        <li>The player is marked inactive this year and can no longer act for the publisher.</li>
        <li>The publisher, and any games it already contains, remains in place for the rest of the year, but it is effectively "frozen in time" as it no longer has a player attached to it.</li>
        <li>Any pending bids or drops drops the player placed are deleted, and any open trades they are involved in are automatically rejected.</li>
        <li>If the player had any games on their publisher's watch list, they will be cleared.</li>
        <li>If this is a multi draft league, the publisher will be skipped in any future drafts.</li>
      </ul>
      If you chose to, you can transfer the publisher to a new player using the "Reassign a Publisher" option.
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
