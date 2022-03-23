<template>
  <b-modal id="changePublisherNameForm" ref="changePublisherNameRef" title="Change Publisher Name" @hidden="clearData" @show="clearData">
    <div class="form-horizontal">
      <div class="form-group">
        <label for="newPublisherName" class="control-label">Publisher Name</label>
        <input v-model="newPublisherName" id="newPublisherName" name="newPublisherName" type="text" class="form-control input" />
      </div>
    </div>
    <div slot="modal-footer">
      <input type="submit" class="btn btn-primary" value="Change Name" v-on:click="changePublisherName" :disabled="!newPublisherName" />
    </div>
  </b-modal>
</template>
<script>
import axios from 'axios';
import LeagueMixin from '@/mixins/leagueMixin';

export default {
  data() {
    return {
      newPublisherName: '',
      errorInfo: ''
    };
  },
  mixins: [LeagueMixin],
  methods: {
    changePublisherName() {
      var model = {
        publisherID: this.userPublisher.publisherID,
        publisherName: this.newPublisherName
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
  },
  mounted() {
    this.clearData();
  }
};
</script>
