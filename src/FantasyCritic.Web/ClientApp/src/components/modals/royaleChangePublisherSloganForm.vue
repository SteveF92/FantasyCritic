<template>
  <b-modal id="royaleChangePublisherSloganForm" ref="changeRoyalePublisherSloganRef" title="Change Publisher Slogan" @hidden="clearData" @show="clearData">
    <div class="alert alert-info">Your publisher slogan is a 'tagline' that you can choose to add to your publisher. Think of it like a "corporate motto".</div>
    <div class="form-horizontal">
      <div class="form-group">
        <label for="newPublisherSlogan" class="control-label">Publisher Slogan</label>
        <input id="newPublisherSlogan" v-model="newPublisherSlogan" name="newPublisherSlogan" type="text" class="form-control input" />
      </div>
    </div>
    <template #modal-footer>
      <input type="submit" class="btn btn-primary" value="Change Slogan" @click="changePublisherSlogan" />
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
      newPublisherSlogan: '',
      errorInfo: ''
    };
  },
  mounted() {
    this.clearData();
  },
  methods: {
    changePublisherSlogan() {
      const model = {
        publisherID: this.userRoyalePublisher.publisherID,
        publisherSlogan: this.newPublisherSlogan
      };
      axios
        .post('/api/royale/changePublisherSlogan', model)
        .then(() => {
          this.$refs.changeRoyalePublisherSloganRef.hide();
          this.$emit('publisherSloganChanged');
        })
        .catch(() => {});
    },
    clearData() {
      this.newPublisherSlogan = this.userRoyalePublisher.publisherSlogan;
    }
  }
};
</script>
