<template>
  <b-modal id="createPublisher" ref="createPublisherRef" title="Create Publisher" @hidden="clearData">
    <div class="form-horizontal">
      <div class="form-group">
        <label for="publisherName" class="control-label">Publisher Name</label>
        <input id="publisherName" v-model="publisherName" name="publisherName" type="text" class="form-control input" />
      </div>
    </div>
    <div slot="modal-footer">
      <input type="submit" class="btn btn-primary" value="Create Publisher" :disabled="!publisherName" @click="createPublisher" />
    </div>
  </b-modal>
</template>
<script>
import axios from 'axios';
import LeagueMixin from '@/mixins/leagueMixin';

export default {
  mixins: [LeagueMixin],
  data() {
    return {
      publisherName: '',
      errorInfo: ''
    };
  },
  methods: {
    createPublisher() {
      var model = {
        leagueID: this.leagueYear.leagueID,
        year: this.leagueYear.year,
        publisherName: this.publisherName
      };
      axios
        .post('/api/league/createPublisher', model)
        .then(() => {
          this.$refs.createPublisherRef.hide();
          this.notifyAction(this.publisherName + ' created.');
          this.publisherName = '';
        })
        .catch(() => {});
    },
    clearData() {
      this.publisherName = '';
    }
  }
};
</script>
