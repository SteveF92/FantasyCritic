<template>
  <div>
    <div class="col-md-10 offset-md-1 col-sm-12">
      <h1>Top Bids and Drops</h1>
      <hr />

      <div v-if="processDateInternal && selectedYear && selectedMonthString" class="header-area">
        <h2>For Week Ending {{ processDateInternal | longDate }}</h2>
        <div>
          <b-dropdown id="year-dropdown" :text="selectedYear.toString()">
            <b-dropdown-item v-for="year in years" :key="year" :active="selectedYear === year" @click="selectedYear = year">
              {{ year }}
            </b-dropdown-item>
          </b-dropdown>
          <b-dropdown id="month-dropdown" :text="selectedMonthString">
            <b-dropdown-item v-for="month in months" :key="month" :active="selectedMonth === month" @click="selectedMonth = month">
              {{ convertMonth(month) }}
            </b-dropdown-item>
          </b-dropdown>
          <b-dropdown id="process-date-dropdown" :text="processDateInternal">
            <b-dropdown-item
              v-for="processDateOption in getProcessingDatesForYearMonth(selectedYear, selectedMonth)"
              :key="processDateOption"
              :active="processDateOption === processDateInternal"
              :to="{ name: 'topBidsAndDrops', params: { processDate: processDateOption } }">
              {{ processDateOption }}
            </b-dropdown-item>
          </b-dropdown>
        </div>
      </div>

      <b-alert v-if="yearOptionsWithinProcessDate && yearOptionsWithinProcessDate.length > 1" show variant="info">
        Since this date was at the end of the year, there was more than one season open when bids and drops were processed. You can choose to see the data for either year.
        <br />
        <label class="multiple-season-year-select-label">Select a year:</label>

        <b-dropdown id="year-dropdown" :text="selectedYearWithinProcessDate.toString()">
          <b-dropdown-item v-for="year in yearOptionsWithinProcessDate" :key="year" :active="selectedYearWithinProcessDate === year" @click="selectedYearWithinProcessDate = year">
            {{ year }}
          </b-dropdown-item>
        </b-dropdown>
      </b-alert>

      <div v-if="!this.topBidsAndDrops" class="spinner">
        <font-awesome-icon icon="circle-notch" size="5x" spin :style="{ color: '#D6993A' }" />
      </div>
      <div v-else>
        <b-tabs pills class="top-bids-and-drops-tabs">
          <b-tab title="Top Bids" title-item-class="tab-header">
            <b-table :items="topBids" :fields="standardBidFields" :sort-by.sync="standardSortBy" :sort-desc.sync="sortDesc" bordered striped responsive class="top-bids-drops-table">
              <template #cell(masterGameYear)="data">
                <masterGamePopover :master-game="data.item.masterGameYear"></masterGamePopover>
              </template>
              <template #cell(totalStandardBidAmount)="data">
                {{ data.item.totalStandardBidAmount | money(0) }}
              </template>
            </b-table>
          </b-tab>
          <b-tab title="Top Counter Picks" title-item-class="tab-header">
            <b-table :items="topCounterPicks" :fields="counterPickFields" :sort-by.sync="counterPickSortBy" :sort-desc.sync="sortDesc" bordered striped responsive>
              <template #cell(masterGameYear)="data">
                <masterGamePopover :master-game="data.item.masterGameYear"></masterGamePopover>
              </template>
              <template #cell(totalCounterPickBidAmount)="data">
                {{ data.item.totalCounterPickBidAmount | money(0) }}
              </template>
            </b-table>
          </b-tab>
          <b-tab title="Top Drops" title-item-class="tab-header">
            <b-table :items="topDrops" :fields="dropFields" :sort-by.sync="dropSortBy" :sort-desc.sync="sortDesc" bordered striped responsive>
              <template #cell(masterGameYear)="data">
                <masterGamePopover :master-game="data.item.masterGameYear"></masterGamePopover>
              </template>
            </b-table>
          </b-tab>
        </b-tabs>
      </div>
    </div>
  </div>
