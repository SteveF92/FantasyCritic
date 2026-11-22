<template>
  <b-modal id="skip-current-draft-pick-modal" title="Warning!" :ok-disabled="!skipConfirmed" @ok="skipCurrentDraftPick" @hidden="clearData">
    <p>Are you sure you want to skip the current pick?</p>
    <p v-if="activeDraft && activeDraft.nextPickPublisherName">
      This will skip
      <strong>{{ activeDraft.nextPickPublisherName }}</strong>
      's turn (round {{ activeDraft.nextPickRoundNumber }}, {{ activeDraft.nextPickIsCounterPick ? 'counter-pick' : 'standard game' }}).
    </p>
    <p>
      If you are sure, type
      <strong>SKIP TURN</strong>
      into the box below and click the OK button.
    </p>
    <input v-model="skipConfirmation" type="text" class="form-control input" />
  </b-modal>
</template>

<script lang="ts">
import { defineComponent } from 'vue';
import { mapState } from 'pinia';

import { leagueManagerClient, type SkipCurrentDraftPickRequest } from '@/api/clients';
import { useLeagueStore } from '@/store/leagueStore';
import { getApiErrorMessage } from '@/utilities/apiError';
import { makeToast } from '@/utilities/toast';

interface SkipCurrentDraftPickModalData {
  skipConfirmation: string;
}

export default defineComponent({
  data(): SkipCurrentDraftPickModalData {
    return {
      skipConfirmation: ''
    };
  },
  computed: {
    ...mapState(useLeagueStore, ['requiredLeagueYear', 'activeDraft']),
    skipConfirmed(): boolean {
      return this.skipConfirmation.toUpperCase() === 'SKIP TURN';
    }
  },
  methods: {
    async skipCurrentDraftPick(): Promise<void> {
      if (!this.activeDraft) {
        return;
      }

      const request: SkipCurrentDraftPickRequest = {
        leagueID: this.requiredLeagueYear.league.leagueID,
        year: this.requiredLeagueYear.year,
        draftID: this.activeDraft.draftID
      };

      try {
        await leagueManagerClient.skipCurrentDraftPick(request);
        makeToast('Turn was skipped.');
      } catch (error) {
        makeToast(getApiErrorMessage(error, 'Something went wrong when skipping the current pick.'), 'danger');
      }
    },
    clearData(): void {
      this.skipConfirmation = '';
    }
  }
});
</script>
