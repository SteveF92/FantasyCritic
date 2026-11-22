<template>
  <b-modal id="active-trades-modal" size="xl" title="Active Trades" no-footer>
    <div v-if="activeTrades.length === 0" class="alert alert-info" role="alert">There are no active trades for this league. To see past trades, check the League History page.</div>

    <tradeSummary v-for="activeTrade in activeTrades" :key="activeTrade.tradeID" :trade="activeTrade" default-visible></tradeSummary>
  </b-modal>
</template>

<script lang="ts">
import { defineComponent } from 'vue';
import { mapState } from 'pinia';

import { type TradeViewModel } from '@/api/clients';
import { useLeagueStore } from '@/store/leagueStore';
import TradeSummary from '@/components/tradeSummary.vue';

export default defineComponent({
  components: {
    TradeSummary
  },
  computed: {
    ...mapState(useLeagueStore, ['requiredLeagueYear']),
    activeTrades(): TradeViewModel[] {
      return this.requiredLeagueYear.activeTrades;
    }
  }
});
</script>
