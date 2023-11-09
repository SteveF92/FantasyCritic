<template>
  <b-card :title="titleText">
    <b-tabs pills class="top-bids-and-drops-tabs">
      <b-tab title="Top Bids" title-item-class="tab-header"></b-tab>
      <b-tab title="Top Counter Picks" title-item-class="tab-header"></b-tab>
      <b-tab title="Top Drops" title-item-class="tab-header"></b-tab>
    </b-tabs>
    <b-card-text>For previous weeks, click here.</b-card-text>
  </b-card>
</template>

<script>
import axios from 'axios';
import _ from 'lodash';

export default {
  data() {
    return {
      processDate: null,
      topBidsAndDrops: [],
      errorInfo: null
    };
  },
  computed: {
    titleText() {
      if (!this.processDate) {
        return 'Trending';
      }

      return `Trending (Week ending ${this.processDate})`;
    },
    topBids() {
      let relevant = this.topBidsAndDrops.filter((x) => x.totalStandardBidCount > 0);
      return _.orderBy(relevant, ['successfulStandardBids'], ['desc']);
    },
    topCounterPicks() {
      let relevant = this.topBidsAndDrops.filter((x) => x.totalCounterPickBidCount > 0);
      return _.orderBy(relevant, ['successfulCounterPickBids'], ['desc']);
    },
    topDrops() {
      let relevant = this.topBidsAndDrops.filter((x) => x.totalDropCount > 0);
      return _.orderBy(relevant, ['successfulDrops'], ['desc']);
    }
  },
  async created() {
    try {
      const response = await axios.get('/api/game/GetTopBidsAndDrops');
      this.processDate = response.data.processDate;
      this.topBidsAndDrops = response.data.data;
    } catch (error) {
      this.errorInfo = error.response.data;
    }
  }
};
</script>
