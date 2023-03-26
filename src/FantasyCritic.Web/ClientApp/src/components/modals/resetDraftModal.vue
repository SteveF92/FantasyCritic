<template>
  <b-modal id="resetDraftModal" ref="resetDraftModalRef" title="Warning!" :ok-disabled="!resetConfirmed" @ok="resetDraft" @hidden="clearData">
    <p>Are you sure you want to reset the draft? Any games that have been drafted will be reset, and you will be able to change players/draft order/game settings again.</p>

    <p>
      If you are sure, type
      <strong>RESET DRAFT</strong>
      into the box below and click the OK button.
    </p>

    <input v-model="resetConfirmation" type="text" class="form-control input" />
  </b-modal>
</template>
<script>
import axios from 'axios';
import LeagueMixin from '@/mixins/leagueMixin.js';

export default {
  mixins: [LeagueMixin],
  data() {
    return {
      resetConfirmation: ''
    };
  },
  computed: {
    resetConfirmed() {
      let upperCase = this.resetConfirmation.toUpperCase();
      return upperCase === 'RESET DRAFT';
    }
  },
  methods: {
    resetDraft() {
      this.resetConfirmation = '';
      this.$refs.resetDraftModalRef.hide();
      var model = {
        leagueID: this.league.leagueID,
        year: this.leagueYear.year
      };
      axios
        .post('/api/leagueManager/ResetDraft', model)
        .then(() => {
          this.notifyAction('Draft has been reset.');
        })
        .catch(() => {});
    },
    clearData() {
      this.resetConfirmation = '';
    }
  }
};
</script>
