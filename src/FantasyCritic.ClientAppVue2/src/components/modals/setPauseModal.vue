<template>
  <b-modal id="setPauseModal" ref="setPauseModalRef" title="Warning!" @ok="setPause">
    <p v-if="!draftIsPaused">Do you wish to pause the draft? No one will be able to draft any games while the draft is paused.</p>
    <p v-if="draftIsPaused">Do you wish to unpause the draft? Things will resume as normal.</p>
  </b-modal>
</template>
<script>
import axios from 'axios';
import { mapState } from 'pinia';
import { useLeagueStore } from '@/store/leagueStore';
import { makeToast } from '@/utilities/toast';

export default {
  computed: {
    ...mapState(useLeagueStore, ['leagueYear', 'league', 'activeDraft', 'draftIsPaused'])
  },
  methods: {
    async setPause() {
      let newPause = !this.draftIsPaused;
      const model = {
        leagueID: this.league.leagueID,
        year: this.leagueYear.year,
        draftID: this.activeDraft.draftID,
        pause: newPause
      };
      try {
        await axios.post('/api/leagueManager/SetDraftPause', model);
        let pauseMessage = 'Draft has been paused.';
        if (!newPause) {
          pauseMessage = 'Draft has been un-paused.';
        }
        makeToast(pauseMessage);
        this.$refs.setPauseModalRef.hide();
      } catch (error) {
        console.log(error);
      }
    }
  }
};
</script>
