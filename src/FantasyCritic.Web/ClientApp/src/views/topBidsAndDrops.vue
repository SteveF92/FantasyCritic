<template>
  <div>
    <div class="col-md-10 offset-md-1 col-sm-12">
      <div class="header-area">
        <h1>Top Bids and Drops</h1>
        <div v-if="processDateOptions">
          <b-dropdown id="dropdown-1" text="Select Week">
            <b-dropdown id="dropdown-2" v-for="year in years" :key="year" :text="`${year}`">
              <b-dropdown id="dropdown-3" v-for="month in months" :key="month" :text="`${month}`">
                <b-dropdown-item
                  v-for="processDateOption in getProcessingDatesForYearMonth(year, month)"
                  :key="processDateOption"
                  :active="processDateOption === processDateInternal"
                  :to="{ name: 'topBidsAndDrops', params: { processDate: processDateOption } }">
                  {{ processDateOption }}
                </b-dropdown-item>
              </b-dropdown>
            </b-dropdown>
          </b-dropdown>
        </div>
      </div>
      <hr />
      <h2 v-if="processDateInternal">For Week Ending {{ processDateInternal | longDate }}</h2>

      <b-tabs pills class="top-bids-and-drops-tabs">
        <b-tab title="Top Bids" title-item-class="tab-header">
          <b-table :items="topBids" :fields="standardBidFields" bordered striped responsive class="top-bids-drops-table">
            <template #cell(masterGameYear)="data">
              <masterGamePopover :master-game="data.item.masterGameYear"></masterGamePopover>
            </template>
            <template #cell(totalStandardBidAmount)="data">
              {{ data.item.totalStandardBidAmount | money(0) }}
            </template>
          </b-table>
        </b-tab>
        <b-tab title="Top Counter Picks" title-item-class="tab-header">
          <b-table :items="topCounterPicks" :fields="counterPickFields" bordered striped responsive>
            <template #cell(masterGameYear)="data">
              <masterGamePopover :master-game="data.item.masterGameYear"></masterGamePopover>
            </template>
            <template #cell(totalCounterPickBidAmount)="data">
              {{ data.item.totalCounterPickBidAmount | money(0) }}
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
import moment from 'moment';
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
      isDropdown2Visible: false,
      isDropdown3Visible: false,
      processDateInternal: null,
      processDateOptions: [],
      groupedProcessDates: [],
      topBidsAndDrops: [],
      standardBidFields: [
        { key: 'masterGameYear', label: 'Game', sortable: true, thClass: 'bg-primary' },
        { key: 'totalStandardBidCount', label: 'Number of Bids', sortable: true, thClass: 'bg-primary' },
        { key: 'successfulStandardBids', label: 'Number Successful', sortable: true, thClass: 'bg-primary' },
        { key: 'failedStandardBids', label: 'Number Failed', sortable: true, thClass: 'bg-primary' },
        { key: 'totalStandardBidAmount', label: 'Total Bid $', sortable: true, thClass: 'bg-primary' }
      ],
      counterPickFields: [
        { key: 'masterGameYear', label: 'Game', sortable: true, thClass: 'bg-primary' },
        { key: 'totalCounterPickBidCount', label: 'Number of Bids', sortable: true, thClass: 'bg-primary' },
        { key: 'successfulCounterPickBids', label: 'Number Successful', sortable: true, thClass: 'bg-primary' },
        { key: 'failedCounterPickBids', label: 'Number Failed', sortable: true, thClass: 'bg-primary' },
        { key: 'totalCounterPickBidAmount', label: 'Total Bid $', sortable: true, thClass: 'bg-primary' }
      ],
      dropFields: [
        { key: 'masterGameYear', label: 'Game', sortable: true, thClass: 'bg-primary' },
        { key: 'totalDropCount', label: 'Total Attempted Drops', sortable: true, thClass: 'bg-primary' },
        { key: 'successfulDrops', label: 'Successful Drops', sortable: true, thClass: 'bg-primary' },
        { key: 'failedDrops', label: 'Failed Drops', sortable: true, thClass: 'bg-primary' }
      ]
    };
  },
  computed: {
    years() {
      return Object.keys(this.groupedProcessDates).sort((a, b) => b - a);
    },
    months() {
      return Array.from({ length: 12 }, (_, i) => i + 1);
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
  watch: {
    async $route() {
      await this.initializePage();
    }
  },
  async created() {
    await this.initializePage();
  },
  mounted() {
    this.$root.$on('bv::dropdown::show', (bvEvent) => {
      if (bvEvent.componentId === 'dropdown-2') {
        this.isDropdown2Visible = true;
      } else if (bvEvent.componentId === 'dropdown-3') {
        this.isDropdown3Visible = true;
      }
    });
    this.$root.$on('bv::dropdown::hide', (bvEvent) => {
      if (bvEvent.componentId === 'dropdown-2') {
        this.isDropdown2Visible = false;
      } else if (bvEvent.componentId === 'dropdown-3') {
        this.isDropdown3Visible = false;
      }
      if (this.isDropdown2Visible || this.isDropdown3Visible) {
        bvEvent.preventDefault();
      }
    });
  },
  methods: {
    async initializePage() {
      try {
        this.processDateInternal = this.processDate;
        let requestPath = '/api/game/GetTopBidsAndDrops';
        if (this.processDateInternal) {
          requestPath = `/api/game/GetTopBidsAndDrops?processDate=${this.processDateInternal}`;
        }
        const response = await axios.get(requestPath);
        this.processDateInternal = response.data.processDate;
        this.topBidsAndDrops = response.data.data;

        const weeksResponse = await axios.get('/api/game/GetProcessingDatesForTopBidsAndDrops');
        this.processDateOptions = weeksResponse.data;
        this.groupedProcessDates = this.groupDatesByYearMonth(this.processDateOptions);
      } catch (error) {
        this.errorInfo = error.response.data;
      }
    },
    groupDatesByYearMonth(dateStrings) {
      const dates = dateStrings.map((dateString) => moment(dateString));
      const groupedByYear = _.groupBy(dates, (date) => date.year());

      const result = {};

      _.forEach(groupedByYear, (yearDates, year) => {
        result[year] = {};

        const groupedByMonth = _.groupBy(yearDates, (date) => date.month());

        _.forEach(groupedByMonth, (monthDates, month) => {
          const monthNumber = parseInt(month) + 1; // Months are zero-indexed in moment.js
          result[year][monthNumber.toString()] = monthDates.map((date) => date.format('YYYY-MM-DD'));
        });
      });

      return result;
    },
    getProcessingDatesForYearMonth(year, month) {
      const numberYear = Number(year);
      if (this.groupedProcessDates[numberYear] && this.groupedProcessDates[numberYear][month]) {
        return this.groupedProcessDates[numberYear][month];
      }
      return [];
    }
  }
};
</script>
<style scoped>
.header-area {
  display: flex;
  justify-content: space-between;
}
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
