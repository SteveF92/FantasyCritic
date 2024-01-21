<template>
  <b-modal id="editAutoDraftForm" ref="editAutoDraftFormRef" size="lg" title="Edit Auto Draft">
    <div class="alert alert-info">
      <p class="text-white">If "Auto Draft" is turned on, the site will select your games for you when it is your turn.</p>
      <p class="text-white">Games will be chosen from your watchlist first, and if there are no available games on your watchlist, the available game with the highest hype factor will be chosen.</p>
      <p class="text-white">For counterpicks, the game with the highest counterpick % site-wide will be chosen.</p>
      <p class="text-white">
        If you turn on the 'Standard Games Only' mode, then the site will not autodraft counter picks for you. You will need to make those selections yourself, as if auto draft was off.
      </p>
    </div>
    <b-form-select v-model="autoDraftMode" :options="autoDraftOptions"></b-form-select>

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
      autoDraftMode: null,
      autoDraftOptions: [
        { text: 'Off', value: 'Off' },
        { text: "Standard Games Only (Don't Auto Draft Counter Picks)", value: 'StandardGamesOnly' },
        { text: 'On (Including Counter Picks)', value: 'All' }
      ]
    };
  },
  mounted() {
    this.autoDraftMode = this.userPublisher.autoDraftMode;
  },
  methods: {
    setAutoDraft() {
      const model = {
        publisherID: this.userPublisher.publisherID,
        mode: this.autoDraftMode
      };
      axios
        .post('/api/league/setAutoDraft', model)
        .then(() => {
          this.$refs.editAutoDraftFormRef.hide();
          this.notifyAction('Auto draft set to ' + this.autoDraftMode);
        })
        .catch(() => {});
    }
  }
};
</script>
