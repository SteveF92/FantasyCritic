<template>
  <b-modal id="changePublisherNameForm" ref="changePublisherNameRef" title="Change Publisher Name" @hidden="clearData" @show="clearData">
    <div class="form-horizontal">
      <div class="form-group">
        <label for="newPublisherName" class="control-label">Publisher Name</label>
        <input id="newPublisherName" v-model="newPublisherName" name="newPublisherName" type="text" class="form-control input" />
      </div>
    </div>
    <template #modal-footer>
      <input type="submit" class="btn btn-primary" value="Change Name" :disabled="!newPublisherName" @click="changePublisherName" />
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
      newPublisherName: '',
      errorInfo: ''
    };
  },
  created() {
    this.clearData();
  },
  methods: {
    changePublisherName() {
      const model = {
        publisherID: this.userPublisher.publisherID,
        publisherName: this.newPublisherName.trim()
      };
      axios
        .post('/api/league/changePublisherName', model)
        .then(() => {
          this.$refs.changePublisherNameRef.hide();
          this.notifyAction('Publisher name changed from ' + this.userPublisher.publisherName + ' to ' + this.newPublisherName);
        })
        .catch(() => {});
    },
    clearData() {
      this.newPublisherName = this.userPublisher.publisherName;
    }
  }
};
</script>
