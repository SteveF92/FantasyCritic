<template>
  <div>
    <div class="col-md-10 offset-md-1 col-sm-12">
      <h1>Top Bids and Drops</h1>
      <hr />
      <h2 v-if="processDateInternal">For Week Ending {{ processDateInternal | longDate }}</h2>

      <b-tabs pills class="top-bids-and-drops-tabs">
        <b-tab title="Top Bids" title-item-class="tab-header">
          <b-table :items="topBids" :fields="standardBidFields" bordered striped responsive class="top-bids-drops-table">
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
    </div>
  </div>
</template>

<script>
import axios from 'axios';
import _ from 'lodash';
import MasterGamePopover from '@/components/masterGamePopover.vue';

export default {
  components: {
    MasterGamePopover
  },
  props: {
    processDate: { type: String, required: false }
  },
  data() {
    return {
      errorInfo: '',
      processDateInternal: null,
      topBidsAndDrops: [],
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
      this.processDateInternal = this.processDate;
      let requestPath = '/api/game/GetTopBidsAndDrops';
      if (this.processDateInternal) {
        requestPath = `/api/game/GetTopBidsAndDrops?processDate=${this.processDateInternal}`;
      }
      const response = await axios.get(requestPath);
      this.processDateInternal = response.data.processDate;
      this.topBidsAndDrops = response.data.data;
    } catch (error) {
      this.errorInfo = error.response.data;
    }
  }
};
</script>
<style scoped>
.top-bids-and-drops-tabs {
  margin-top: 20px;
}

div >>> .tab-header {
  margin-bottom: 5px;
}

div >>> .tab-header a {
  border-radius: 0px;
  font-weight: bolder;
  color: white;
}
</style>
