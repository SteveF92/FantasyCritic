<template>
  <b-modal id="removePublisherModal" ref="removePublisherModalRef" title="Remove a Publisher" @hidden="clearData">
    <div v-show="errorInfo" class="alert alert-danger">
      {{ errorInfo }}
    </div>

    <div class="alert alert-info">
      Only a publisher with no player can be removed. To remove a player's publisher, use "Disconnect a Player" first.
    </div>

    <div v-if="playStarted" class="alert alert-warning">
      Removing a publisher affects the competitive balance of your league. 
      If you remove the publisher, all of the games owned by it that haven't released yet become available for bidding.
      This should not be done lightly, as it is not reversible. 
      A publisher with no player connected to it is already effectively "frozen", that player cannot participate any longer.
    </div>

    <div class="form-group">
      <label for="publisherToRemove" class="control-label">Publisher</label>
      <b-form-select id="publisherToRemove" v-model="publisherToRemove">
        <option v-for="publisher in removablePublishers" :key="publisher.publisherID" :value="publisher">{{ publisher.publisherName }}</option>
      </b-form-select>
    </div>

    <div v-if="readyToConfirm">
      <div class="alert alert-warning">If you are sure you want do this, please type "I acknowledge the implications for my league." below.</div>
      <input v-model="removeConfirmation" type="text" class="form-control input" />
    </div>

    <template #modal-footer>
      <input type="submit" class="btn btn-danger" value="Remove Publisher" :disabled="!readyToRemove" @click="removePublisher" />
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
      removeConfirmation: '',
      errorInfo: ''
    };
  },
  computed: {
    removablePublishers() {
      return this.publishers.filter((x) => !x.userID);
    },
    readyToConfirm() {
      return !!this.publisherToRemove;
    },
    readyToRemove() {
      return this.readyToConfirm && this.removeConfirmation === 'I acknowledge the implications for my league.';
    },
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
      this.removeConfirmation = '';
    }
  }
};
</script>
