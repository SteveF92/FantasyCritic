<template>
  <b-modal id="skipCurrentDraftPickModal" ref="skipCurrentDraftPickModalRef" title="Warning!" :ok-disabled="!skipConfirmed" @ok="skipCurrentDraftPick" @hidden="clearData">
    <p>
      Are you sure you want to skip the current pick?
    </p>
    <p v-if="activeDraft && activeDraft.nextPickPublisherName">
      This will skip
      <strong>{{ activeDraft.nextPickPublisherName }}</strong>'s turn
      (round {{ activeDraft.nextPickRoundNumber }},
      {{ activeDraft.nextPickIsCounterPick ? 'counter-pick' : 'standard game' }}).
    </p>
    <p>
      If you are sure, type
      <strong>SKIP TURN</strong>
      into the box below and click the OK button.
    </p>
    <input v-model="skipConfirmation" type="text" class="form-control input" />
  </b-modal>
</template>
<script>
import axios from 'axios';
import LeagueMixin from '@/mixins/leagueMixin.js';

export default {
  mixins: [LeagueMixin],
  data() {
    return {
      skipConfirmation: ''
    };
  },
  computed: {
    skipConfirmed() {
      return this.skipConfirmation.toUpperCase() === 'SKIP TURN';
    }
  },
  methods: {
    skipCurrentDraftPick() {
      this.skipConfirmation = '';
      this.$refs.skipCurrentDraftPickModalRef.hide();
      const model = {
        leagueID: this.league.leagueID,
        year: this.leagueYear.year,
        draftID: this.activeDraft.draftID
      };
      axios
        .post('/api/leagueManager/SkipCurrentDraftPick', model)
        .then(() => {
          this.notifyAction('Turn was skipped.', false);
        })
        .catch(() => {});
    },
    clearData() {
      this.skipConfirmation = '';
    }
  }
};
</script>
