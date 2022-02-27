<template>
  <b-modal id="activeTradesModal" ref="activeTradesModalRef" size="xl" title="Active Trades" @hidden="clearData" hide-footer>
    <div class="alert alert-danger" v-show="errorInfo" role="alert">
      {{errorInfo}}
    </div>
    <div class="alert alert-info" v-show="!activeTrades" role="alert">
      There are no active trades for this league. To see past trades, check the League History page.
    </div>

    <tradeSummary v-for="activeTrade in activeTrades" :trade="activeTrade" :leagueYear="leagueYear" :publisher="publisher"></tradeSummary>
  </b-modal>
</template>

<script>
import TradeSummary from '@/components/modules/tradeSummary';

export default {
  components: {
    TradeSummary
  },
  props: ['leagueYear', 'publisher'],
  data() {
    return {
      errorInfo: "",
    };
  },
  computed: {
    activeTrades() {
      return this.leagueYear.activeTrades;
    }
  },
  methods: {
    clearData() {

    }
  }
};
</script>
<style scoped>
  .add-game-button {
    width: 100%;
  }

  .select-cell {
    text-align: center;
  }

  .priority-checkbox {
    float: right;
  }
</style>
