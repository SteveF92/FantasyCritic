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
          <div v-if="league.isManager">To select the next player's game for them, Select 'Select Next Game' under 'Draft Management' in the sidebar!</div>
        </div>
      </div>
      <div v-else>
        <div class="alert alert-success draft-header">
          <div>
            <div v-show="!activeDraft?.draftingCounterPicks">The draft is currently in progress!</div>
            <div v-show="activeDraft?.draftingCounterPicks">It's time to draft counter picks!</div>
            <div><strong>It is your turn to draft!</strong></div>
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
  mixins: [LeagueMixin]
};
</script>

<style scoped>
.draft-header {
  display: flex;
  gap: 20px;
}
</style>
