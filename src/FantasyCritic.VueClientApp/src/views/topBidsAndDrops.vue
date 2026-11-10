<template>
  <div class="col-md-10 offset-md-1 col-sm-12">
    <h1>Top Bids and Drops</h1>
    <hr />

    <div v-if="processDateInternal && selectedYear && selectedMonth" class="header-area">
      <h2>For Week Ending {{ formatDateLong(processDateInternal) }}</h2>
      <div>
        <b-dropdown id="year-dropdown" :text="selectedYear.toString()">
          <b-dropdown-item v-for="year in years" :key="year" :active="selectedYear === year" @click="selectedYear = year">
            {{ year }}
          </b-dropdown-item>
        </b-dropdown>
        <b-dropdown id="month-dropdown" :text="convertMonth(selectedMonth)">
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

    <b-alert v-if="selectedYearWithinProcessDate && yearOptionsWithinProcessDate.length > 1" :model-value="true" variant="info">
      Since this date was at the end of the year, there was more than one season open when bids and drops were processed. You can choose to see the data for either year.
      <br />
      <label class="multiple-season-year-select-label">Select a year:</label>

      <b-dropdown id="year-within-process-date-dropdown" :text="selectedYearWithinProcessDate.toString()">
        <b-dropdown-item v-for="year in yearOptionsWithinProcessDate" :key="year" :active="selectedYearWithinProcessDate === year" @click="selectedYearWithinProcessDate = year">
          {{ year }}
        </b-dropdown-item>
      </b-dropdown>
    </b-alert>

    <b-alert v-if="errorInfo" variant="danger" :model-value="true">{{ errorInfo }}</b-alert>
    <div v-else-if="!topBidsAndDrops" class="spinner">
      <font-awesome-icon icon="circle-notch" size="5x" spin :style="{ color: '#D6993A' }" />
    </div>
    <div v-else>
      <b-tabs pills class="top-bids-and-drops-tabs">
        <b-tab title="Top Bids" title-item-class="tab-header">
          <b-table :items="topBids" :fields="standardBidFields" v-model:sort-by="standardSortBy" bordered striped responsive class="top-bids-drops-table">
            <template #cell(masterGameYear)="data">
              <masterGamePopover :master-game="data.item.masterGameYear"></masterGamePopover>
            </template>
            <template #cell(totalStandardBidAmount)="data">
              {{ formatCurrency(data.item.totalStandardBidAmount, 0) }}
            </template>
          </b-table>
        </b-tab>
        <b-tab title="Top Counter Picks" title-item-class="tab-header">
          <b-table :items="topCounterPicks" :fields="counterPickFields" v-model:sort-by="counterPickSortBy" bordered striped responsive>
            <template #cell(masterGameYear)="data">
              <masterGamePopover :master-game="data.item.masterGameYear"></masterGamePopover>
            </template>
            <template #cell(totalCounterPickBidAmount)="data">
              {{ formatCurrency(data.item.totalCounterPickBidAmount, 0) }}
            </template>
          </b-table>
        </b-tab>
        <b-tab title="Top Drops" title-item-class="tab-header">
          <b-table :items="topDrops" :fields="dropFields" v-model:sort-by="dropSortBy" bordered striped responsive>
            <template #cell(masterGameYear)="data">
              <masterGamePopover :master-game="data.item.masterGameYear"></masterGamePopover>
            </template>
          </b-table>
        </b-tab>
      </b-tabs>
    </div>
  </div>
</template>

<script lang="ts">
import { defineComponent } from 'vue';
import { type BTableSortBy, type TableField } from 'bootstrap-vue-next';
import { DateTime } from 'luxon';

import { gameClient, type TopBidsAndDropsGameViewModel } from '@/api/clients';
import { orderBy, orderByDescending } from '@/globalFunctions';
import { getApiErrorMessage } from '@/utilities/apiError';
import { formatCurrency, formatDateLong } from '@/utilities/formatters';
import MasterGamePopover from '@/components/masterGamePopover.vue';

interface TopBidsAndDropsData {
  errorInfo: string;
  selectedYear: number | null;
  selectedMonth: number | null;
  processDateInternal: string | null;
  processDateOptions: string[];
  allTopBidsAndDrops: Record<string, TopBidsAndDropsGameViewModel[]> | null;
  selectedYearWithinProcessDate: number | null;
  standardSortBy: BTableSortBy[];
  counterPickSortBy: BTableSortBy[];
  dropSortBy: BTableSortBy[];
}

