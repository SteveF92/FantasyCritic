<template>
  <b-modal id="reset-draft-modal" title="Warning!" :ok-disabled="!resetConfirmed" @ok="resetDraft" @hidden="clearData">
    <p>Are you sure you want to reset the draft? Any games that have been drafted will be reset, and you will be able to change players/draft order/game settings again.</p>

    <p>
      If you are sure, type
      <strong>RESET DRAFT</strong>
      into the box below and click the OK button.
    </p>

    <input v-model="resetConfirmation" type="text" class="form-control input" />
  </b-modal>
</template>

<script lang="ts">
import { defineComponent } from 'vue';
import { mapState } from 'pinia';

import { leagueManagerClient, type ResetDraftRequest } from '@/api/clients';
import { useLeagueStore } from '@/store/leagueStore';
import { getApiErrorMessage } from '@/utilities/apiError';
import { makeToast } from '@/utilities/toast';

interface ResetDraftModalData {
  resetConfirmation: string;
}

export default defineComponent({
  data(): ResetDraftModalData {
    return {
      resetConfirmation: ''
    };
  },
  computed: {
    ...mapState(useLeagueStore, ['requiredLeagueYear', 'activeDraft']),
    resetConfirmed(): boolean {
      return this.resetConfirmation.toUpperCase() === 'RESET DRAFT';
    }
  },
  methods: {
    async resetDraft(): Promise<void> {
      if (!this.activeDraft) {
        return;
      }

      const request: ResetDraftRequest = {
        leagueID: this.requiredLeagueYear.league.leagueID,
        year: this.requiredLeagueYear.year,
        draftID: this.activeDraft.draftID
      };

      try {
        await leagueManagerClient.resetDraft(request);
        makeToast('Draft has been reset.');
      } catch (error) {
        makeToast(getApiErrorMessage(error, 'Something went wrong when resetting the draft.'), 'danger');
      }
    },
    clearData(): void {
      this.resetConfirmation = '';
    }
  }
});
</script>