</template>

<script>
import axios from 'axios';
import moment from 'moment';
import MasterGamePopover from '@/components/masterGamePopover.vue';
import globalFunctions from '@/globalFunctions';

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
      selectedYear: null,
      selectedMonth: null,
      processDateInternal: null,
      processDateOptions: [],
      allTopBidsAndDrops: null,
      selectedYearWithinProcessDate: null,
      standardSortBy: 'totalStandardBidCount',
      counterPickSortBy: 'totalCounterPickBidCount',
      dropSortBy: 'totalDropCount',
      sortDesc: true,
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
      const yearsList = this.processDateOptions.map((date) => {
        return moment(date).year();
      });

      const uniqueYearsList = [...new Set(yearsList)];
      const sortedYears = globalFunctions.orderBy(uniqueYearsList, (x) => x);
      return sortedYears;
    },
    months() {
      return Array.from({ length: 12 }, (_, i) => i + 1);
    },
    selectedMonthString() {
      return this.convertMonth(this.selectedMonth);
    },
    topBidsAndDrops() {
      if (!this.allTopBidsAndDrops) {
        return null;
      }
      return this.allTopBidsAndDrops[this.selectedYearWithinProcessDate];
    },
    yearOptionsWithinProcessDate() {
      if (!this.allTopBidsAndDrops) {
        return null;
      }
      return Object.keys(this.allTopBidsAndDrops);
    },
    topBids() {
      let relevant = this.topBidsAndDrops.filter((x) => x.totalStandardBidCount > 0);
      return globalFunctions.orderByDescending(relevant, (x) => x.successfulStandardBids).slice(0, 25);
    },
    topCounterPicks() {
      let relevant = this.topBidsAndDrops.filter((x) => x.totalCounterPickBidCount > 0);
      return globalFunctions.orderByDescending(relevant, (x) => x.successfulCounterPickBids).slice(0, 25);
    },
    topDrops() {
      let relevant = this.topBidsAndDrops.filter((x) => x.totalDropCount > 0);
      return globalFunctions.orderByDescending(relevant, (x) => x.successfulDrops).slice(0, 25);
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
  methods: {
    async initializePage() {
      try {
        this.allTopBidsAndDrops = null;
        this.selectedYearWithinProcessDate = null;
        this.processDateInternal = this.processDate;

        const weeksResponse = await axios.get('/api/game/GetProcessingDatesForTopBidsAndDrops');
        this.processDateOptions = weeksResponse.data;

        let requestPath = '/api/game/GetTopBidsAndDrops';
        if (this.processDateInternal) {
          requestPath = `/api/game/GetTopBidsAndDrops?processDate=${this.processDateInternal}`;
        }
        const response = await axios.get(requestPath);

        this.processDateInternal = response.data.processDate;
        this.selectedYear = moment(this.processDateInternal).year();
        this.selectedMonth = moment(this.processDateInternal).month() + 1;
        this.allTopBidsAndDrops = response.data.data;
        this.selectedYearWithinProcessDate = response.data.yearWithMostData;
      } catch (error) {
        this.errorInfo = error.response.data;
      }
    },
    convertMonth(month) {
      if (!month) {
        return null;
      }

      return moment()
        .month(month - 1)
        .format('MMMM');
    },
    getProcessingDatesForYearMonth(year, month) {
      return this.processDateOptions.filter((date) => {
        const momentDate = moment(date);
        return momentDate.year() === year && momentDate.month() === month - 1; // Moment.js uses 0-indexed months
      });
    }
  }
};
</script>
<style scoped>
.header-area {
  display: flex;
  justify-content: space-between;
}

.spinner {
  display: flex;
  justify-content: space-around;
}

.multiple-season-year-select-label {
  margin-right: 5px;
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
