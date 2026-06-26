<template>
  <b-modal id="draftCompleteModal" ref="draftCompleteModalRef" title="Draft Complete!" ok-only>
    <!-- User not in league: short fallback -->
    <template v-if="!league.userIsInLeague">
      <p>The draft is complete!</p>
    </template>

    <!-- Multi-draft -->
    <template v-else-if="isMultiDraft">
      <template v-if="leagueYear.enableBids">
        <p>
          This draft is complete! Your league is set up as a multi-draft league that allows bids between drafts, so think carefully about how you want to use your budget vs what games you might want
          to grab in the next draft!
        </p>
      </template>
      <template v-else>
        <p>This draft is complete! Your league is set up as a multi-draft league with no bids in between drafts, so you're good to sit tight until the next draft is scheduled!</p>
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
    }
  },
  methods: {
    show() {
      this.$refs.draftCompleteModalRef.show();
    }
  }
};
</script>
