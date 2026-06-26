<template>
  <b-modal id="draftCompleteModal" ref="draftCompleteModalRef" title="Draft Complete!" ok-only>
    <!-- User not in league: short fallback -->
    <template v-if="!league.userIsInLeague">
      <p>The draft is complete!</p>
    </template>

    <!-- Multi-draft: non-final draft -->
    <template v-else-if="isMultiDraft && !isFinalDraft">
      <template v-if="leagueYear.enableBids">
        <p>[Lorem ipsum — draft N done, bids may be open between drafts, next draft coming up.]</p>
      </template>
      <template v-else>
        <p>[Lorem ipsum — draft N done, no bids between drafts, sit tight for the next draft.]</p>
      </template>
    </template>

    <!-- Multi-draft: final draft -->
    <template v-else-if="isMultiDraft && isFinalDraft">
      <template v-if="leagueYear.enableBids">
        <p>[Lorem ipsum — all drafts done, bids now open for the rest of the year.]</p>
      </template>
      <template v-else>
        <p>[Lorem ipsum — all drafts done, rosters locked for the year.]</p>
      </template>
    </template>

    <!-- Single draft, one-shot -->
    <template v-else-if="oneShotMode">
      <p>The draft is complete!</p>
    </template>

    <!-- Single draft, bids enabled -->
    <template v-else-if="leagueYear.enableBids">
      <p>The draft is complete! From here you can make bids for games that were not drafted, however, you may want to hold onto your available budget until later in the year!</p>
    </template>

    <!-- Single draft, bids disabled -->
    <template v-else>
      <p>The draft is complete!</p>
    </template>
  </b-modal>
</template>
<script>
import LeagueMixin from '@/mixins/leagueMixin.js';

export default {
  mixins: [LeagueMixin],
  computed: {
    isMultiDraft() {
      return (this.leagueYear?.drafts?.length ?? 0) >= 2;
    },
    isFinalDraft() {
      return this.pendingDraft === null;
    }
  },
  methods: {
    show() {
      this.$refs.draftCompleteModalRef.show();
    }
  }
};
</script>
