<template>
  <b-modal id="changePublisherIconForm" ref="changePublisherIconRef" title="Change Publisher Name" @hidden="clearData" @show="clearData">
    <div v-show="!iconIsValid" class="alert alert-info">Your icon must be a single emoji.</div>
    <div class="form-horizontal">
      <div class="form-group">
        <label for="newPublisherIcon" class="control-label">Publisher Icon</label>
        <input v-model="newPublisherIcon" id="newPublisherIcon" name="newPublisherIcon" type="text" class="form-control input" />
      </div>
    </div>
    <div slot="modal-footer">
      <input type="submit" class="btn btn-primary" value="Change Icon" v-on:click="changePublisherIcon" :disabled="!iconIsValid" />
    </div>
  </b-modal>
</template>
<script>
import axios from 'axios';
import GlobalFunctions from '@/globalFunctions';

export default {
  data() {
    return {
      newPublisherIcon: '',
      errorInfo: ''
    };
  },
  props: ['publisher'],
  computed: {
    iconIsValid() {
      return GlobalFunctions.publisherIconIsValid(this.newPublisherIcon);
    }
  },
  methods: {
    changePublisherIcon() {
      var model = {
        publisherID: this.publisher.publisherID,
        publisherIcon: this.newPublisherIcon
      };
      axios
        .post('/api/league/changePublisherIcon', model)
        .then(response => {
          this.$refs.changePublisherIconRef.hide();
          let actionInfo = {
            oldName: this.publisher.publisherIcon,
            newName: this.newPublisherIcon,
            fetchLeagueYear: true
          };
          this.$emit('publisherIconChanged', actionInfo);
        })
        .catch(response => {
        });
    },
    clearData() {
      this.newPublisherIcon = this.publisher.publisherIcon;
    }
  },
  mounted() {
    this.clearData();
  }
};
</script>
