<template>
  <div class="col-md-10 offset-md-1 col-sm-12">
    <h1>Most Desired Reviews</h1>
    <p>These are the games that have the most interest in seeing more reviews written for them.</p>

    <b-form-group label="Released within" label-for="release-date-filter" label-size="sm" class="release-date-filter">
      <b-form-radio-group id="release-date-filter" v-model="releaseDateFilter" :options="releaseDateFilterOptions" buttons button-variant="outline-primary" size="sm" />
    </b-form-group>

    <div v-if="!isBusy">
      <b-table small bordered striped responsive :items="filteredMostDesiredReviews" :fields="gameFields" v-model:sort-by="sortBy">
        <template #cell(masterGame.gameName)="data">
          <masterGamePopover :master-game="data.item.masterGame"></masterGamePopover>
        </template>
        <template #cell(masterGame.maximumReleaseDate)="data">
          {{ formatDateLong(data.item.masterGame.maximumReleaseDate) }}
        </template>
        <template #cell(openCriticLink)="data">
          <a v-if="data.item.masterGame.openCriticID" :href="openCriticLink(data.item.masterGame)" target="_blank">
            <strong>
              OpenCritic
              <font-awesome-icon icon="external-link-alt" />
            </strong>
          </a>
          <span v-else>--</span>
        </template>
      </b-table>
    </div>

    <div v-else class="spinner">
      <font-awesome-icon icon="circle-notch" size="5x" spin :style="{ color: '#D6993A' }" />
    </div>
  </div>
</template>

<script lang="ts">
import { defineComponent } from 'vue';
import { type BTableSortBy, type TableField } from 'bootstrap-vue-next';
import { DateTime } from 'luxon';

import { gameClient, type MostDesiredReviewViewModel } from '@/api/clients';
import { formatDateLong } from '@/utilities/formatters';
import { openCriticLink } from '@/utilities/openCriticLinks';
import MasterGamePopover from '@/components/masterGamePopover.vue';

type ReleaseDateFilter = 'week' | 'month' | '3months' | 'year';

interface ReleaseDateFilterOption {
  text: string;
  value: ReleaseDateFilter;
}

interface MostDesiredReviewsData {
  mostDesiredReviews: MostDesiredReviewViewModel[];
  isBusy: boolean;
  releaseDateFilter: ReleaseDateFilter;
  sortBy: BTableSortBy[];
}

export default defineComponent({
  setup() {
    const releaseDateFilterOptions: ReleaseDateFilterOption[] = [
      { text: 'Past week', value: 'week' },
      { text: 'Past month', value: 'month' },
      { text: 'Past 3 months', value: '3months' },
      { text: 'All Year', value: 'year' }
    ];

    const gameFields: TableField<MostDesiredReviewViewModel>[] = [
      { key: 'masterGame.gameName', label: 'Name', sortable: true, thClass: 'bg-primary' },
      { key: 'masterGame.maximumReleaseDate', label: 'Release Date', sortable: true, thClass: 'bg-primary' },
      { key: 'openCriticLink', label: 'Open Critic Link', thClass: ['bg-primary', 'md-screen-minimum', 'position-relative'], tdClass: 'md-screen-minimum' },
      { key: 'desireFactor', label: 'Desire Factor', sortable: true, thClass: 'bg-primary' }
    ];

    return { releaseDateFilterOptions, gameFields };
  },
  components: {
    MasterGamePopover
  },
  data(): MostDesiredReviewsData {
    return {
      mostDesiredReviews: [],
      isBusy: true,
      releaseDateFilter: 'year',
      sortBy: [{ key: 'desireFactor', order: 'desc' }]
    };
  },
  computed: {
    filteredMostDesiredReviews(): MostDesiredReviewViewModel[] {
      if (this.releaseDateFilter === 'year') {
        return this.mostDesiredReviews;
      }

      const now = DateTime.local().startOf('day');
      let cutoff: DateTime;
      switch (this.releaseDateFilter) {
        case 'week':
          cutoff = now.minus({ weeks: 1 });
          break;
        case 'month':
          cutoff = now.minus({ months: 1 });
          break;
        case '3months':
          cutoff = now.minus({ months: 3 });
          break;
      }

      return this.mostDesiredReviews.filter((item) => {
        const releaseDateStr = item.masterGame.releaseDate ?? item.masterGame.maximumReleaseDate;
        const releaseDate = DateTime.fromISO(releaseDateStr);
        return releaseDate >= cutoff;
      });
    }
  },
  methods: {
    formatDateLong,
    openCriticLink
  },
  async created() {
    this.mostDesiredReviews = await gameClient.getMostDesiredReviews();
    this.isBusy = false;
  }
});
</script>

<style scoped>
.release-date-filter {
  margin-bottom: 1rem;
}

.spinner {
  display: flex;
  justify-content: space-around;
}
</style>
