<template>
  <b-modal id="changePublisherSloganForm" ref="changePublisherSloganRef" title="Change Publisher Slogan" @hidden="clearData" @show="clearData">
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
import LeagueMixin from '@/mixins/leagueMixin.js';

export default {
  mixins: [LeagueMixin],
  data() {
    return {
      newPublisherSlogan: '',
      errorInfo: ''
    };
  },
  created() {
    this.clearData();
  },
  methods: {
    changePublisherSlogan() {
      const model = {
        publisherID: this.userPublisher.publisherID,
        publisherSlogan: this.newPublisherSlogan.trim()
      };
      axios
        .post('/api/league/changePublisherSlogan', model)
        .then(() => {
          this.$refs.changePublisherSloganRef.hide();
          this.notifyAction('Publisher slogan changed.');
        })
        .catch(() => {});
    },
    clearData() {
      this.newPublisherSlogan = this.userPublisher.publisherSlogan;
    }
  }
};
</script>
