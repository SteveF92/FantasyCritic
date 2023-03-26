<template>
  <b-modal id="removePublisherForm" ref="removePublisherFormRef" size="lg" title="Remove Player" hide-footer @hidden="clearData">
    <label>Use this option to remove a Publisher (does not remove the player).</label>
    <div class="form-group">
      <label for="publisherToRemove" class="control-label">Publisher to Remove</label>
      <b-form-select v-model="publisherToRemove">
        <option v-for="publisher in publishers" :key="publisher.publisherID" :value="publisher">
          {{ publisher.publisherName }}
        </option>
      </b-form-select>
    </div>
    <b-button v-show="publisherToRemove" variant="danger" @click="removePublisher">Remove Publisher</b-button>
  </b-modal>
</template>

<script>
import axios from 'axios';
import LeagueMixin from '@/mixins/leagueMixin.js';

export default {
  mixins: [LeagueMixin],
  data() {
    return {
      publisherToRemove: null
    };
  },
  methods: {
    async removePublisher() {
      var model = {
        publisherID: this.publisherToRemove.publisherID
      };

      await axios.post('/api/leagueManager/RemovePublisher', model);
      this.notifyAction('Publisher ' + this.publisherToRemove.publisherName + ' has been removed from the league.');
      this.clearData();
    },
    clearData() {
      this.publisherToRemove = null;
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
