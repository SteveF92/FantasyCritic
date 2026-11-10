<template>
  <b-card header-class="header">
    <template #header>
      <h2 v-if="processDate">
        <router-link :to="{ name: 'topBidsAndDrops', params: { processDate: processDate } }" title="Trending">Trending (Week ending {{ processDate }})</router-link>
      </h2>
      <h2 v-else>
        <router-link :to="{ name: 'topBidsAndDrops' }" title="Trending">Trending</router-link>
      </h2>
    </template>
    <div v-if="topBidsAndDrops.length === 0">No Bids and Drops Found.</div>
    <b-tabs v-if="topBidsAndDrops.length > 0" pills class="top-bids-and-drops-tabs">
      <b-tab title="Top Bids" title-item-class="tab-header">
        <b-table :items="topBids" :fields="standardBidFields" v-model:sort-by="standardSortBy" bordered striped responsive class="top-bids-drops-widget-table">
          <template #cell(masterGameYear)="data">
            <masterGamePopover :master-game="data.item.masterGameYear"></masterGamePopover>
          </template>
          <template #cell(totalStandardBidAmount)="data">
            {{ formatCurrency(data.item.totalStandardBidAmount, 0) }}
          </template>
        </b-table>
      </b-tab>
      <b-tab title="Top Counter Picks" title-item-class="tab-header">
        <b-table :items="topCounterPicks" :fields="counterPickFields" v-model:sort-by="counterPickSortBy" bordered striped responsive class="top-bids-drops-widget-table">
          <template #cell(masterGameYear)="data">
            <masterGamePopover :master-game="data.item.masterGameYear"></masterGamePopover>
          </template>
          <template #cell(totalCounterPickBidAmount)="data">
            {{ formatCurrency(data.item.totalCounterPickBidAmount, 0) }}
          </template>
        </b-table>
      </b-tab>
      <b-tab title="Top Drops" title-item-class="tab-header">
        <b-table :items="topDrops" :fields="dropFields" v-model:sort-by="dropSortBy" bordered striped responsive class="top-bids-drops-widget-table">
          <template #cell(masterGameYear)="data">
            <masterGamePopover :master-game="data.item.masterGameYear"></masterGamePopover>
          </template>
        </b-table>
      </b-tab>
    </b-tabs>
  </b-card>
</template>

<script lang="ts">
import { defineComponent, type PropType } from 'vue';
import { BTableSortBy, TableField } from 'bootstrap-vue-next';

import { type TopBidsAndDropsGameViewModel } from '@/api/clients';
import { orderByDescending } from '@/globalFunctions';
import { formatCurrency } from '@/utilities/formatters';
import MasterGamePopover from '@/components/masterGamePopover.vue';

interface TopBidsAndDropsWidgetData {
  standardSortBy: BTableSortBy[];
  counterPickSortBy: BTableSortBy[];
  dropSortBy: BTableSortBy[];
}

export default defineComponent({
  setup() {
    const standardBidFields: TableField<TopBidsAndDropsGameViewModel>[] = [
      { key: 'masterGameYear', label: 'Game', sortable: true, thClass: 'bg-primary' },
      { key: 'totalStandardBidCount', label: 'Bids', sortable: true, thClass: 'bg-primary' },
      { key: 'totalStandardBidAmount', label: 'Total $', sortable: true, thClass: 'bg-primary' }
    ];
    const counterPickFields: TableField<TopBidsAndDropsGameViewModel>[] = [
      { key: 'masterGameYear', label: 'Game', sortable: true, thClass: 'bg-primary' },
      { key: 'totalCounterPickBidCount', label: 'Bids', sortable: true, thClass: 'bg-primary' },
      { key: 'totalCounterPickBidAmount', label: 'Total $', sortable: true, thClass: 'bg-primary' }
    ];
    const dropFields: TableField<TopBidsAndDropsGameViewModel>[] = [
      { key: 'masterGameYear', label: 'Game', sortable: true, thClass: 'bg-primary' },
      { key: 'successfulDrops', label: 'Drops', sortable: true, thClass: 'bg-primary' }
    ];

    return { standardBidFields, counterPickFields, dropFields };
  },
  components: {
    MasterGamePopover
  },
  props: {
    topBidsAndDrops: { type: Array as PropType<TopBidsAndDropsGameViewModel[]>, required: true },
    processDate: { type: String, required: true }
  },
  data(): TopBidsAndDropsWidgetData {
    return {
      standardSortBy: [{ key: 'totalStandardBidCount', order: 'desc' }],
      counterPickSortBy: [{ key: 'totalCounterPickBidCount', order: 'desc' }],
      dropSortBy: [{ key: 'totalDropCount', order: 'desc' }]
    };
  },
  computed: {
    topBids(): TopBidsAndDropsGameViewModel[] {
      const relevant = this.topBidsAndDrops.filter((x) => x.totalStandardBidCount > 0);
      return orderByDescending(relevant, (x) => x.successfulStandardBids).slice(0, 25);
    },
    topCounterPicks(): TopBidsAndDropsGameViewModel[] {
      const relevant = this.topBidsAndDrops.filter((x) => x.totalCounterPickBidCount > 0);
      return orderByDescending(relevant, (x) => x.successfulCounterPickBids).slice(0, 25);
    },
    topDrops(): TopBidsAndDropsGameViewModel[] {
      const relevant = this.topBidsAndDrops.filter((x) => x.totalDropCount > 0);
      return orderByDescending(relevant, (x) => x.successfulDrops).slice(0, 25);
    }
  },
  methods: {
    formatCurrency
  }
});
</script>

<style scoped>
:deep(.header) {
  margin-bottom: 0;
  padding-bottom: 0;
}

.header h2 {
  font-size: 25px;
  margin-bottom: 0;
  padding-bottom: 0;
}

.spinner {
  display: flex;
  justify-content: space-around;
}

.top-bids-drops-widget-table {
  max-height: 500px;
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
