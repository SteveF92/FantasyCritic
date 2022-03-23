<template>
  <b-modal id="undoLastDraftActionModal" ref="undoLastDraftActionModalRef" title="Warning!" @ok="undoLastDraftAction">
    <p>
      Do you wish to undo the last draft action?
      <br />
      This will remove the game that was added most recently from it's publisher.
      <br />
      Only do this if there was a mistake.
    </p>
  </b-modal>
</template>
<script>
import axios from 'axios';
import LeagueMixin from '@/mixins/leagueMixin';

export default {
  mixins: [LeagueMixin],
  methods: {
    undoLastDraftAction() {
      this.$refs.undoLastDraftActionModalRef.hide();
      var model = {
        leagueID: this.league.leagueID,
        year: this.leagueYear.year
      };
      axios
        .post('/api/leagueManager/UndoLastDraftAction', model)
        .then(() => {
          this.notifyAction('Last action was undone.');
        })
        .catch(() => {});
    }
  }
};
</script>
