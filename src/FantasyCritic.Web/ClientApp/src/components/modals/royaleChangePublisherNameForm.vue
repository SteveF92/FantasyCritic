<template>
  <b-modal id="royaleChangePublisherNameForm" ref="changeRoyalePublisherNameRef" title="Change Publisher Name" @hidden="clearData" @show="clearData">
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

export default {
  props: {
    userRoyalePublisher: { type: Object, required: true }
  },
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
        publisherID: this.userRoyalePublisher.publisherID,
        publisherName: this.newPublisherName
      };
      axios
        .post('/api/royale/changePublisherName', model)
        .then(() => {
          this.$refs.changeRoyalePublisherNameRef.hide();
          this.$emit('publisherNameChanged');
        })
        .catch(() => {});
    },
    clearData() {
      this.newPublisherName = this.userRoyalePublisher.publisherName;
    }
  }
};
</script>
