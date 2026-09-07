<template>
  <b-modal id="activeTradesModal" ref="activeTradesModalRef" size="xl" title="Active Trades" hide-footer>
    <div v-show="errorInfo" class="alert alert-danger" role="alert">
      {{ errorInfo }}
    </div>
    <div v-if="activeTrades.length === 0" class="alert alert-info" role="alert">There are no active trades for this league. To see past trades, check the League History page.</div>

    <tradeSummary v-for="activeTrade in activeTrades" :key="activeTrade.tradeID" :trade="activeTrade" default-visible></tradeSummary>
  </b-modal>
</template>

<script>
import LeagueMixin from '@/mixins/leagueMixin.js';
import TradeSummary from '@/components/tradeSummary.vue';

export default {
  components: {
    TradeSummary
  },
  mixins: [LeagueMixin],
  data() {
    return {
      errorInfo: ''
    };
  },
  computed: {
    activeTrades() {
      return this.leagueYear.activeTrades;
    }
  }
};
</script>
<style scoped>
.select-cell {
  text-align: center;
}

.priority-checkbox {
  float: right;
}
</style>
