<template>
  <b-modal id="set-pause-modal" title="Warning!" @ok="setPause">
    <p v-if="!draftIsPaused">Do you wish to pause the draft? No one will be able to draft any games while the draft is paused.</p>
    <p v-if="draftIsPaused">Do you wish to unpause the draft? Things will resume as normal.</p>
  </b-modal>
</template>

<script lang="ts">
import { defineComponent } from 'vue';
import { mapState } from 'pinia';

import { leagueManagerClient, type DraftPauseRequest } from '@/api/clients';
import { useLeagueStore } from '@/store/leagueStore';
import { getApiErrorMessage } from '@/utilities/apiError';
import { makeToast } from '@/utilities/toast';

export default defineComponent({
  computed: {
    ...mapState(useLeagueStore, ['requiredLeagueYear', 'draftIsPaused'])
  },
  methods: {
    async setPause(): Promise<void> {
      const request: DraftPauseRequest = {
        leagueID: this.requiredLeagueYear.league.leagueID,
        year: this.requiredLeagueYear.year,
        pause: !this.draftIsPaused
      };

      try {
        await leagueManagerClient.setDraftPause(request);
        makeToast(request.pause ? 'Draft has been paused.' : 'Draft has been un-paused.');
      } catch (error) {
        makeToast(getApiErrorMessage(error, 'Something went wrong when pausing or resuming the draft.'), 'danger');
      }
    }
  }
});
</script>
