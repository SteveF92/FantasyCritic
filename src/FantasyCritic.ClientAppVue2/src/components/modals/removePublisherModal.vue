<template>
  <b-modal id="removePublisherModal" ref="removePublisherModalRef" title="Remove a Publisher" @hidden="clearData">
    <div v-show="errorInfo" class="alert alert-danger">
      {{ errorInfo }}
    </div>

    <div class="alert alert-info">Only a publisher with no player can be removed. To remove a player's publisher, use "Disconnect a Player" first.</div>

    <div v-if="playStarted" class="alert alert-warning">
      This is not recommended. A publisher with no player is already frozen and can stay in the league for the rest of the year. If you remove it, all of its games are deleted, and any that haven't
      released yet become available for the rest of the league to pick up, which can change the balance of your league. It cannot be undone.
    </div>

    <div class="form-group">
      <label for="publisherToRemove" class="control-label">Publisher</label>
      <b-form-select id="publisherToRemove" v-model="publisherToRemove">
        <option v-for="publisher in removablePublishers" :key="publisher.publisherID" :value="publisher">{{ publisher.publisherName }}</option>
      </b-form-select>
    </div>

    <template #modal-footer>
      <input type="submit" class="btn btn-danger" value="Remove Publisher" :disabled="!publisherToRemove" @click="removePublisher" />
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
      publisherToRemove: null,
      errorInfo: ''
    };
  },
  computed: {
    removablePublishers() {
      return this.publishers.filter((x) => !x.userID);
    }
  },
  methods: {
    async removePublisher() {
      const publisher = this.publisherToRemove;
      const model = {
        publisherID: publisher.publisherID
      };

      try {
        await axios.post('/api/leagueManager/RemovePublisher', model);
        this.$refs.removePublisherModalRef.hide();
        this.notifyAction(`${publisher.publisherName} has been removed from the league.`);
      } catch (error) {
        this.errorInfo = error.response.data;
      }
    },
    clearData() {
      this.publisherToRemove = null;
      this.errorInfo = '';
    }
  }
};
</script>
