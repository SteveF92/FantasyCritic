<template>
  <b-modal id="changePublisherNameForm" ref="changePublisherNameRef" title="Change Publisher Name" @hidden="clearData" @show="clearData">
    <div class="form-horizontal">
      <div class="form-group">
        <label for="newPublisherName" class="control-label">Publisher Name</label>
        <input v-model="newPublisherName" id="newPublisherName" name="newPublisherName" type="text" class="form-control input" />
      </div>
    </div>
    <div slot="modal-footer">
      <input type="submit" class="btn btn-primary" value="Change Name" v-on:click="changePublisherName" :disabled="!newPublisherName"/>
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
  props: ['publisher'],
  methods: {
    changePublisherName() {
      var model = {
        publisherID: this.publisher.publisherID,
        publisherName: this.newPublisherName
      };
      axios
        .post('/api/league/changePublisherName', model)
        .then(response => {
          this.$refs.changePublisherNameRef.hide();
          let actionInfo = {
            oldName: this.publisher.publisherName,
            newName: this.newPublisherName,
            fetchLeagueYear: true
          };
          this.$emit('publisherNameChanged', actionInfo);
        })
        .catch(response => {
        });
    },
    clearData() {
      this.newPublisherName = this.publisher.publisherName;
    }
  },
  mounted() {
    this.clearData();
  }
};
</script>
