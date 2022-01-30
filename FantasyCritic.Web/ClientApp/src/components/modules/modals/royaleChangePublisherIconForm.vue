<template>
  <b-modal id="royaleChangePublisherIconForm" ref="changeRoyalePublisherIconRef" title="Change Publisher Icon" @hidden="clearData" @show="clearData">
    <div v-show="!newPublisherIcon || !iconIsValid" class="alert alert-info">
      Your icon must be a single character, ideally an emoji.
      <br />
      <br />
      On mobile your keyboard probably has emoji built in, on desktop you can go to:
      <a href="https://emojipedia.org/" target="_blank">Emojipedia <font-awesome-icon icon="external-link-alt" size="sm" /></a>
    </div>
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
  computed: {
    iconIsValid() {
      return GlobalFunctions.publisherIconIsValid(this.newPublisherIcon);
    }
  },
  props: ['userRoyalePublisher'],
  methods: {
    changePublisherIcon() {
      var model = {
        publisherID: this.userRoyalePublisher.publisherID,
        publisherIcon: this.newPublisherIcon
      };
      axios
        .post('/api/royale/changePublisherIcon', model)
        .then(response => {
          this.$refs.changeRoyalePublisherIconRef.hide();
          this.$emit('publisherIconChanged');
        })
        .catch(response => {
        });
    },
    clearData() {
      this.newPublisherIcon = this.userRoyalePublisher.publisherIcon;
    }
  },
  mounted() {
    this.clearData();
  }
};
</script>
