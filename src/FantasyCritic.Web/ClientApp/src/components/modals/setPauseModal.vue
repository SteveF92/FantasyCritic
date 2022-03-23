<template>
  <b-modal id="setPauseModal" ref="setPauseModalRef" title="Warning!" @ok="setPause">
    <p v-if="!draftIsPaused">Do you wish to pause the draft? No one will be able to draft any games while the draft is paused.</p>
    <p v-if="draftIsPaused">Do you wish to unpause the draft? Things will resume as normal.</p>
  </b-modal>
</template>
<script>
import axios from 'axios';
import LeagueMixin from '@/mixins/leagueMixin';

export default {
  mixins: [LeagueMixin],
  methods: {
    setPause() {
      this.$refs.setPauseModalRef.hide();
      const newPause = !this.draftIsPaused;
      var model = {
        leagueID: this.league.leagueID,
        year: this.leagueYear.year,
        pause: newPause
      };
      axios
        .post('/api/leagueManager/SetDraftPause', model)
        .then(() => {
          let pauseMessage = 'Draft has been paused.';
          if (!newPause) {
            pauseMessage = 'Draft has been un-paused.';
          }
          this.notifyAction(pauseMessage);
        })
        .catch(() => {});
    }
  }
};
</script>
