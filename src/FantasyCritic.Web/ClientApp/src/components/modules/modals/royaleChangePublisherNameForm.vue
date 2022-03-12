<template>
  <b-modal id="royaleChangePublisherNameForm" ref="changeRoyalePublisherNameRef" title="Change Publisher Name" @hidden="clearData" @show="clearData">
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

export default {
  data() {
    return {
      newPublisherName: '',
      errorInfo: ''
    };
  },
  props: ['userRoyalePublisher'],
  methods: {
    changePublisherName() {
      var model = {
        publisherID: this.userRoyalePublisher.publisherID,
        publisherName: this.newPublisherName
      };
      axios
        .post('/api/royale/changePublisherName', model)
        .then((response) => {
          this.$refs.changeRoyalePublisherNameRef.hide();
          this.$emit('publisherNameChanged');
        })
        .catch((response) => {});
    },
    clearData() {
      this.newPublisherName = this.userRoyalePublisher.publisherName;
    }
  },
  mounted() {
    this.clearData();
  }
};
</script>
