<template>
  <b-modal id="undo-last-draft-action-modal" title="Warning!" @ok="undoLastDraftAction">
    <p>
      Do you wish to undo the last draft action?
      <br />
      This will remove the game that was added most recently from its publisher. If the last publisher's turn was skipped, this will revert the draft to their turn.
      <br />
      Only do this if there was a mistake.
    </p>
  </b-modal>
</template>

<script lang="ts">
import { defineComponent } from 'vue';
import { mapState } from 'pinia';

import { leagueManagerClient, type UndoLastDraftActionRequest } from '@/api/clients';
import { useLeagueStore } from '@/store/leagueStore';
import { getApiErrorMessage } from '@/utilities/apiError';
import { makeToast } from '@/utilities/toast';

export default defineComponent({
  computed: {
    ...mapState(useLeagueStore, ['requiredLeagueYear', 'activeDraft'])
  },
  methods: {
    async undoLastDraftAction(): Promise<void> {
      if (!this.activeDraft) {
        return;
      }

      const request: UndoLastDraftActionRequest = {
        leagueID: this.requiredLeagueYear.league.leagueID,
        year: this.requiredLeagueYear.year,
        draftID: this.activeDraft.draftID
      };

      try {
        await leagueManagerClient.undoLastDraftAction(request);
        makeToast('Last action was undone.');
      } catch (error) {
        makeToast(getApiErrorMessage(error, 'Something went wrong when undoing the last draft action.'), 'danger');
      }
    }
  }
});
</script>