export default defineComponent({
  setup() {
    const standardBidFields: TableField<TopBidsAndDropsGameViewModel>[] = [
      { key: 'masterGameYear', label: 'Game', sortable: true, thClass: 'bg-primary' },
      { key: 'totalStandardBidCount', label: 'Number of Bids', sortable: true, thClass: 'bg-primary' },
      { key: 'successfulStandardBids', label: 'Number Successful', sortable: true, thClass: 'bg-primary' },
      { key: 'failedStandardBids', label: 'Number Failed', sortable: true, thClass: 'bg-primary' },
      { key: 'totalStandardBidAmount', label: 'Total Bid $', sortable: true, thClass: 'bg-primary' }
    ];
    const counterPickFields: TableField<TopBidsAndDropsGameViewModel>[] = [
      { key: 'masterGameYear', label: 'Game', sortable: true, thClass: 'bg-primary' },
      { key: 'totalCounterPickBidCount', label: 'Number of Bids', sortable: true, thClass: 'bg-primary' },
      { key: 'successfulCounterPickBids', label: 'Number Successful', sortable: true, thClass: 'bg-primary' },
      { key: 'failedCounterPickBids', label: 'Number Failed', sortable: true, thClass: 'bg-primary' },
      { key: 'totalCounterPickBidAmount', label: 'Total Bid $', sortable: true, thClass: 'bg-primary' }
    ];
    const dropFields: TableField<TopBidsAndDropsGameViewModel>[] = [
      { key: 'masterGameYear', label: 'Game', sortable: true, thClass: 'bg-primary' },
      { key: 'totalDropCount', label: 'Total Attempted Drops', sortable: true, thClass: 'bg-primary' },
      { key: 'successfulDrops', label: 'Successful Drops', sortable: true, thClass: 'bg-primary' },
      { key: 'failedDrops', label: 'Failed Drops', sortable: true, thClass: 'bg-primary' }
    ];

    const months: number[] = Array.from({ length: 12 }, (_, i) => i + 1);

    return { standardBidFields, counterPickFields, dropFields, months };
  },
  components: {
    MasterGamePopover
  },
  props: {
    processDate: { type: String, required: false }
  },
  data(): TopBidsAndDropsData {
    return {
      errorInfo: '',
      selectedYear: null,
      selectedMonth: null,
      processDateInternal: null,
      processDateOptions: [],
      allTopBidsAndDrops: null,
      selectedYearWithinProcessDate: null,
      standardSortBy: [{ key: 'totalStandardBidCount', order: 'desc' }],
      counterPickSortBy: [{ key: 'totalCounterPickBidCount', order: 'desc' }],
      dropSortBy: [{ key: 'totalDropCount', order: 'desc' }]
    };
  },
  computed: {
    years(): number[] {
      const yearsList = this.processDateOptions.map((date) => DateTime.fromISO(date).year);
      const uniqueYearsList = [...new Set(yearsList)];
      const sortedYears = orderBy(uniqueYearsList, (x) => x);
      return sortedYears;
    },
    topBidsAndDrops(): TopBidsAndDropsGameViewModel[] | null {
      if (!this.allTopBidsAndDrops || this.selectedYearWithinProcessDate === null) {
        return null;
      }
      return this.allTopBidsAndDrops[this.selectedYearWithinProcessDate] ?? null;
    },
    yearOptionsWithinProcessDate(): number[] {
      if (!this.allTopBidsAndDrops) {
        return [];
      }
      return Object.keys(this.allTopBidsAndDrops).map((x) => Number(x));
    },
    topBids(): TopBidsAndDropsGameViewModel[] {
      if (!this.topBidsAndDrops) {
        return [];
      }
      const relevant = this.topBidsAndDrops.filter((x) => x.totalStandardBidCount > 0);
      return orderByDescending(relevant, (x) => x.successfulStandardBids).slice(0, 25);
    },
    topCounterPicks(): TopBidsAndDropsGameViewModel[] {
      if (!this.topBidsAndDrops) {
        return [];
      }
      const relevant = this.topBidsAndDrops.filter((x) => x.totalCounterPickBidCount > 0);
      return orderByDescending(relevant, (x) => x.successfulCounterPickBids).slice(0, 25);
    },
    topDrops(): TopBidsAndDropsGameViewModel[] {
      if (!this.topBidsAndDrops) {
        return [];
      }
      const relevant = this.topBidsAndDrops.filter((x) => x.totalDropCount > 0);
      return orderByDescending(relevant, (x) => x.successfulDrops).slice(0, 25);
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
    formatCurrency,
    formatDateLong,
    async initializePage() {
      try {
        this.allTopBidsAndDrops = null;
        this.selectedYearWithinProcessDate = null;
        this.processDateInternal = this.processDate ?? null;

        this.processDateOptions = await gameClient.getProcessingDatesForTopBidsAndDrops();

        const response = await gameClient.getTopBidsAndDrops(this.processDateInternal);
        this.allTopBidsAndDrops = response.data;
        this.processDateInternal = response.processDate;
        const processDate = DateTime.fromISO(response.processDate);
        this.selectedYear = processDate.year;
        this.selectedMonth = processDate.month;
        this.selectedYearWithinProcessDate = response.yearWithMostData;
      } catch (error) {
        this.errorInfo = getApiErrorMessage(error, 'Failed to load top bids and drops.');
      }
    },
    convertMonth(month: number): string {
      return DateTime.now().set({ month }).toFormat('MMMM');
    },
    getProcessingDatesForYearMonth(year: number, month: number): string[] {
      return this.processDateOptions.filter((date) => {
        const luxonDate = DateTime.fromISO(date);
        return luxonDate.year === year && luxonDate.month === month; // Luxon uses 1-indexed months
      });
    }
  }
});
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

div :deep(.tab-header) {
  margin-bottom: 5px;
}

div :deep(.tab-header .nav-link) {
  border-radius: 0px;
  font-weight: bolder;
  color: white;
}
</style>
