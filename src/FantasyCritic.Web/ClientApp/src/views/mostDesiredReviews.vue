<template>
  <div class="col-md-10 offset-md-1 col-sm-12">
    <h1>Most Desired Reviews</h1>
    <p>These are the games that have the most interest in seeing more reviews written for them.</p>

    <b-form-group label="Released within" label-for="release-date-filter" label-size="sm" class="release-date-filter">
      <b-form-radio-group id="release-date-filter" v-model="releaseDateFilter" :options="releaseDateFilterOptions" buttons button-variant="outline-primary" size="sm" />
    </b-form-group>

    <b-table small bordered striped responsive :items="filteredMostDesiredReviews" :fields="gameFields" :sort-by.sync="sortBy" :sort-desc.sync="sortDesc">
      <template #cell(masterGame.gameName)="data">
        <masterGamePopover :master-game="data.item.masterGame"></masterGamePopover>
      </template>
      <template #cell(masterGame.maximumReleaseDate)="data">
        {{ data.item.masterGame.maximumReleaseDate | longDate }}
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
</template>

<script>
import axios from 'axios';
import { DateTime } from 'luxon';
import MasterGamePopover from '@/components/masterGamePopover.vue';

export default {
  components: {
    MasterGamePopover
  },
  data() {
    return {
      mostDesiredReviews: null,
      releaseDateFilter: 'year',
      releaseDateFilterOptions: [
        { text: 'Past week', value: 'week' },
        { text: 'Past month', value: 'month' },
        { text: 'Past 3 months', value: '3months' },
        { text: 'All Year', value: 'year' }
      ],
      sortBy: 'desireFactor',
      sortDesc: true,
      gameFields: [
        { key: 'masterGame.gameName', label: 'Name', sortable: true, thClass: 'bg-primary' },
        { key: 'masterGame.maximumReleaseDate', label: 'Release Date', sortable: true, thClass: 'bg-primary' },
        { key: 'openCriticLink', label: 'Open Critic Link', thClass: ['bg-primary', 'md-screen-minimum', 'position-relative'], tdClass: 'md-screen-minimum' },
        { key: 'desireFactor', label: 'Desire Factor', sortable: true, thClass: 'bg-primary' }
      ]
    };
  },
  computed: {
    filteredMostDesiredReviews() {
      if (!this.mostDesiredReviews) {
        return null;
      }

      if (this.releaseDateFilter === 'year') {
        return this.mostDesiredReviews;
      }

      const now = DateTime.local().startOf('day');
      let cutoff;
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
        default:
          return this.mostDesiredReviews;
      }

      return this.mostDesiredReviews.filter((item) => {
        const releaseDateStr = item.masterGame.releaseDate || item.masterGame.maximumReleaseDate;
        if (!releaseDateStr) {
          return false;
        }

        const releaseDate = DateTime.fromISO(releaseDateStr);
        return releaseDate >= cutoff;
      });
    }
  },
  methods: {
    openCriticLink(game) {
      return `https://opencritic.com/game/${game.openCriticID}/${game.openCriticSlug ?? 'b'}`;
    }
  },
  async created() {
    const response = await axios.get('/api/game/GetMostDesiredReviews');
    this.mostDesiredReviews = response.data;
  }
};
</script>

<style scoped>
.release-date-filter {
  margin-bottom: 1rem;
}
</style>
