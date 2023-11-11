<template>
  <b-card :title="titleText">
    <b-tabs pills class="top-bids-and-drops-tabs">
      <b-tab title="Top Bids" title-item-class="tab-header">
        <b-table :items="topBids" :fields="standardBidFields" bordered striped responsive class="top-bids-drops-widget-table">
          <template #cell(masterGameYear)="data">
            <masterGamePopover :master-game="data.item.masterGameYear"></masterGamePopover>
          </template>
        </b-table>
      </b-tab>
      <b-tab title="Top Counter Picks" title-item-class="tab-header">
        <b-table :items="topCounterPicks" :fields="counterPickFields" bordered striped responsive>
          <template #cell(masterGameYear)="data">
            <masterGamePopover :master-game="data.item.masterGameYear"></masterGamePopover>
          </template>
        </b-table>
      </b-tab>
      <b-tab title="Top Drops" title-item-class="tab-header">
        <b-table :items="topDrops" :fields="dropFields" bordered striped responsive>
          <template #cell(masterGameYear)="data">
            <masterGamePopover :master-game="data.item.masterGameYear"></masterGamePopover>
          </template>
        </b-table>
      </b-tab>
    </b-tabs>
  </b-card>
</template>

<script>
import axios from 'axios';
import _ from 'lodash';
import MasterGamePopover from '@/components/masterGamePopover.vue';

export default {
  components: {
    MasterGamePopover
  },
  data() {
    return {
      processDate: null,
      topBidsAndDrops: [],
      errorInfo: null,
      standardBidFields: [
        { key: 'masterGameYear', label: 'Game', sortable: true, thClass: 'bg-primary' },
        { key: 'totalStandardBidCount', label: 'Bids', sortable: true, thClass: 'bg-primary' },
        { key: 'totalStandardBidAmount', label: 'Total $', sortable: true, thClass: 'bg-primary' }
      ],
      counterPickFields: [
        { key: 'masterGameYear', label: 'Game', sortable: true, thClass: 'bg-primary' },
        { key: 'totalCounterPickBidCount', label: 'Bids', sortable: true, thClass: 'bg-primary' },
        { key: 'totalCounterPickBidAmount', label: 'Total $', sortable: true, thClass: 'bg-primary' }
      ],
      dropFields: [
        { key: 'masterGameYear', label: 'Game', sortable: true, thClass: 'bg-primary' },
        { key: 'successfulDrops', label: 'Drops', sortable: true, thClass: 'bg-primary' }
      ]
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
      return _.orderBy(relevant, ['successfulStandardBids'], ['desc']).slice(0, 25);
    },
    topCounterPicks() {
      let relevant = this.topBidsAndDrops.filter((x) => x.totalCounterPickBidCount > 0);
      return _.orderBy(relevant, ['successfulCounterPickBids'], ['desc']).slice(0, 25);
    },
    topDrops() {
      let relevant = this.topBidsAndDrops.filter((x) => x.totalDropCount > 0);
      return _.orderBy(relevant, ['successfulDrops'], ['desc']).slice(0, 25);
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

<style scoped>
.top-bids-drops-widget-table {
  max-height: 500px;
}
</style>
