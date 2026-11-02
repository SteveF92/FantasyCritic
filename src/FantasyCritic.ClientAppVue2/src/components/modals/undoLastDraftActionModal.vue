<template>
  <b-modal id="undoLastDraftActionModal" ref="undoLastDraftActionModalRef" title="Warning!" @ok="undoLastDraftAction">
    <p>
      Do you wish to undo the last draft action?
      <br />
      This will remove the game that was added most recently from its publisher. If the last publisher's turn was skipped, this will revert the draft to their turn.
      <br />
      Only do this if there was a mistake.
    </p>
  </b-modal>
</template>
<script>
import axios from 'axios';
import { mapState } from 'pinia';
import { useLeagueStore } from '@/store/leagueStore';
import { makeToast } from '@/utilities/toast';

export default {
  computed: {
    ...mapState(useLeagueStore, ['leagueYear', 'league', 'activeDraft'])
  },
  methods: {
    undoLastDraftAction() {
      this.$refs.undoLastDraftActionModalRef.hide();
      const model = {
        leagueID: this.league.leagueID,
        year: this.leagueYear.year,
        draftID: this.activeDraft.draftID
      };
      axios
        .post('/api/leagueManager/UndoLastDraftAction', model)
        .then(() => {
          makeToast('Last action was undone.');
        })
        .catch(() => {});
    }
  }
};
</script>
