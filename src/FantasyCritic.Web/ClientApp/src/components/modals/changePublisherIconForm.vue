<template>
  <b-modal id="changePublisherIconForm" ref="changePublisherIconRef" title="Change Publisher Icon" @hidden="clearData" @show="clearData">
    <div v-show="!newPublisherIcon || !iconIsValid" class="alert alert-info">
      Your icon must be a single character, ideally an emoji.
      <br />
      <br />
      On mobile your keyboard probably has emoji built in, on desktop you can go to:
      <a href="https://emojipedia.org/" target="_blank">
        Emojipedia
        <font-awesome-icon icon="external-link-alt" size="sm" />
      </a>
    </div>
    <div class="form-horizontal">
      <div class="form-group">
        <label for="newPublisherIcon" class="control-label">Publisher Icon</label>
        <input id="newPublisherIcon" v-model="newPublisherIcon" name="newPublisherIcon" type="text" class="form-control input" />
      </div>
    </div>
    <template #modal-footer>
      <input type="submit" class="btn btn-primary" value="Change Icon" :disabled="!iconIsValid" @click="changePublisherIcon" />
    </template>
  </b-modal>
</template>
<script>
import axios from 'axios';
import GlobalFunctions from '@/globalFunctions';
import LeagueMixin from '@/mixins/leagueMixin';

export default {
  mixins: [LeagueMixin],
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
  mounted() {
    this.clearData();
  },
  methods: {
    changePublisherIcon() {
      var model = {
        publisherID: this.userPublisher.publisherID,
        publisherIcon: this.newPublisherIcon
      };
      axios
        .post('/api/league/changePublisherIcon', model)
        .then(() => {
          this.$refs.changePublisherIconRef.hide();
          this.notifyAction('Publisher icon changed.');
        })
        .catch(() => {});
    },
    clearData() {
      this.newPublisherIcon = this.userPublisher.publisherIcon;
    }
  }
};
</script>
