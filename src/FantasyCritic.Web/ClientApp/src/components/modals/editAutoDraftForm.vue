<template>
  <b-modal id="editAutoDraftForm" ref="editAutoDraftFormRef" size="lg" title="Edit Auto Draft">
    <div class="alert alert-info">
      If "Auto Draft" is turned on, the site will select your games for you when it is your turn.
      <br />
      Games will be chosen from your watchlist first, and if there are no available games on your watchlist, the available game with the highest hype factor will be chosen.
      <br />
      For counterpicks, the game with the highest counterpick % site-wide will be chosen.
    </div>
    <b-form inline>
      <b-form-checkbox v-model="isAutoDraft" class="mb-2 mr-sm-2 mb-sm-0">Auto Draft</b-form-checkbox>
    </b-form>
    <template #modal-footer>
      <input type="submit" class="btn btn-primary" value="Set Auto Draft" @click="setAutoDraft" />
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
      isAutoDraft: null
    };
  },
  mounted() {
    this.isAutoDraft = this.userPublisher.autoDraft;
  },
  methods: {
    setAutoDraft() {
      var model = {
        publisherID: this.userPublisher.publisherID,
        autoDraft: this.isAutoDraft
      };
      axios
        .post('/api/league/setAutoDraft', model)
        .then(() => {
          this.$refs.editAutoDraftFormRef.hide();
          let autoDraftStatus = 'off.';
          if (this.isAutoDraft) {
            autoDraftStatus = 'on.';
          }
          this.notifyAction('Auto draft set to ' + autoDraftStatus);
        })
        .catch(() => {});
    }
  }
};
</script>
