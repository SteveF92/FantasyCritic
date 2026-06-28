<template>
  <div>
    <div v-if="activeDraft?.draftIsPaused">
      <div class="alert alert-danger">
        <div v-if="!league.isManager">The draft has been paused. Speak to your league manager for details.</div>
        <div v-else>
          The draft has been paused. You can undo games that have been drafted.
          <b-button v-b-modal="'setPauseModal'" variant="success">Resume Draft</b-button>
        </div>
      </div>
    </div>
    <div v-if="activeDraft?.draftIsActive && nextPublisherUp">
      <div v-if="!userIsNextInDraft">
        <div class="alert alert-info">
          <div v-show="!activeDraft?.draftingCounterPicks">The draft is currently in progress!</div>
          <div v-show="activeDraft?.draftingCounterPicks">It's time to draft Counter Picks!</div>
          <div>
            Next to draft:
            <strong>{{ nextPublisherUp.publisherName }}</strong>
          </div>
          <div v-if="skippedPicksMessage" class="text-muted small mt-1">{{ skippedPicksMessage }}</div>
          <div v-if="league.isManager">To select the next player's game for them, Select 'Select Next Game' under 'Draft Management' in the sidebar!</div>
        </div>
      </div>
      <div v-else>
        <div class="alert alert-success draft-header">
          <div>
            <div v-show="!activeDraft?.draftingCounterPicks">The draft is currently in progress!</div>
            <div v-show="activeDraft?.draftingCounterPicks">It's time to draft counter picks!</div>
            <div><strong>It is your turn to draft!</strong></div>
            <div v-if="skippedPicksMessage" class="text-muted small mt-1">{{ skippedPicksMessage }}</div>
          </div>
          <div v-if="!activeDraft?.draftingCounterPicks">
            <b-button v-b-modal="'playerDraftGameForm'" variant="primary">Draft Game</b-button>
          </div>
          <div v-else>
            <b-button v-b-modal="'playerDraftCounterPickForm'" variant="primary">Draft Counter Pick</b-button>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script>
import LeagueMixin from '@/mixins/leagueMixin.js';

export default {
  mixins: [LeagueMixin],
  computed: {
    skippedPicksMessage() {
      const skips = this.activeDraft?.skippedPicksSinceLastRealPick;
      if (!skips || skips.length === 0) return null;

      const counts = {};
      for (const skip of skips) {
        counts[skip.publisherName] = (counts[skip.publisherName] || 0) + 1;
      }

      const distinctNames = Object.keys(counts);

      if (distinctNames.length === 1) {
        const name = distinctNames[0];
        const count = counts[name];
        const hasManual = skips.some(s => s.publisherName === name && s.isManualSkip);

        if (hasManual) {
          return `${name}'s pick was skipped by the league manager.`;
        }

        if (count === 1) {
          return `${name}'s pick was auto-skipped because they have no open slots.`;
        }

        const timesWord = count === 2 ? 'twice' : `${count} times`;
        return `${name}'s pick was auto-skipped ${timesWord} because they have no open slots.`;
      }

      const nameList = distinctNames.join(', ');
      return `The following players had their draft picks skipped: ${nameList}. See the History page for more information.`;
    }
  }
};
</script>

<style scoped>
.draft-header {
  display: flex;
  gap: 20px;
}
</style>
